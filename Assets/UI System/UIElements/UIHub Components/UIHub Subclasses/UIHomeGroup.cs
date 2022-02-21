using System;
using EZ.Events;
using EZ.Service;
using UIElements;

public interface IHomeGroup: IMonoEnable, IMonoDisable
{
    void SetUpHomeGroup();
    void SwitchHomeGroups(SwitchType switchType);
}

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup : IEZEventUser, IHomeGroup, IServiceUser
{
    //Variables
    private int _index = 0;
    private IBranch _lastActiveHomeBranch;
    private IBranch _activeBranch;
    private IHub _myUIHub;
    private IDataHub _myDataHub;

    //Properties and Getters / Setters
    private IBranch[] HomeGroup => _myUIHub.HomeBranches.ToArray();
    private bool GameIsPaused => _myDataHub.GamePaused;

    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        ObserveEvents();
    }
    
    public void OnDisable() => UnObserveEvents();

    public void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
        _myUIHub = EZService.Locator.Get<IHub>(this);
    }
    
    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IReturnToHome>(ActivateHomeGroupBranch);
        HistoryEvents.Do.Subscribe<IActiveBranch>(SetActiveHomeBranch);
        HistoryEvents.Do.Subscribe<IReturnHomeGroupIndex>(ReturnHomeGroup);
        HistoryEvents.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
    }

    public void UnObserveEvents() { }

    public void SetUpHomeGroup() => _activeBranch = HomeGroup[_index];

    private void SaveHighlighted(IHighlightedNode args)
    {
        if (IsHomeScreenBranchAndNoChildrenOpen())
        {
            SearchHomeBranchesAndSet(args.Highlighted.MyBranch);
        }

        bool IsHomeScreenBranchAndNoChildrenOpen() 
            => args.Highlighted.MyBranch.IsHomeScreenBranch() && _activeBranch.IsHomeScreenBranch();
    }

    private void ReturnHomeGroup(IReturnHomeGroupIndex args)
    {
        if (HomeGroup[_index].ThisGroupsUiNodes.Length == 0)
        {
            _index = _index.PositiveIterate(HomeGroup.Length);
        }
        args.TargetNode = HomeGroup[_index].LastHighlighted;
    }

    public void SwitchHomeGroups(SwitchType switchType)
    {
        if(HomeGroup.Length <=1) return;
        
        switch (switchType)
        {
            case SwitchType.Positive:
                _index = _index.PositiveIterate(HomeGroup.Length);
                break;
            case SwitchType.Negative:
                _index = _index.NegativeIterate(HomeGroup.Length);
                break;
            case SwitchType.Activate:
                break;
        }
        
        _lastActiveHomeBranch = HomeGroup[_index];
        HomeGroup[_index].MoveToThisBranch();
    }

    private void SetActiveHomeBranch(IActiveBranch args)
    {
        _activeBranch = args.ActiveBranch;
        if(DontDoSearch(_activeBranch)) return;
        if(_lastActiveHomeBranch == _activeBranch) return;
        
        _lastActiveHomeBranch = _activeBranch;
        FindHomeScreenBranch(_activeBranch);
    }

    private bool DontDoSearch(IBranch newBranch) 
        => newBranch.IsAPopUpBranch() || newBranch.IsPauseMenuBranch() 
                                              || newBranch.IsInGameBranch() 
                                              || GameIsPaused;

    private void FindHomeScreenBranch(IBranch newBranch)
    {
        while (!newBranch.IsHomeScreenBranch() && !DontDoSearch(newBranch))
        {
            newBranch = newBranch.MyParentBranch;
        }
        
        SearchHomeBranchesAndSet(newBranch);
    }

    private void SearchHomeBranchesAndSet(IBranch newBranch)
    {
        if(!newBranch.IsHomeScreenBranch()) return;

        _index = Array.IndexOf(HomeGroup, newBranch);
    }

    private void ActivateHomeGroupBranch(IReturnToHome args)
    {
        HomeGroup[_index].MoveToThisBranch();
        
        foreach (var branch in HomeGroup)
        {
            if(branch == HomeGroup[_index]) continue;
            branch.DontSetBranchAsActive();
            branch.MoveToThisBranch();
        }
    }
}
