using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class ToggleData
{
    [SerializeField] private ToggleGroupObject _toggleGroup;
    
    [SerializeField] 
    [AllowNesting] [HideIf(NotInGroup)]
    private IsActive _startAsSelected = IsActive.No;
    
    [SerializeField] 
    [AllowNesting] [ValidateInput(ValidBranch, ErrorMessage)] [Label("Tab Branch")] [HideIf(NotInGroup)]  
    private Branch _linkBranch = default;

    //Editor
    private const string ValidBranch = nameof(CheckValidBranch);
    private const string ErrorMessage = "Must NOT use a Pop Up here. Do this via the Event Functions Or HotKeys instead.";

    private bool CheckValidBranch(Branch branch)
    {
        if (branch.IsNull()) return true;
        return !branch.IsAPopUpBranch();
    }

    //Properties, Getters & Setters
    public ToggleGroupObject ToggleGroupData => _toggleGroup;
    public bool HasToggleGroup => _toggleGroup.IsNotNull();
     public IsActive StartAsSelected => _startAsSelected;

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
    }

}
