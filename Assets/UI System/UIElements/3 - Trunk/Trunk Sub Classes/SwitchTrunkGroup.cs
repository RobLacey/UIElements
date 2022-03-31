using System;
using System.Collections.Generic;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

public interface ISwitchTrunkGroup : IMonoEnable, IMonoDisable, ISwitch
{
    List<IBranch> ThisGroup { set; }
    IBranch CurrentBranch { get; }
    void OpenAllBranches(IBranch newParent);
    void CloseAllBranches(Action endOfClose);
}

public interface ISwitch
{
    List<INode> SwitchHistory { get; }
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
    private IBranch _lastActiveHomeBranch;
    private IDataHub _myDataHub;

    //Properties and Getters / Setters
    public List<IBranch> ThisGroup { private get; set; }
    public bool HasOnlyOneMember => ThisGroup.Count == 1;
    public IBranch CurrentBranch => ThisGroup[_index];
    public List<INode> SwitchHistory { get; private set; }

    public void ClearSwitchHistory() => SwitchHistory.Clear();

    //Main
    public void OnEnable()
    {
        ObserveEvents();
        UseEZServiceLocator();
    }

    public void OnDisable() => UnObserveEvents();

    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    public void ObserveEvents() => HistoryEvents.Do.Subscribe<IHighlightedNode>(SaveHighlighted);

    public void UnObserveEvents() => HistoryEvents.Do.Unsubscribe<IHighlightedNode>(SaveHighlighted);

    private void SaveHighlighted(IHighlightedNode args)
    {
        bool DontDoSearch()  => !ThisGroup.Contains(_myDataHub.ActiveBranch)  || args.Highlighted.MyBranch.IsInGameBranch();
        bool SameAsLastActive() => _lastActiveHomeBranch == args.Highlighted.MyBranch;

        if(SameAsLastActive() || DontDoSearch()) return;
        
        if(ThisGroup.Contains(args.Highlighted.MyBranch))
        {
            _index = ThisGroup.IndexOf(args.Highlighted.MyBranch);
            _lastActiveHomeBranch = ThisGroup[_index];
        }
    }
    
    public void DoSwitch(SwitchInputType switchInputType)
    {
        if(ThisGroup.Count <=1) return;
        
        switch (switchInputType)
        {
            case SwitchInputType.Positive:
                _index = _index.PositiveIterate(ThisGroup.Count);
                break;
            case SwitchInputType.Negative:
                _index = _index.NegativeIterate(ThisGroup.Count);
                break;
        }
        
        _lastActiveHomeBranch = ThisGroup[_index];
        ThisGroup[_index].OpenThisBranch();
    }

    public void OpenAllBranches(IBranch newParent)
    {
        foreach (var branch in ThisGroup)
        {
            if(branch == CurrentBranch) continue;
            branch.DontSetAsActiveBranch();
            branch.OpenThisBranch(newParent);
        }
        CurrentBranch.OpenThisBranch(newParent);
    }
    
    public void CloseAllBranches(Action endOfClose)
    {
        var counter = ThisGroup.Count;

        foreach (var branch in ThisGroup)
        {
            branch.ExitThisBranch(OutTweenType.Cancel, Complete);
        }

        void Complete()
        {
            counter--;
            if(counter == 0)
                endOfClose?.Invoke();
        }
    }

}