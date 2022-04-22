using System;
using NaughtyAttributes;
using UnityEngine;

public interface INavigationSettings : IComponentSettings
{
    NavigateKeyPress Up { get; }
    NavigateKeyPress Down { get; }
    NavigateKeyPress Left { get; }
    NavigateKeyPress Right { get; }
}

[Serializable]
public class NavigationSettings :INavigationSettings
{
    [Space(10f, order = 1)] [Header("Linked Branch - Always When Selected and Key Press If Set", order = 2)] 
    [HorizontalLine(1f, EColor.Blue, order = 3)]

    [SerializeField] 
    [ValidateInput(ValidBranch, ErrorMessage)] [AllowNesting] [Label("Child Branch")] [HideIf(CannotNav)] 
    private Branch _childBranch = default;

    [Space(10f, order = 1)] [Header("Navigation Key Presses", order = 2)] [HorizontalLine(1f, EColor.Blue, order = 3)]
    
    [SerializeField]  private NavigateKeyPress _upPress = default;
    [SerializeField]  private NavigateKeyPress _downPress = default;
    [SerializeField] private NavigateKeyPress _rightPress = default;
    [SerializeField] private NavigateKeyPress _leftPress = default;
    
    //Editor Scripts
    private const string ValidBranch = nameof(CheckValidBranch);
    private const string CannotNav = nameof(SetNavigate);
    private const string ErrorMessage = "Must NOT use a Pop Up here. Do this via the Event Functions Or HotKeys instead.";

    private bool SetNavigate()
    {
        if (CantNavigate)
            _childBranch = null;

        return CantNavigate;
    }
    public bool CantNavigate { get; set; }
    
    private bool CheckValidBranch(Branch branch)
    {
        if (branch.IsNull()) return true;
        return !branch.IsAPopUpBranch();
    }

    //Properties, Setters & Getters
    public IBranch ChildBranch
    {
        get => _childBranch;
        set => _childBranch = (Branch)value;
    }
    
    private void SetNewChild(IBranch newChild) => ChildBranch = newChild;
    public NavigateKeyPress Up => _upPress;
    public NavigateKeyPress Down => _downPress;
    public NavigateKeyPress Left => _leftPress;
    public NavigateKeyPress Right => _rightPress;
    public UINavigation Instance { get; set; }

    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        if (CanCreate(functions))
        {
            uiNodeEvents.ReturnMasterNode.MyRunTimeSetter.SetChildBranch = SetNewChild;
            Instance = new UINavigation(this, uiNodeEvents);
            return Instance;
        }
        return null;
    }

    private bool CanCreate(Setting functions) => (functions & Setting.NavigationAndOnClick) != 0;
}