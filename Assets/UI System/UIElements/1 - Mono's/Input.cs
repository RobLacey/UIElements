﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

public partial class Input : MonoBehaviour, IEZEventUser, /*IPausePressed,*/ IStandardCancel, IChangeControlsPressed, 
                             IMenuGameSwitchingPressed, IServiceUser, IEZEventDispatcher, IVirtualCursorSettings,
                             IIsAService
{
    [SerializeField] [Space(10f)]
    [ValidateInput(CheckForScheme, InfoBox)]
    private InputScheme _inputScheme  = default;

    [SerializeField] private Transform _virtualCursorParent;
    [SerializeField]
    [Space(10f)]
    private PauseAndEscapeHandler _pauseAndEscapeSettings;
    
    [SerializeField] 
    [OnValueChanged(CheckForNewHotKey)] [Space(10f)]
    private List<HotKeys> _hotKeySettings = new List<HotKeys>();
    
    [Header(Settings, order = 2)][HorizontalLine(1f, EColor.Blue, order = 3)] 
    
    [SerializeField] 
    private UIInputEvents _uiInputEvents  = default;

    //Variables
    private bool _inMenu;
    private IDataHub _myDataHub;

    //Events
    private Action<IMenuGameSwitchingPressed> OnMenuAndGameSwitch { get; set; }
    private Action<IStandardCancel> OnCancelPressed { get; set; }
    private Action<IChangeControlsPressed> OnChangeControlPressed { get; set; }
    
    //Properties and Getters / Setters
    private bool GameIsPaused => _myDataHub.GamePaused;
    private bool AllowKeys => _myDataHub.AllowKeys;
    private bool ActiveResolvePopUps => _myDataHub.HasResolvePopUp;
    private bool IsAtRootTrunk => _myDataHub.IsAtRoot;
    private IBranch ActiveBranch => _myDataHub.ActiveBranch;
    private bool CanStart => _myDataHub.SceneStarted;

    public bool StartInGame()
    {
        if (_inputScheme.InGameMenuSystem == InGameSystem.On)
        {
            return _inputScheme.WhereToStartGame == InMenuOrGame.InGameControl;
        }
        return false;
    }
    
    private void SaveInMenu(IInMenu args)
    {
        _inMenu = args.InTheMenu;
        if(_inMenu)
        {
            _uiInputEvents.SwitchBetweenGameAndMenuPressed(InMenuOrGame.InMenu);
        }
        else
        {
            _uiInputEvents.SwitchBetweenGameAndMenuPressed(InMenuOrGame.InGameControl);
        }
    }

    public Transform GetParentTransform => _virtualCursorParent;
    public EscapeKey EscapeKeySettings => ActiveBranch.EscapeKeyType;
    private IMenuAndGameSwitching MenuToGameSwitching { get; set; }
    private IChangeControl ChangeControl { get; set; }
    private ISwitchGroup SwitchGroups { get; set; }
    private IVirtualCursor VirtualCursor { get; set; }


    //Main
    private void Awake()
    {
        _inputScheme.Awake();
        ChangeControl = EZInject.Class.WithParams<IChangeControl>(this);
        MenuToGameSwitching = EZInject.Class.NoParams<IMenuAndGameSwitching>();
        SwitchGroups = EZInject.Class.NoParams<ISwitchGroup>();
        
        if(_inputScheme.CanUseVirtualCursor)
            VirtualCursor = EZInject.Class.WithParams<IVirtualCursor>(this);
        AddService();
    }

    public void AddService() => EZService.Locator.AddNew<IInput>(this);

    public void OnRemoveService() { }

    private void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        SetUpHotKeys();
        ChangeControl.OnEnable();
        MenuToGameSwitching.OnEnable();
        SwitchGroups.OnEnable();
        _pauseAndEscapeSettings.OnEnable();
        if(_inputScheme.CanUseVirtualCursor)
            VirtualCursor.OnEnable();
        ObserveEvents();
    }

    private void OnDisable()
    {
        UnObserveEvents();
        ChangeControl.OnDisable();
        MenuToGameSwitching.OnDisable();
        _pauseAndEscapeSettings.OnDisable();
    }

    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    private void SetUpHotKeys()
    {
        if (_hotKeySettings.Count == 0) return;
        foreach (var hotKey in _hotKeySettings)
        {
            hotKey.OnEnable();
        }
    }
    
    public void FetchEvents()
    {
        OnMenuAndGameSwitch = InputEvents.Do.Fetch<IMenuGameSwitchingPressed>();
        OnCancelPressed = InputEvents.Do.Fetch<IStandardCancel>();
        OnChangeControlPressed = InputEvents.Do.Fetch<IChangeControlsPressed>();
    }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IInMenu>(SaveInMenu);
    }

    public void UnObserveEvents()
    {
        HistoryEvents.Do.Unsubscribe<IInMenu>(SaveInMenu);
    }

    private void Start()   
    {
        ChangeControl.OnStart();
        SwitchGroups.OnStart();
        if(_inputScheme.CanUseVirtualCursor)
            VirtualCursor.OnStart();
        MenuToGameSwitching.OnStart();
        _pauseAndEscapeSettings.OnStart();
    }

    private void Update()
    {
        if(_myDataHub.PlayingTweens > 0) return;
        
        if(_inputScheme.CanUseVirtualCursor)
            VirtualCursor.PreStartMovement();
        
        if (!CanStart) return;
        
        if (CanPauseGame())
        {
            _pauseAndEscapeSettings.DoPauseOrEscapeProcess();
            return;
        }
        
        if (CanSwitchBetweenInGameAndMenu() && IsAtRootTrunk) return;
        
        if (CheckIfHotKeyAllowed()) return;
        
        if (_inMenu) InMenuControls();
    }

    private void InMenuControls()
    {
        if (CanDoCancel())
        {
            CancelPressed();
            return;
        }
        
        if(_inputScheme.CanUseVirtualCursor)
        {
            if (MoveVirtualCursor()) return;
            
            if (_inputScheme.PressSelect())
            {
                _myDataHub.Highlighted.NodeSelected();
                return;
            }
        }

        if(SwitchGroups.CanSwitchBranches())
        {
            if(!AllowKeys)
            {
                SwitchGroups.ImmediateSwitch();
            }
            else
            {
                SwitchGroups.SwitchGroupProcess();
                return;
            }
        }
        
        if (_inputScheme.MenuNavigationPressed(AllowKeys))
        {
            if (_inputScheme.PressSelect())
            {
                _myDataHub.Highlighted.NodeSelected();
                return;
            }
            if(AllowKeys)
            {
                DoMenuNavigation();
                return;
            }           
        }

        if(MultiSelectPressed) return;
        DoChangeControlPressed();
    }

    private void DoMenuNavigation()
    {
        var eventData = _inputScheme.DoMenuNavigation();
        _myDataHub.Highlighted.NavigateToNextMenuItem(eventData);
    }

    private bool MoveVirtualCursor()
    {
        if (!VirtualCursor.CanMoveVirtualCursor) return false;

        VirtualCursor.Update();
        return true;
    }

    private void DoChangeControlPressed() => OnChangeControlPressed?.Invoke(this);

    private bool MultiSelectPressed => _inputScheme.MultiSelectPressed() && !AllowKeys;

    private bool CanPauseGame() => Paused() || Cancel() && !MultiSelectPressed;

    private bool Paused() => _inputScheme.PressPause() && _pauseAndEscapeSettings.CanPause();

    private bool Cancel() => _inputScheme.PressedCancel() && _pauseAndEscapeSettings.CanEscape();

    private bool CanSwitchBetweenInGameAndMenu()
    {
        if (!_inputScheme.PressedMenuToGameSwitch()) return false;
        OnMenuAndGameSwitch?.Invoke(this);
        return true;
    }

    private bool CheckIfHotKeyAllowed()
    {
        if (GameIsPaused || ActiveResolvePopUps || MultiSelectPressed) return false;
        if (!HasMatchingHotKey()) return false;
        if(!_inMenu)
            OnMenuAndGameSwitch?.Invoke(this);
        return true;
    }

    private bool HasMatchingHotKey()
    {
        foreach (var hotKeySetting in _hotKeySettings)
        {
            if (_inputScheme.HotKeyChecker(hotKeySetting.HotKeyInput))
            {
                hotKeySetting.HotKeyActivation();
                return true;
            }
        }
        return false;
    }

    private bool CanDoCancel() => _inputScheme.PressedCancel() && !MultiSelectPressed;

    private void CancelPressed() => OnCancelPressed?.Invoke(this);
}
