using System.Collections.Generic;

public class ToggleSwitcher: ISwitch
{
    public ToggleSwitcher(ToggleGroupObject toggleGroupObject) => _myToggleGroupObject = toggleGroupObject;

    //Variables
    private int _index = 0;
    private Toggle _lastActiveToggle;
    private readonly ToggleGroupObject _myToggleGroupObject;
    
    
    //Properties, Getter / Setters
    private List<Toggle> ThisToggleGroup { get; set; } = new List<Toggle>();
    public bool HasOnlyOneMember => ThisToggleGroup.Count == 1;
    
    //Main
    public void SetSwitchGroup(List<Toggle> nodes)
    {
        ThisToggleGroup = nodes;
        _lastActiveToggle = ThisToggleGroup[_index];
    }

    public void SetNewIndex(Toggle toggle)
    {
        if(_lastActiveToggle == toggle) return;
        
        _index = ThisToggleGroup.IndexOf(toggle);
        _lastActiveToggle = ThisToggleGroup[_index];
    }

    public void DoSwitch(SwitchInputType switchInputType)
    {
        if(ThisToggleGroup.Count <=1) return;
        
        switch (switchInputType)
        {
            case SwitchInputType.Positive:
                _index = _index.PositiveIterate(ThisToggleGroup.Count);
                break;
            case SwitchInputType.Negative:
                _index = _index.NegativeIterate(ThisToggleGroup.Count);
                break;
        }
        
        _lastActiveToggle = ThisToggleGroup[_index];

       if(_myToggleGroupObject.ActivateToggleOnSwitch)
       {
           ThisToggleGroup[_index].OnEnteringNode();
           ThisToggleGroup[_index].NodeSelected();
       }
       else
       {
            ThisToggleGroup[_index].OnEnteringNode();
       }
    }
}