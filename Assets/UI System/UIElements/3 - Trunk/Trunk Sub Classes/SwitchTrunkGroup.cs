using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;


public interface ISwitchTrunkGroup : IMonoEnable, IMonoDisable, ISwitch
{
    Trunk ThisTrunk { set; }
    List<IBranch> ThisGroup { set; }
    IBranch CurrentBranch { get; }
    void SetNewIndex(INode newNode);
    void ActivateCurrentBranch();
    void OpenAllBranches(IBranch newParent, bool trunkCanTween);
    void CloseAllBranches(Action endOfClose, bool trunkCanTween);
}

public interface ISwitch
{
    List<Node> SwitchHistory { get; }
    void ClearSwitchHistory();
    void DoSwitch(SwitchInputType switchInputType);
    bool HasOnlyOneMember { get; }

}

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class SwitchTrunkGroup : IEZEventUser, ISwitchTrunkGroup, IServiceUser
{
    //Variables
    private int _index = 0;
    private IDataHub _myDataHub;

    //Properties and Getters / Setters
    public Trunk ThisTrunk { private get; set; }
    public List<IBranch> ThisGroup { private get; set; }
    public bool HasOnlyOneMember => ThisGroup.Count == 1;
    public IBranch CurrentBranch => ThisGroup[_index];
    public List<Node> SwitchHistory { get; } = new List<Node>();

    public void ClearSwitchHistory() => SwitchHistory.Clear();
    private bool DontPositiveLoop => ThisTrunk.DontLoopSwitcher & _index == ThisGroup.Count - 1;
    private bool DontNegativeLoop => ThisTrunk.DontLoopSwitcher & _index == 0;

    //Main
    public void OnEnable()
    {
        ObserveEvents();
        UseEZServiceLocator();
    }

    public void OnDisable() => UnObserveEvents();

    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
        HistoryEvents.Do.Subscribe<ISelectedNode>(NewSelected);
    }

    public void UnObserveEvents()
    {
        HistoryEvents.Do.Unsubscribe<IHighlightedNode>(SaveHighlighted);
        HistoryEvents.Do.Unsubscribe<ISelectedNode>(NewSelected);
    }
    
    private void SaveHighlighted(IHighlightedNode args)
    {
        bool DontDoSearch()  => _myDataHub.CurrentSwitchHistory.IsNotEmpty() || NotValidBranchType(args.Highlighted);
        
        if(!ThisTrunk.CanvasIsActive || SameAsLastActive(args.Highlighted) || DontDoSearch()) return;
        
        SetNewIndex(args.Highlighted);
    }

    private void NewSelected(ISelectedNode args)
    {
        if(!ThisTrunk.CanvasIsActive) return;
        if(SameAsLastActive(args.SelectedNode) || NotValidBranchType(args.SelectedNode)) return;
        
        SetNewIndex(args.SelectedNode);
    }
    
    private bool SameAsLastActive(INode nodeToCheck) => ThisGroup[_index] == nodeToCheck.MyBranch;

    private bool NotValidBranchType(INode nodeToCheck) =>
        nodeToCheck.MyBranch.IsInGameBranch() || nodeToCheck.MyBranch.IsAPopUpBranch();

    public void SetNewIndex(INode newNode)
    {
        if(ThisGroup.Contains(newNode.MyBranch))
            _index = ThisGroup.IndexOf(newNode.MyBranch);
    }

    public void DoSwitch(SwitchInputType switchInputType)
    {
        if(ThisGroup.Count <=1) return;
        
        switch (switchInputType)
        {
            case SwitchInputType.Positive:
                if(DontPositiveLoop) return;
                _index = _index.PositiveIterate(ThisGroup.Count);
                break;
            case SwitchInputType.Negative:
                if(DontNegativeLoop) return;
                _index = _index.NegativeIterate(ThisGroup.Count);
                break;
        }
        //_lastActiveHomeBranch = ThisGroup[_index];
        ThisGroup[_index].OpenThisBranch();
    }

    public void OpenAllBranches(IBranch newParent, bool trunkCanTween)
    {
        foreach (var branch in ThisGroup)
        {
            if(trunkCanTween)
                branch.DoNotTween();
            branch.DontSetAsActiveBranch();
            branch.OpenThisBranch(newParent);
        }
        //CurrentBranch.OpenThisBranch(newParent);
    }
    
    public void CloseAllBranches(Action endOfClose, bool trunkCanTween)
    {
        var counter = ThisGroup.Count;

        foreach (var branch in ThisGroup)
        {
            if(trunkCanTween)
            {
                branch.DoNotTween();
            }            
            branch.ExitThisBranch(OutTweenType.Cancel, Complete);
        }

        void Complete()
        {
            counter--;
            if(counter == 0)
            {
                endOfClose?.Invoke();
            }        
        }
    }
    
    public void ActivateCurrentBranch()
    {
        if(SwitchHistory.IsEmpty())
        {
            CurrentBranch.OpenThisBranch();
        }
        else
        {
            var last = SwitchHistory.Last();
            foreach (var node in SwitchHistory)
            {
                ProcessBranchOpen(node, last);
            }

            SwitchHistory.Remove(last);
        }
    }

    private static void ProcessBranchOpen(Node node, Node last)
    {
        if (!node.MyBranch.StayVisibleMovingToChild() && node != last) return;
        
        if (node != last)
            node.MyBranch.DontSetAsActiveBranch();
        
        node.MyBranch.OpenThisBranch();
    }
}