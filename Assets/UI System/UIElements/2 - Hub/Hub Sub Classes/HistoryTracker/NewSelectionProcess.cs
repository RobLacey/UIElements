
using UIElements.Hub_Sub_Classes.HistoryTracker;
using UnityEngine;

public static class NewSelectionProcess 
{
    public static void AddNewSelection(HistoryData data)
    {
        if (data.History.Contains(data.NewNode))
        {
            ContainsNewNode(data);
            return;
        }
        DoesntContainNewNode(data);
    }

    private static void ContainsNewNode(HistoryData data)
    {
        data.AddStopPoint(data.NewNode);
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
    }

    private static void DoesntContainNewNode(HistoryData data)
    {
        NodeInDifferentBranchAndNotAChildObject(data);
        data.AddToHistory(data.NewNode);
        NavigateToChildBranch(data);
    }

    private static void NavigateToChildBranch(HistoryData data)
    {
        if(data.NewNode.HasChildBranch.IsNull()) return;
        
        if(TrunkTracker.MovingToNewTrunk(data)) return;
        
        data.NewNode.MyBranch.ExitThisBranch(OutTweenType.MoveToChild, StartChild);
        
        void StartChild() => data.NewNode.HasChildBranch.OpenThisBranch(data.NewNodesBranch);
    }

    private static void NodeInDifferentBranchAndNotAChildObject(HistoryData data)
    {
        bool NodeIsInSameHierarchy() => data.LastSelected().HasChildBranch.IsNotNull() && 
                                        data.LastSelected().HasChildBranch.MyParentBranch == data.NewNode.MyBranch.MyParentBranch;
        
        if (data.NoHistory || NodeIsInSameHierarchy()) return;
        
        data.AddStopPoint(data.NewNode);
        data.TweenType = OutTweenType.MoveToChild;
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
    }
}