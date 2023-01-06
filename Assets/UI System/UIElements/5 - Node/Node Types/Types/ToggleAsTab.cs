using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

public class ToggleAsTab : IServiceUser, IMonoEnable, IMonoDisable, IMonoStart, IEZEventUser
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
        ObserveEvents();
        MyBranch.OpenBranchStartEvent += OnBranchEnter;
        MyBranch.ExitBranchStartEvent += OnBranchExit;
    }
    
    public void UseEZServiceLocator() => _inputScheme = EZService.Locator.Get<InputScheme>(this);

    public void ObserveEvents() => HistoryEvents.Do.Subscribe<IOnStart>(OnSceneStart);

    public void UnObserveEvents() => HistoryEvents.Do.Unsubscribe<IOnStart>(OnSceneStart);

    public void OnDisable()
    {
        if(!HasLink) return;

        UnObserveEvents();
        MyBranch.OpenBranchStartEvent -= OnBranchEnter;
        MyBranch.ExitBranchStartEvent -= OnBranchExit;
    }

    public void OnStart()
    {
        if (!HasLink) return;
        LinkBranch.ParentTrunk = MyBranch.ParentTrunk;
    }

    private void OnSceneStart(IOnStart args)
    {
        if(_myToggle.IsToggleSelected)
        {
            LinkBranch.MyCanvas.enabled = true;
        }
    }
    
    private void OnBranchEnter()
    {
        if (!_myToggle.IsToggleSelected) return;
        LinkBranch.DoNotTween();
    }
    
    public void NavigateToChildBranch()
    {
        if (!HasLink || MultiSelectAllowed) return;
        LinkBranch.OpenThisBranch(newParentBranch: MyBranch.MyParentBranch);
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