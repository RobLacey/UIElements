using System;
using EZ.Service;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class CursorSettings
{
    [SerializeField] 
    [ShowIf("CustomCursor")] 
    private Texture2D _cursor = default;

    [SerializeField] 
    [ShowIf("CustomCursor")] 
    private Vector2 _hotSpot =default;

    public Texture2D CursorTexture => _cursor;
    public Vector2 HotSpot => _hotSpot;
}

public abstract class InputScheme : ScriptableObject, IIsAService
{
    [Space(EditorSpace, order = 0)]
    
    [SerializeField] 
    [DisableIf(IsPlaying)]
    protected ControlMethod _mainControlType = ControlMethod.MouseOnly;

    [Header("Mouse and Cursor Settings")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]

    [SerializeField]
    [Label("Hide Cursor When Keys Active")]
    private IsActive _hideMouseCursor = IsActive.No;
    
    [SerializeField] 
    [HideIf(EConditionOperator.Or, KeysOnly, VirtualCursor)]
    private IsActive _customMouseCursor = IsActive.No;

    [SerializeField] 
    [HideIf(EConditionOperator.Or, KeysOnly, VirtualCursor)]
    [EnableIf(UseCustomCursor)]
    private CursorSettings _cursorSettings = default;

    [SerializeField] 
    [ShowIf(KeysOnly)]
    protected InGameSystem _inGameMenuSystem = InGameSystem.Off;
    
    [SerializeField]
    [Header("Virtual Cursor", order = 2)][HorizontalLine(1f, EColor.Blue, order = 3)]
    [Space(EditorSpace)]
    [DisableIf(EConditionOperator.Or, IsPlaying, UseCustomCursor)]
    private VirtualControl _useVirtualCursor = VirtualControl.No;

    [SerializeField] 
    [ShowIf(VirtualCursor)] 
    private VirtualCursorSettings _virtualCursorSettings;
    
    [SerializeField] [Space(EditorSpace)] [DisableIf(IsPlaying)]
    protected InMenuOrGame _startGameWhere = InMenuOrGame.InGameControl;
    
    [Header("Cancel / Back Settings")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    
    [SerializeField] 
    [Label("Nothing to Cancel Action")] 
    protected PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
    
    [SerializeField]
    private PauseFunction _globalEscapeFunction;
    

    public void Awake()
    {
        if(!Application.isPlaying) return; 
        AddService();
        SetCursor();
    }

    public void AddService()
    {
        EZService.Locator.AddNew<InputScheme>(this);
    }

    public void OnRemoveService() { }

    //Variables
    protected Vector3 _virtualCursorPosition;

    private enum PauseFunction { DoNothing, BackOneLevel, BackToHome }

    
    //Editor
    private bool InGameOn => _inGameMenuSystem == InGameSystem.On;
    private bool CustomCursor => _customMouseCursor == IsActive.Yes;
    private const string UseCustomCursor = nameof(CustomCursor);
    private bool UseVirtualCursor => _useVirtualCursor == VirtualControl.Yes;
    private const string VirtualCursor = nameof(UseVirtualCursor);
    private const int EditorSpace = 20;
    private static bool AppIsPlaying => Application.isPlaying;
    private const string IsPlaying = nameof(AppIsPlaying);
    private const string KeysOnly = nameof(KeyboardOnly);
    private bool KeyboardOnly
    {
        get{
            var keysOnly = _mainControlType == ControlMethod.KeysOrControllerOnly;

            if (!keysOnly)
            {
                _inGameMenuSystem = InGameSystem.Off;
            }
            else
            {
                _customMouseCursor = IsActive.No;
            }
            return keysOnly;
        }
    }

    private void SetCursor()
    {
        if (_customMouseCursor == IsActive.Yes && !KeyboardOnly)
        {
            Cursor.SetCursor(_cursorSettings.CursorTexture, _cursorSettings.HotSpot, CursorMode.Auto);
        }
    }

    public ControlMethod ControlType => _mainControlType;
    public PauseOptionsOnEscape PauseOptions => _pauseOptionsOnEscape;
    public EscapeKey GlobalCancelAction => SetGlobalEscapeFunction();
    public InGameSystem InGameMenuSystem => _inGameMenuSystem;
    public InMenuOrGame WhereToStartGame => _startGameWhere;
    public bool CanUseVirtualCursor => _useVirtualCursor == VirtualControl.Yes;
    public bool HideMouseCursor => _hideMouseCursor == IsActive.Yes;
    public VirtualCursorSettings ReturnVirtualCursorSettings => _virtualCursorSettings;
    
    
    //Abstracts
    public abstract bool  AnyMouseClicked { get; }
    public abstract bool  LeftMouseClicked { get; }
    public abstract bool  RightMouseClicked { get; }
    public abstract bool CanSwitchToKeysOrController(bool allowKeys);
    public abstract bool SwitchKeyPressed();
    public abstract bool MenuNavigationPressed(bool allowKeys);
    public abstract AxisEventData DoMenuNavigation();
    public abstract bool CanSwitchToMouseOrVC(bool allowKeys);
    public abstract bool SwitchToVCPressed();
    protected abstract float MouseXAxis { get; }
    protected abstract float MouseYAxis { get; }
    public abstract Vector3 GetMouseOrVcPosition();
    public abstract void SetVirtualCursorPosition(Vector3 pos);
    private protected abstract Vector3 GetVirtualCursorPosition();
    public void TurnOffInGameMenuSystem() => _inGameMenuSystem = InGameSystem.Off;
    public abstract bool HorizontalNavPressed();
    public abstract bool VerticalNavPressed();
    public abstract bool PressPause();
    public abstract bool PressedMenuToGameSwitch();
    public abstract bool PressedCancel();
    public abstract bool PressedPositiveSwitch();
    public abstract bool PressedNegativeSwitch();
    public abstract bool PressedPositiveGOUISwitch();
    public abstract bool PressedNegativeGOUISwitch();
    public abstract float VcHorizontal();
    public abstract float VcVertical();
    public abstract bool VcHorizontalPressed();
    public abstract bool VcVerticalPressed();
    public abstract bool MultiSelectPressed();
    public abstract bool PressSelect();
    public abstract bool HotKeyChecker(HotKey hotKey);

    private EscapeKey SetGlobalEscapeFunction()
    {
        switch (_globalEscapeFunction)
        {
            case PauseFunction.DoNothing:
                return EscapeKey.None;
            case PauseFunction.BackOneLevel:
                return EscapeKey.BackOneLevel;
            case PauseFunction.BackToHome:
                return EscapeKey.BackToHome;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}