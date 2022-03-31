using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class ToggleData
{
    // [SerializeField] 
    // private ToggleGroup _toggleGroupId = ToggleGroup.TG1;

    [SerializeField] private ToggleGroupObject _toggleGroup;
    
    [SerializeField] 
    [AllowNesting] [HideIf(NotInGroup)]
    private IsActive _startAsSelected = IsActive.No;
    [SerializeField] 
    [AllowNesting] [Label("Tab Branch")] [HideIf(NotInGroup)]  private Branch _linkBranch = default;


    //public ToggleGroup ReturnToggleId => _toggleGroupId;
   // public bool CanUseToggleSwitcher => _toggleGroup.CanUseToggleSwitcher;
    public ToggleGroupObject ToggleGroupData => _toggleGroup;
    public bool HasToggleGroup => _toggleGroup.IsNotNull();
    
    public IBranch LinkBranch
    {
        get => _linkBranch;
        set => _linkBranch = (Branch) value;
    }

    private const string NotInGroup = nameof(IsNotInGroup);
    private bool IsNotInGroup()
    {
        if (_toggleGroup.IsNull())
        {
            _linkBranch = null;
            _startAsSelected = IsActive.No;
        }        
        return _toggleGroup.IsNull();
        // if (_toggleGroupId == ToggleGroup.None)
        // {
        //     _linkBranch = null;
        //     _startAsSelected = IsActive.No;
        // }        
        // return _toggleGroupId == ToggleGroup.None;
    }

    public IsActive StartAsSelected => _startAsSelected;
    public void SetStartAsSelected() => _startAsSelected = IsActive.Yes;
}
