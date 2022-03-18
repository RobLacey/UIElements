using System.Collections.Generic;
using System.Linq;
using UIElements;
using UnityEngine;

public class SelectData
{
    public SelectData(IHistoryTrack historyTracker) => HistoryTracker = historyTracker;

    public void AddData(INode newNode)
    {
        NewNode = newNode;
        MultiSelectIsActive = false;
    }
    
    public void AddStopPoint(INode newNode)
    {
        StopPoint = newNode;
        MultiSelectIsActive = false;
    }
    
    public void AddMultiSelectData(INode newNode) => NewNode = newNode;

    public void MultiSelectOn() => MultiSelectIsActive = true;
    public void MultiSelectOff() => MultiSelectIsActive = false;
    public INode NewNode { get; private set; }
    public INode StopPoint { get; private set; }
    public INode LastSelected() => MyDataHub.History.Count > 0 ? MyDataHub.History.Last() : null;

    public List<INode> History => HistoryTracker.History;
    public bool NoHistory => History.Count == 0;
    public ScreenType ScreenTypeOfDestinationTrunk => NewNode.HasChildBranch.ParentTrunk.ScreenType;
    public Trunk DestinationTrunk => NewNode.HasChildBranch.ParentTrunk;
    public IBranch NewNodesBranch => NewNode.MyBranch;
    public Trunk CurrentTrunk => MyDataHub.CurrentTrunk;
    public bool MultiSelectIsActive { get; private set; }
    public IDataHub MyDataHub => HistoryTracker.MyDataHub;
    public IHistoryTrack HistoryTracker { get; }
    public OutTweenType TweenType { get; set; }

}