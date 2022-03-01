using System;
using System.Collections.Generic;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IVirtualCursor :IMonoEnable, IMonoStart
{
    RectTransform CursorRect { get; }
    bool CanMoveVirtualCursor { get; }
    void PreStartMovement();
    void Update();
}

public interface ICursorSettings
{
    RectTransform CursorRect { get; }
    float Speed { get; }
    Vector3 Position { get; }
}

public interface IVcSettings : IParameters
{
    VirtualCursorSettings VCSettings { get; }
}

[Serializable]
public class VirtualCursor : IEZEventUser, IVirtualCursor, ICursorSettings, IServiceUser, IMonoAwake, IVcSettings
{
    public VirtualCursor(IVirtualCursorSettings settings)
    {
        _parentTransform = settings.GetParentTransform;
        OnAwake();
    }
    
    //Variables
    private Canvas _cursorCanvas;
    private IInteractWithUi _interactWithUi;
    private IMoveVirtualCursor _moveVirtualCursor;
    private ICanvasOrderData _canvasOrderData;
    private Transform _parentTransform;
    private List<IRaycast> _gouiCasts = new List<IRaycast>();
    private IDataHub _myDataHub;

    //Properties & Setters / Getters
    public VirtualCursorSettings VCSettings => Scheme.ReturnVirtualCursorSettings;
    public bool SelectPressed => Scheme.PressSelect();
    public Vector3 Position => CursorRect.transform.position;
    public RectTransform CursorRect { get; private set; }
    public float Speed => VCSettings.CursorSpeed;
    private GameObject VirtualCursorPrefab => VCSettings.VirtualCursorPrefab;
    private InputScheme Scheme { get; set; }
    private bool HasInput => Scheme.VcHorizontal() != 0 || Scheme.VcVertical() != 0;
    private bool Allow2D => VCSettings.RestrictRaycastTo == GameType._2D 
                            || VCSettings.RestrictRaycastTo == GameType.NoRestrictions;
    private bool Allow3D => VCSettings.RestrictRaycastTo == GameType._3D 
                            || VCSettings.RestrictRaycastTo == GameType.NoRestrictions;

    private bool ShowCursorOnStart => (Scheme.ControlType == ControlMethod.MouseOnly
                                       || Scheme.ControlType == ControlMethod.AllowBothStartWithMouse) 
                                        || !Scheme.HideMouseCursor;
    public bool CanMoveVirtualCursor => HasInput && !SelectPressed && !_myDataHub.AllowKeys;

    private void SaveAllowKeys(IAllowKeys args)
    {
        ShowVC_Cursor();
        _interactWithUi.ClearLastHit();
    }
    private void CancelPressed(ICancelPressed args) => _interactWithUi.ClearLastHit();

    //Main
    public void OnAwake()
    {
        _moveVirtualCursor = EZInject.Class.NoParams<IMoveVirtualCursor>();
        _interactWithUi = EZInject.Class.NoParams<IInteractWithUi>();
    }

    public void UseEZServiceLocator()
    {
        Scheme = EZService.Locator.Get<InputScheme>(this);
        _canvasOrderData = EZService.Locator.Get<ICanvasOrderData>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    public void OnEnable()
    {
        UseEZServiceLocator();
        ObserveEvents();
        _interactWithUi.OnEnable();
        _moveVirtualCursor.OnEnable();
    }

    public void ObserveEvents()
    {
        InputEvents.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        InputEvents.Do.Subscribe<ICancelPressed>(CancelPressed);
    }

    public void UnObserveEvents() { }

    public void OnStart()
    {
        SetUpVirtualCursor(_parentTransform);
        _moveVirtualCursor.OnStart();
        SetStartingCanvasOrder();
        SetUpGouiRaycasts();
        SetCursorForStartUp();
    }

    private void SetUpVirtualCursor(Transform transform)
    {
        var newVirtualCursor = Object.Instantiate(VirtualCursorPrefab, transform, true);
        CursorRect = newVirtualCursor.GetComponent<RectTransform>();
        CursorRect.anchoredPosition3D = Vector3.zero;
        SetUpCursorCanvas();
    }
    
    private void SetUpCursorCanvas()
    {
        _cursorCanvas = CursorRect.GetComponent<Canvas>();
        _cursorCanvas.enabled = Scheme.CanUseVirtualCursor;
    }

    private void SetUpGouiRaycasts()
    {
        if (Allow2D)
             _gouiCasts.Add(EZInject.Class.WithParams<I2DRaycast>(this));

        if (Allow3D)
            _gouiCasts.Add(EZInject.Class.WithParams<I3DRaycast>(this));
    }

    private void SetCursorForStartUp() => _cursorCanvas.enabled = ShowCursorOnStart;

    private void ShowVC_Cursor()
    {
        if (_myDataHub.AllowKeys)
        {
            TurnOffAndResetCursor();
        }
        else
        {
            ActivateCursor();
        }
    }

    private void SetStartingCanvasOrder() 
        => SetCanvasOrderUtil.Set(_canvasOrderData.ReturnVirtualCursorCanvasOrder, _cursorCanvas);

    private void TurnOffAndResetCursor()
    {
        if (Scheme.HideMouseCursor)
            _cursorCanvas.enabled = false;
        
        foreach (var gOuiCast in _gouiCasts)
        {
            gOuiCast.WhenInMenu();
        }
    }

    private void ActivateCursor()
    {
        _cursorCanvas.enabled = true;
        DoRaycasts();
    }
    
    public void Update()
    {
        _moveVirtualCursor.Move(this);
        if (!HasInput) return;
        DoRaycasts();
    }

    private void DoRaycasts()
    {
        
        //TODO add a timer to restrict raycast to every other frame or user set value
        _interactWithUi.CheckIfCursorOverUI(this);
        if (_interactWithUi.OverNothingUI())
        {
            foreach (var gOuiCast in _gouiCasts)
            {
                gOuiCast.ExitLastObject();
            }
            return;
        }
        CheckIfCursorOverGOUI();
    }

    public void PreStartMovement()
    {
        if(_myDataHub.SceneStarted) return;
        if (!HasInput)
            _moveVirtualCursor.Move(this);
    }

    private void CheckIfCursorOverGOUI()
    {
        foreach (var gOuiCast in _gouiCasts)
        {
            if(gOuiCast.DoRaycast(CursorRect.position)) return;
        }
    }
}
