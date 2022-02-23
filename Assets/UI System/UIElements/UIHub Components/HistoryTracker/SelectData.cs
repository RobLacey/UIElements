using System.Collections.Generic;
using UnityEngine;

public class SelectData
{
    public SelectData(IHistoryTrack historyTracker)
    {
        HistoryTracker = historyTracker;
        NewNode = null;
        LastSelected = null;
        History = null;
        IsPaused = false;
        OnHomeScreen = false;
        ActiveBranch = null;
    }

    public void AddData(INode newNode, INode lastSelected, List<INode> history, bool isPaused = false)
    {
        NewNode = newNode;
        LastSelected = lastSelected;
        History = history;
        IsPaused = isPaused;
        MultiSelectIsActive = false;
        CurrentGroup = MultiSelectGroup.None;
    }

    public void AddMoveBackData(List<INode> history, IBranch activeBranch, bool onHomeScreen)
    {
        ActiveBranch = activeBranch;
        History = history;
        OnHomeScreen = onHomeScreen;
    }

    public void AddMultiSelectData(INode newNode, List<INode> history)
    {
        NewNode = newNode;
        History = history;
    }

    public void MultiSelectOn() => MultiSelectIsActive = true;
    public void MultiSelectOff() => MultiSelectIsActive = false;
    public void SetCurrentGroup(MultiSelectGroup group) => CurrentGroup = group;

    public INode NewNode { get; private set; }
    public INode LastSelected { get; private set; }
    public List<INode> History { get; private set; }
    public bool IsPaused { get; private set; }
    public bool OnHomeScreen { get; private set; }
    public bool MultiSelectIsActive { get; private set; }
    public MultiSelectGroup CurrentGroup { get; private set; }
    public IBranch ActiveBranch { get; private set; }
    public IHistoryTrack HistoryTracker { get; }

}