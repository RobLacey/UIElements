using EZ.Service;
using UIElements;

public class ToggleAsTab : IServiceUser, IMonoEnable, IMonoDisable, IMonoStart
{
    public ToggleAsTab(IBranch linkBranch, IBranch myBranch, INode myNode, Toggle toggle)
    {
        LinkBranch = linkBranch;
        MyBranch = myBranch;
        MyNode = myNode;
        _myToggle = toggle;
    }

    //Variables
    private readonly HistoryData _data = new HistoryData();
    private InputScheme _inputScheme;
    private readonly Toggle _myToggle;
    
    //Properties, Getters & Setters
    private IBranch LinkBranch  { get; }
    private bool HasLink => LinkBranch.IsNotNull();
    private IBranch MyBranch { get; }
    private INode MyNode { get; }
    private bool MultiSelectAllowed => _inputScheme.MultiSelectPressed() &&
                                       MyNode.MultiSelectSettings.OpenChildBranch     == IsActive.No
                                       && MyNode.MultiSelectSettings.AllowMultiSelect == IsActive.Yes ;
    private bool LinkIsSelectedAndHasAChild => LinkBranch.LastSelected.IsNotNull() && LinkBranch.LastSelected.HasChildBranch.IsNotNull();
    private bool LinkBranchIsActive => LinkBranch.LastSelected.HasChildBranch.CanvasIsEnabled;
    
    //Main
    public void OnEnable()
    {
        if(!HasLink) return;
        UseEZServiceLocator();
        MyBranch.EnterBranchEvent += OnBranchEnter;
        MyBranch.ExitBranchEvent += OnBranchExit;
    }

    public void OnDisable()
    {
        if(!HasLink) return;

        MyBranch.EnterBranchEvent -= OnBranchEnter;
        MyBranch.ExitBranchEvent -= OnBranchExit;
    }

    public void OnStart()
    {
        if (!HasLink) return;
        LinkBranch.ParentTrunk = MyBranch.ParentTrunk;
    }

    public void UseEZServiceLocator() => _inputScheme = EZService.Locator.Get<InputScheme>(this);

    private void OnBranchEnter()
    {
        if (!_myToggle.IsToggleSelected) return;
        LinkBranch.DoNotTween();
    }
    
    public void NavigateToChildBranch()
    {
        if (!HasLink || MultiSelectAllowed) return;
        LinkBranch.OpenThisBranch(MyBranch.MyParentBranch);
    }

    private void OnBranchExit()
    {
        ClearOpenLinkedBranches();
    }

    public void ClearOpenLinkedBranches()
    {
        if(!HasLink) return;
        
         if (LinkIsSelectedAndHasAChild && LinkBranchIsActive)
         {
             _data.AddData(LinkBranch.LastSelected);
             _data.AddStopPoint(LinkBranch.LastSelected);
             HistoryListManagement.ResetAndClearHistoryList(_data, ClearAction.StopAt);
         }
        
        if(_myToggle.IsToggleSelected) return;
        LinkBranch.ExitThisBranch(OutTweenType.Cancel);
    }
}