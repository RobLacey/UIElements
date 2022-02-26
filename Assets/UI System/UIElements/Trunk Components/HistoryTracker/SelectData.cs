using System.Collections.Generic;
using UnityEngine;

public class SelectData
{
    public SelectData(IHistoryTrack historyTracker) => HistoryTracker = historyTracker;

    public void AddData(INode newNode, INode lastSelected)
    {
        NewNode = newNode;
        LastSelected = lastSelected;
        MultiSelectIsActive = false;
    }
    
    public void AddMultiSelectData(INode newNode) => NewNode = newNode;

    public void MultiSelectOn() => MultiSelectIsActive = true;
    public void MultiSelectOff() => MultiSelectIsActive = false;
    public INode NewNode { get; private set; }
    public INode LastSelected { get; private set; }
    public List<INode> History => HistoryTracker.History;
    public List<IBranch> ResolvePopUps => HistoryTracker.MyDataHub.ActiveResolvePopUps;
    public List<IBranch> OptionalPopUps => HistoryTracker.MyDataHub.ActiveOptionalPopUps;
    public bool IsPaused => HistoryTracker.MyDataHub.GamePaused;
    public bool OnHomeScreen => HistoryTracker.MyDataHub.OnHomeScreen;
    public bool NoPopUps => HistoryTracker.MyDataHub.NoPopups;
    public bool MultiSelectIsActive { get; private set; }
    public IBranch ActiveBranch => HistoryTracker.MyDataHub.ActiveBranch;
    public IHistoryTrack HistoryTracker { get; }

}