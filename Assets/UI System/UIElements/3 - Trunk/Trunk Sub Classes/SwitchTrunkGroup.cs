using System;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

public interface ISwitchTrunkGroup : IMonoEnable, IMonoDisable, ISwitch
{
    IBranch CurrentBranch { get; }
    void OpenAllBranches();
    void CloseAllBranches(Action endOfClose);
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
public class SwitchTrunkGroup : IEZEventUser, ISwitchTrunkGroup, IServiceUser
{
    public SwitchTrunkGroup(ITrunkData settings)
    {
        ThisGroup = settings.GroupsBranches.ToArray();
    }

    //Variables
    private int _index = 0;
    private IBranch _lastActiveHomeBranch;
    private IBranch _activeBranch;
    private IDataHub _myDataHub;

    //Properties and Getters / Setters
    private IBranch[] ThisGroup { get; }
    private bool GameIsPaused => _myDataHub.GamePaused;
    public bool HasOnlyOneMember => ThisGroup.Length == 1;
    public IBranch CurrentBranch => ThisGroup[_index];

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
        //HistoryEvents.Do.Subscribe<IReturnToHome>(ActivateHomeGroupBranch);
       // HistoryEvents.Do.Subscribe<IActiveBranch>(SetActiveHomeBranch);
        HistoryEvents.Do.Subscribe<IReturnHomeGroupIndex>(ReturnHomeGroup);
        HistoryEvents.Do.Subscribe<ISelectedNode>(SaveHighlighted);

       // HistoryEvents.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
    }

    public void UnObserveEvents()
    {
       // HistoryEvents.Do.Unsubscribe<IReturnToHome>(ActivateHomeGroupBranch);
       // HistoryEvents.Do.Unsubscribe<IActiveBranch>(SetActiveHomeBranch);
        HistoryEvents.Do.Unsubscribe<IReturnHomeGroupIndex>(ReturnHomeGroup);
        HistoryEvents.Do.Unsubscribe<ISelectedNode>(SaveHighlighted);
    }

    private void SaveHighlighted(ISelectedNode args)
    {
        bool DontDoSearch()  => args.SelectedNode.MyBranch.IsAPopUpBranch() || args.SelectedNode.MyBranch.IsInGameBranch();
        bool SameAsLastActive() => _lastActiveHomeBranch == args.SelectedNode.MyBranch;

        if(SameAsLastActive() || DontDoSearch()) return;
        
        if(ThisGroup.Contains(args.SelectedNode.MyBranch))
        {
            _index = Array.IndexOf(ThisGroup, args.SelectedNode.MyBranch);
            _lastActiveHomeBranch = ThisGroup[_index];
            //_activeBranch = args.ActiveBranch;
        }

        // Debug.Log("Set to : " + HomeGroup[_index] + " : " + args.ActiveBranch);
        //
        // if (_activeBranch.IsNull()) _activeBranch = HomeGroup[_index];
        //
        // if (IsHomeScreenBranchAndNoChildrenOpen())
        // {
        //     SearchHomeBranchesAndSet(args.Highlighted.MyBranch);
        // }
        //
        // bool IsHomeScreenBranchAndNoChildrenOpen() 
        //     => args.Highlighted.MyBranch.IsHomeScreenBranch() && _activeBranch.IsHomeScreenBranch();
    }
    


    private void ReturnHomeGroup(IReturnHomeGroupIndex args)
    {
        bool BranchHasNoNodes() => ThisGroup[_index].ThisBranchesNodes.Length == 0;
        if (BranchHasNoNodes())
        {
            DoSwitch(SwitchInputType.Positive);
           // _index = _index.PositiveIterate(HomeGroup.Length);
        }
       // args.TargetNode = HomeGroup[_index].LastHighlighted;
    }

    public void DoSwitch(SwitchInputType switchInputType)
    {
        if(ThisGroup.Length <=1) return;
        
        switch (switchInputType)
        {
            case SwitchInputType.Positive:
                _index = _index.PositiveIterate(ThisGroup.Length);
                break;
            case SwitchInputType.Negative:
                _index = _index.NegativeIterate(ThisGroup.Length);
                break;
            case SwitchInputType.Activate:
                ThisGroup[_index].MoveToThisBranch();
                _lastActiveHomeBranch = ThisGroup[_index];
                return;
        }
        
        _lastActiveHomeBranch = ThisGroup[_index];
        ThisGroup[_index].MoveToThisBranch();
    }

    // private void SetActiveHomeBranch(IActiveBranch args)
    // {
    //     // if(_lastActiveHomeBranch == args.ActiveBranch || DontDoSearch(args.ActiveBranch)) return;
    //     //
    //     // if(HomeGroup.Contains(args.ActiveBranch))
    //     // {
    //     //     _index = Array.IndexOf(HomeGroup, args.ActiveBranch);
    //     //     //_activeBranch = args.ActiveBranch;
    //     // }
    //     // Debug.Log("Set to : " + HomeGroup[_index] + " : " + args.ActiveBranch);
    //     // _activeBranch = _myDataHub.ActiveBranch;
    //     // if(DontDoSearch(_activeBranch)) return;
    //     // if(_lastActiveHomeBranch == _activeBranch) return;
    //     //
    //     // _lastActiveHomeBranch = _activeBranch;
    //     // FindHomeScreenBranch(_activeBranch);
    // }


    // private void FindHomeScreenBranch(IBranch newBranch)
    // {
    //     while (!newBranch.IsHomeScreenBranch() && !DontDoSearch(newBranch))
    //     {
    //         newBranch = newBranch.MyParentBranch;
    //     }
    //     
    //     SearchHomeBranchesAndSet(newBranch);
    // }

    // private void SearchHomeBranchesAndSet(IBranch newBranch)
    // {
    //     if(!newBranch.IsHomeScreenBranch()) return;
    //
    //     _index = Array.IndexOf(HomeGroup, newBranch);
    // }

    // public void ActivateHomeGroupBranch(IReturnToHome args)
    // {
    //     // HomeGroup[_index].MoveToThisBranch();
    //     //
    //     // foreach (var branch in HomeGroup)
    //     // {
    //     //     if(branch == HomeGroup[_index]) continue;
    //     //     branch.DontSetBranchAsActive();
    //     //     branch.MoveToThisBranch();
    //     // }
    // }
    public void OpenAllBranches()
    {
        //HomeGroup[_index].MoveToThisBranch();
        
        foreach (var branch in ThisGroup)
        {
            //if(branch == ThisGroup[_index])continue;    
            branch.DontSetBranchAsActive();
            branch.MoveToThisBranch();
        }
        //ThisGroup[_index].MoveToThisBranch();
    }
    public void CloseAllBranches(Action endOfClose)
    {
        foreach (var branch in ThisGroup)
        {
            if(branch == ThisGroup[_index]) continue;
            branch.StartBranchExitProcess(OutTweenType.Cancel);
        }
        ThisGroup[_index].StartBranchExitProcess(OutTweenType.Cancel, EndOfClose);

        void EndOfClose()
        {
            endOfClose.Invoke();
        }
    }
}
