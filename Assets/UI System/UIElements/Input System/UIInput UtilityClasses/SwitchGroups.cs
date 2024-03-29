﻿using System;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using UIElements;
using UnityEngine;


public interface ISwitchGroup: IMonoEnable, IMonoStart
{
    bool CanSwitchBranches();
    void SwitchGroupProcess();
    public void ImmediateSwitch();
}

public class SwitchGroups : IParameters, IServiceUser, ISwitchGroup
{
    public SwitchGroups()
    { 
        _gouiSwitcher = EZInject.Class.NoParams<IGOUISwitcher>();
        _homeGroup = EZInject.Class.NoParams<IHomeGroup>();
        UseEZServiceLocator();
    }

    public void UseEZServiceLocator()
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
        _historyTracker = EZService.Locator.Get<IHistoryTrack>(this);
    }

    //Variables
    private InputScheme _inputScheme;
    private readonly IGOUISwitcher _gouiSwitcher;
    private readonly IHomeGroup _homeGroup;
    private IHistoryTrack _historyTracker;
    private IDataHub _myDataHub;
    private SwitchType _lastSwitchType = SwitchType.Normal;
    
    //Enums
    private enum SwitchType { Normal, GOUI }

    //Properties, Getters / Setters
    private bool SwitcherButtonsPressed => _inputScheme.PressedPositiveSwitch() || _inputScheme.PressedNegativeSwitch();
    private bool GOUIButtonsPressed => _inputScheme.PressedPositiveGOUISwitch() || _inputScheme.PressedNegativeGOUISwitch();

    //Main
    public void OnEnable()
    {
        _gouiSwitcher.OnEnable();
        _homeGroup.OnEnable();
        if (_inputScheme.WhereToStartGame == InMenuOrGame.InGameControl)
            _lastSwitchType = SwitchType.GOUI;
    }
    
    public void OnStart()
    {
        _homeGroup.SetUpHomeGroup();
        _gouiSwitcher.OnStart();
    }
    
    public bool CanSwitchBranches() => _myDataHub.NoPopups && !_myDataHub.GamePaused
                                                           && _myDataHub.SceneStarted
                                                           && !_inputScheme.MultiSelectPressed()
                                                           && _inputScheme.SwitchKeyPressed();

    public void SwitchGroupProcess()
    {
        if (_myDataHub.OnHomeScreen)
        {
            if(Switch(_gouiSwitcher, GOUIButtonsPressed, SwitchType.GOUI, _inputScheme.PressedPositiveGOUISwitch()) || 
               Switch(_homeGroup, SwitcherButtonsPressed, SwitchType.Normal, _inputScheme.PressedPositiveSwitch())) return;
        }

        Switch(_myDataHub.ActiveBranch.BranchGroupsHandler, SwitcherButtonsPressed, SwitchType.Normal,
               _inputScheme.PressedPositiveSwitch());
    }

    private bool Switch(ISwitch group, bool switchPressed, SwitchType switchType, bool inputCheck)
    {
        if(!switchPressed) return false;
        if (HasOnlyOnePlayer(group.HasOnlyOneMember, _lastSwitchType == switchType)) return true;
        
        _historyTracker.SwitchGroupPressed();

        if (_lastSwitchType != switchType)
        {
            group.DoSwitch(SwitchInputType.Activate);
            _lastSwitchType = switchType;
            return true;
        }
        

        _lastSwitchType = switchType;
        group.DoSwitch(inputCheck ? SwitchInputType.Positive : SwitchInputType.Negative);
        Debug.Log("End");

        return true;
    }
    
    private bool HasOnlyOnePlayer(bool oneObjectInList, bool switchTypesMatch)
    {
        if (oneObjectInList && switchTypesMatch)
        {
            _myDataHub.Highlighted.SetNodeAsActive();
            return true;
        }
        return false;
    }

    public void ImmediateSwitch()
    {
        if (_myDataHub.OnHomeScreen)
        {
            if(GOUIButtonsPressed)
                _lastSwitchType = SwitchType.GOUI;

            if (SwitcherButtonsPressed)
                _lastSwitchType = SwitchType.Normal;
        }
        else
        {
            if(SwitcherButtonsPressed)
                _lastSwitchType = SwitchType.Normal;
        }
    }
}

