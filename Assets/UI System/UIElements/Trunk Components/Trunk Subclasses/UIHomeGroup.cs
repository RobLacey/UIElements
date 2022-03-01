using System;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

public interface IHomeGroup : IMonoEnable, IMonoDisable, ISwitch
{
    void ActivateHomeGroupBranch(IReturnToHome args);
}

public interface ISwitch
{
    void DoSwitch(SwitchInputType switchInputType);
    bool HasOnlyOneMember { get; }

}

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup : IEZEventUser, IHomeGroup, IServiceUser
{
    public UIHomeGroup(IHomeGroupSettings settings)
    {
        HomeGroup = settings.GroupsBranches.ToArray();
    }

    //Variables
    private int _index = 0;
    private IBranch _lastActiveHomeBranch;
    private IBranch _activeBranch;
    private IDataHub _myDataHub;

    //Properties and Getters / Setters
    private IBranch[] HomeGroup { get; }
    private bool GameIsPaused => _myDataHub.GamePaused;
    public bool HasOnlyOneMember => HomeGroup.Length == 1;

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
    }
    
    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IReturnToHome>(ActivateHomeGroupBranch);
        HistoryEvents.Do.Subscribe<IActiveBranch>(SetActiveHomeBranch);
        HistoryEvents.Do.Subscribe<IReturnHomeGroupIndex>(ReturnHomeGroup);
        HistoryEvents.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
    }

    public void UnObserveEvents()
    {
        HistoryEvents.Do.Unsubscribe<IReturnToHome>(ActivateHomeGroupBranch);
        HistoryEvents.Do.Unsubscribe<IActiveBranch>(SetActiveHomeBranch);
        HistoryEvents.Do.Unsubscribe<IReturnHomeGroupIndex>(ReturnHomeGroup);
        HistoryEvents.Do.Unsubscribe<IHighlightedNode>(SaveHighlighted);
    }

    private void SaveHighlighted(IHighlightedNode args)
    {
        if (_activeBranch.IsNull()) _activeBranch = HomeGroup[_index];
        
        if (IsHomeScreenBranchAndNoChildrenOpen())
        {
            SearchHomeBranchesAndSet(args.Highlighted.MyBranch);
        }

        bool IsHomeScreenBranchAndNoChildrenOpen() 
            => args.Highlighted.MyBranch.IsHomeScreenBranch() && _activeBranch.IsHomeScreenBranch();
    }

    private void ReturnHomeGroup(IReturnHomeGroupIndex args)
    {
        if (HomeGroup[_index].ThisBranchesNodes.Length == 0)
        {
            _index = _index.PositiveIterate(HomeGroup.Length);
        }
        args.TargetNode = HomeGroup[_index].LastHighlighted;
    }

    public void DoSwitch(SwitchInputType switchInputType)
    {
        if(HomeGroup.Length <=1) return;
        
        switch (switchInputType)
        {
            case SwitchInputType.Positive:
                _index = _index.PositiveIterate(HomeGroup.Length);
                break;
            case SwitchInputType.Negative:
                _index = _index.NegativeIterate(HomeGroup.Length);
                break;
            case SwitchInputType.Activate:
                HomeGroup[_index].MoveToThisBranch();
                _lastActiveHomeBranch = HomeGroup[_index];
                return;
        }
        
        _lastActiveHomeBranch = HomeGroup[_index];
        HomeGroup[_index].MoveToThisBranch();
    }

    private void SetActiveHomeBranch(IActiveBranch args)
    {
        // _activeBranch = _myDataHub.ActiveBranch;
        // if(DontDoSearch(_activeBranch)) return;
        // if(_lastActiveHomeBranch == _activeBranch) return;
        //
        // _lastActiveHomeBranch = _activeBranch;
        // FindHomeScreenBranch(_activeBranch);
    }

    private bool DontDoSearch(IBranch newBranch) 
        => newBranch.IsAPopUpBranch() || newBranch.IsPauseMenuBranch() 
                                              || newBranch.IsInGameBranch() 
                                              || GameIsPaused;

    // private void FindHomeScreenBranch(IBranch newBranch)
    // {
    //     while (!newBranch.IsHomeScreenBranch() && !DontDoSearch(newBranch))
    //     {
    //         newBranch = newBranch.MyParentBranch;
    //     }
    //     
    //     SearchHomeBranchesAndSet(newBranch);
    // }

    private void SearchHomeBranchesAndSet(IBranch newBranch)
    {
        if(!newBranch.IsHomeScreenBranch()) return;

        _index = Array.IndexOf(HomeGroup, newBranch);
    }

    public void ActivateHomeGroupBranch(IReturnToHome args)
    {
        Debug.Log(HomeGroup.Length + " : " + _index);
        HomeGroup[_index].MoveToThisBranch();
        
        foreach (var branch in HomeGroup)
        {
            if(branch == HomeGroup[_index]) continue;
            branch.DontSetBranchAsActive();
            branch.MoveToThisBranch();
        }
    }
}
