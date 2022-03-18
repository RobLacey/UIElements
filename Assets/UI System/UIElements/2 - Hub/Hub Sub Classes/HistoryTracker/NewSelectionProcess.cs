
using UIElements.Hub_Sub_Classes.HistoryTracker;
using UnityEngine;

public static class NewSelectionProcess 
{
    public static void AddNewSelection(SelectData data)
    {
        if (data.History.Contains(data.NewNode))
        {
            ContainsNewNode(data);
            return;
        }
        DoesntContainNewNode(data);
    }

    private static void ContainsNewNode(SelectData data)
    {
        data.AddStopPoint(data.NewNode);
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
    }

    private static void DoesntContainNewNode(SelectData data)
    {
        NodeInDifferentBranchAndNotAChildObject(data);
        NavigateToChildBranch(data);
        HistoryListManagement.AddHistoryData(data, data.NewNode);
    }

    private static void NavigateToChildBranch(SelectData data)
    {
        if(data.NewNode.HasChildBranch.IsNull()) return;
        
        if(TrunkTracker.MovingToNewTrunk(data)) return;
        
        data.NewNode.HasChildBranch.MoveToThisBranch(data.NewNodesBranch);
    }

    private static void NodeInDifferentBranchAndNotAChildObject(SelectData data)
    {
        bool NodeIsInSameHierarchy() => data.LastSelected().HasChildBranch.IsNotNull() && 
                                        data.LastSelected().HasChildBranch.MyParentBranch == data.NewNode.MyBranch.MyParentBranch;
        
        if (data.NoHistory || NodeIsInSameHierarchy()) return;
        
        data.AddStopPoint(data.NewNode);
        data.TweenType = OutTweenType.MoveToChild;
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
    }
}