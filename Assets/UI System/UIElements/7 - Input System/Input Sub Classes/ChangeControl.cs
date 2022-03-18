using System;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

/// <summary>
/// Class that handles switching control from the mouse to a keyboard or controller
/// </summary>

public interface IChangeControl : IEZEventUser, IMonoEnable, IMonoStart, IMonoDisable { }

public class ChangeControl : IChangeControl, IAllowKeys, IEZEventDispatcher, IServiceUser
{
    public ChangeControl(IInput input) => _startInGame = input.StartInGame();

    //Variables
    private ControlMethod _controlMethod;
    private readonly bool _startInGame;
    private InputScheme _inputScheme;
    private IDataHub _myDataHub;
    private IHistoryTrack _historyTracker;

    //Properties
    public bool CanAllowKeys { get; private set; }
    private bool UsingVirtualCursor => _inputScheme.CanUseVirtualCursor;
    private bool ShowCursorOnStart { get; set; }
    private bool SceneStarted { get; set; }

    //Events
    private Action<IAllowKeys> AllowKeys { get; set; }

    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        ObserveEvents();
    }
    
    public void UseEZServiceLocator()
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
        _historyTracker = EZService.Locator.Get<IHistoryTrack>(this);
    }

    public void FetchEvents() => AllowKeys = InputEvents.Do.Fetch<IAllowKeys>();

    public void ObserveEvents()
    {
        InputEvents.Do.Subscribe<IChangeControlsPressed>(ChangeControlType);
        HistoryEvents.Do.Subscribe<IOnStart>(StartGame);
    }
    
    public void OnDisable() => UnObserveEvents();

    public void UnObserveEvents()
    {
        InputEvents.Do.Unsubscribe<IChangeControlsPressed>(ChangeControlType);
        HistoryEvents.Do.Unsubscribe<IOnStart>(StartGame);
    }

    public void OnStart()
    {
        _controlMethod = _inputScheme.ControlType;
        OnLevelSetUp();
    }

    private void OnLevelSetUp()
    {
        ShowCursorOnStart = (_inputScheme.ControlType == ControlMethod.MouseOnly
                           || _inputScheme.ControlType == ControlMethod.AllowBothStartWithMouse) 
                            || !_inputScheme.HideMouseCursor;

        ShowMouseCursorOnStart();
    }

    private void ShowMouseCursorOnStart()
    {
        if (UsingVirtualCursor)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = ShowCursorOnStart;
        }
    }

    private void StartGame(IOnStart onStart)
    {
        if (MousePreferredControlMethod())
        {
            SetUpMouse();
        }
        else
        {
            SetUpKeysOrCtrl();
        }
        SceneStarted = true;
    }

    private bool MousePreferredControlMethod() 
        => _controlMethod == ControlMethod.MouseOnly || _controlMethod == ControlMethod.AllowBothStartWithMouse;

    private void SetUpMouse()
    {
        if (!_startInGame)
        {
            CanAllowKeys = true;
            ActivateMouseOrVirtualCursor();
        }
        else
        {
            CanAllowKeys = false;
            SetAllowKeys();
        }
    }

    private void SetUpKeysOrCtrl()
    {
        if (!_startInGame)
        {
            CanAllowKeys = false;
            ActivateKeysOrControl();
        }
        else
        {
            CanAllowKeys = true;
            SetAllowKeys();
        }
    }

    private void ChangeControlType(IChangeControlsPressed args)
    {
        if (_inputScheme.CanSwitchToMouseOrVC(CanAllowKeys))
        {
            ActivateMouseOrVirtualCursor();
            return;
        }
        
        if(_inputScheme.CanSwitchToKeysOrController(CanAllowKeys))
        {
            ActivateKeysOrControl();
        }
    }
    
    private void ActivateMouseOrVirtualCursor()
    {
        Cursor.visible = !UsingVirtualCursor;
        
        if (!CanAllowKeys) return;
        CanAllowKeys = false;
        SetAllowKeys();
        
        if(!SceneStarted) return;
        _myDataHub.Highlighted.ThisNodeNotHighLighted();
    }

    private void ActivateKeysOrControl()
    {
        if (_inputScheme.HideMouseCursor)
        {
            Cursor.visible = false;
        }
        
        if (CanAllowKeys) return;
        CanAllowKeys = true;
        SetAllowKeys();

       if(!SceneStarted) return;
        
       _historyTracker.MoveToLastBranchInHistory();
    }

    private void SetAllowKeys()
    {
        _myDataHub.SetAllowKeys(CanAllowKeys);
        AllowKeys?.Invoke(this);
    }
}
