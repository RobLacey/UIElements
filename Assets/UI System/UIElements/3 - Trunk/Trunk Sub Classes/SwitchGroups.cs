using EZ.Inject;
using EZ.Service;
using UIElements;
using UnityEngine;


public interface ISwitchGroup: IMonoEnable, IMonoStart
{
    bool CanSwitchBranches();
    void SwitchGroupProcess();
    void ImmediateSwitch();
}

public class SwitchGroups : IParameters, IServiceUser, ISwitchGroup
{
    public SwitchGroups()
    { 
        _gouiSwitcher = EZInject.Class.NoParams<IGOUISwitcher>();
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
    private ISwitch SwitchTrunkGroup => _myDataHub.CurrentSwitcher;
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
        if (_inputScheme.WhereToStartGame == InMenuOrGame.InGameControl)
            _lastSwitchType = SwitchType.GOUI;
    }
    
    public void OnStart()
    {
        _gouiSwitcher.OnStart();
    }
    
    public bool CanSwitchBranches() => _myDataHub.NoPopUps && !_myDataHub.GamePaused
                                                           && _myDataHub.SceneStarted
                                                           && !_inputScheme.MultiSelectPressed()
                                                           && _inputScheme.SwitchKeyPressed();

    public void SwitchGroupProcess()
    {
        //TODO Change this check in in Input or in GOUI Switch
        if(_myDataHub.IsAtRoot && 
           Switch(_gouiSwitcher, GOUIButtonsPressed, SwitchType.GOUI, _inputScheme.PressedPositiveGOUISwitch())) return;
        
        Switch(SwitchTrunkGroup, SwitcherButtonsPressed, SwitchType.Normal, _inputScheme.PressedPositiveSwitch());
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
        if (_myDataHub.IsAtRoot)
        {
            if (GOUIButtonsPressed)
                _lastSwitchType = SwitchType.GOUI;
            
            if (SwitcherButtonsPressed)
                _lastSwitchType = SwitchType.Normal;
        }
    }
}

