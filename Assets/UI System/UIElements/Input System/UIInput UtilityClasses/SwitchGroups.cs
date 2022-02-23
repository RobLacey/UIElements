using System;
using EZ.Events;
using EZ.Inject;
using EZ.Service;
using UIElements;
using UnityEngine;


public interface ISwitchGroup: IMonoDisable, IMonoEnable, IMonoStart
{
    bool CanSwitchBranches();
    bool SwitchGroupProcess();
    public void ImmediateSwitch();
}

public class SwitchGroups : IEZEventUser, IParameters, ISwitchGroupPressed, IServiceUser, ISwitchGroup, 
                            IChangeControlsSwitchPressed, IEZEventDispatcher
{
    public SwitchGroups()
    { 
        _switcher = EZInject.Class.NoParams<IGOUISwitcher>();
        _homeGroup = EZInject.Class.NoParams<IHomeGroup>();
        UseEZServiceLocator();
    }

    public void UseEZServiceLocator()
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    //Variables
    private InputScheme _inputScheme;
    private readonly IGOUISwitcher _switcher;
    private readonly IHomeGroup _homeGroup;
    private bool _onHomeScreen = true;
    private IDataHub _myDataHub;

    private enum SwitchStyle
    {
        Normal, Branch
    }

    //Properties, Getters / Setters
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private Action<ISwitchGroupPressed> OnSwitchGroupPressed { get; set; }


    public bool CanSwitchBranches() => _myDataHub.NoPopups && !MouseOnly() 
                                                           && !_myDataHub.GamePaused 
                                                           && !_inputScheme.MultiSelectPressed();

    private bool MouseOnly()
    {
        if(_inputScheme.ControlType == ControlMethod.MouseOnly) 
            _inputScheme.TurnOffInGameMenuSystem();
        return _inputScheme.ControlType == ControlMethod.MouseOnly;
    }
    
    //Main
    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
        _switcher.OnEnable();
        _homeGroup.OnEnable();
    }
    
    public void FetchEvents() => OnSwitchGroupPressed = InputEvents.Do.Fetch<ISwitchGroupPressed>();

    public void ObserveEvents() => HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
    
    public void OnDisable() => UnObserveEvents();

    public void UnObserveEvents() => HistoryEvents.Do.Unsubscribe<IOnHomeScreen>(SaveOnHomeScreen);

    public void OnStart()
    {
        _homeGroup.SetUpHomeGroup();
        _switcher.OnStart();
    }
    
    public bool SwitchGroupProcess()
    {
        if (_onHomeScreen)
        {
            return NormalSwitch(HomeGroupSwitch, SwitchStyle.Normal) || GOUISwitch();
        }
        
        if(!_onHomeScreen)
        {
            if (_myDataHub.ActiveBranch.BranchGroupsList.Count <= 1) return false;
            return NormalSwitch(BranchSwitch, SwitchStyle.Branch);
        }
        return false;
    }

    private bool NormalSwitch(Action<SwitchInputType> switchAction, SwitchStyle switchStyle)
    {
        var canSwitch = Switch(_inputScheme.PressedPositiveSwitch(),
                               _inputScheme.PressedNegativeSwitch(),
                               switchAction);
        if (canSwitch && switchStyle == SwitchStyle.Normal)
        {
            OnSwitchGroupPressed?.Invoke(this);
        }

        return canSwitch;
    }

    // private bool BranchGroupSwitch(Action<SwitchType> switchAction)
    // {
    //     // if (_myDataHub.ActiveBranch.BranchGroupsList.Count <= 1) return false;
    //
    //     var canSwitch = Switch(_inputScheme.PressedPositiveSwitch(),
    //                          _inputScheme.PressedNegativeSwitch(),
    //                          switchAction);
    //     
    //     if (canSwitch)
    //     {
    //         _myDataHub.Highlighted.ThisNodeNotHighLighted();
    //         OnSwitchGroupPressed?.Invoke(this);
    //     }
    //
    //     return canSwitch;
    // }

    public void ImmediateSwitch()
    {
        if (!_onHomeScreen || _switcher.GOUIPlayerCount <= 1) return;
        
        if(_inputScheme.PressedPositiveGOUISwitch() || _inputScheme.PressedNegativeGOUISwitch())
        {
            GOUISwitchAction(SwitchInputType.Activate);
        }
    }

    private bool GOUISwitch()
    {
        if (!_onHomeScreen) return false;
        
        if (_switcher.GOUIPlayerCount > 1)
        {
            var canSwitch = Switch(_inputScheme.PressedPositiveGOUISwitch(), 
                                   _inputScheme.PressedNegativeGOUISwitch(), 
                                   GOUISwitchAction);
            if(canSwitch)
                OnSwitchGroupPressed?.Invoke(this);
            return canSwitch;
        }

        return false;
    }
    
    private static bool Switch(bool posPressed, bool negPressed, Action<SwitchInputType> switchAction)
    {
        if (posPressed)
        {
            switchAction.Invoke(SwitchInputType.Positive);
            return true;
        }

        if (negPressed)
        {
            switchAction.Invoke(SwitchInputType.Negative);
            return true;
        }

        return false;
    }

    private void HomeGroupSwitch(SwitchInputType switchInputType) => _homeGroup.SwitchHomeGroups(switchInputType);

    private void BranchSwitch(SwitchInputType switchInputType)
    {
        var activeBranch = _myDataHub.ActiveBranch;
        
        activeBranch.GroupIndex = BranchGroups.SwitchBranchGroup(activeBranch.BranchGroupsList,
                                                                 activeBranch.GroupIndex,
                                                                 switchInputType);
    }

    private void GOUISwitchAction(SwitchInputType switchInputType) => _switcher.UseGOUISwitcher(switchInputType);

}

