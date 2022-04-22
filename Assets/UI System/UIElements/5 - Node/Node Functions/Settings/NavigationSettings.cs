using System;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

public interface INavigationSettings : IComponentSettings
{
    IBranch ChildBranch { get; set; }
    NavigationType NavType { get; }
    Node Up { get; }
    Node Down { get; }
    Node Left { get; }
    Node Right { get; }
}

[Serializable]
public class NavigationSettings :INavigationSettings
{
    [SerializeField] 
    [ValidateInput(ValidBranch, ErrorMessage)]
    [AllowNesting] [Label("Child Branch")] [HideIf(CannotNav)] 
    private Branch _childBranch = default;
    
    [SerializeField] 
    private NavigationType _setKeyNavigation = NavigationType.None;

    [SerializeField] 
    //[ValidateInput(ValidBranch, ErrorMessage)]
    //[AllowNesting] [EnableIf("UpDownNav")]
    private NavigateKeyPress _upPress = default;
    [SerializeField] 
    //[ValidateInput(ValidBranch, ErrorMessage)]
    // [AllowNesting] [ShowIf("UpDownNav")]
    private NavigateKeyPress _downPress = default;
    [SerializeField] 
    //[ValidateInput(ValidBranch, ErrorMessage)]
    // [AllowNesting] [ShowIf("RightLeftNav")]
    private NavigateKeyPress _rightPress = default;
    [SerializeField] 
    //[ValidateInput(ValidBranch, ErrorMessage)]
    // [AllowNesting] [ShowIf("RightLeftNav")]
    private NavigateKeyPress _leftPress = default;
    
    // [SerializeField] 
    // [AllowNesting] [ShowIf("UpDownNav")] private Node _up = default;
    // [SerializeField] 
    // [AllowNesting] [ShowIf("UpDownNav")] private Node _down = default;
    // [SerializeField] 
    // [AllowNesting] [ShowIf("RightLeftNav")] private Node _left = default;
    // [SerializeField] 
    // [AllowNesting] [ShowIf("RightLeftNav")] private Node _right = default;

    //Editor Scripts
    private const string ValidBranch = nameof(CheckValidBranch);
    private const string CannotNav = nameof(CantNavigate);
    private const string ErrorMessage = "Must NOT use a Pop Up here. Do this via the Event Functions Or HotKeys instead.";
    public bool CantNavigate { get; set; }

    public bool UpDownNav()
        => _setKeyNavigation == NavigationType.UpAndDown || _setKeyNavigation == NavigationType.AllDirections;

    public bool RightLeftNav()
        => _setKeyNavigation == NavigationType.RightAndLeft || _setKeyNavigation == NavigationType.AllDirections;
    
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
    public NavigationType NavType => _setKeyNavigation;
    public Node Up => _upPress.Navigate;
    public Node Down => _downPress.Navigate;
    public Node Left => _leftPress.Navigate;
    public Node Right => _rightPress.Navigate;
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

[Serializable]
public class NavigateKeyPress
{
    [SerializeField] private NavPressMoveType _moveType = NavPressMoveType.None;
    [SerializeField] private Node _navigate = default;

    private enum NavPressMoveType
    {
        None, Navigate, ToBranch, Back
    }

    public Node Navigate => _navigate;
}
