
using System;
using UIElements.Hub_Sub_Classes.HistoryTracker;

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
        NodeInDifferentBranchAndNotAChildObject(data, EndOfTrunkClose);
        
        void EndOfTrunkClose()
        {
            data.AddToHistory(data.NewNode);
            NavigateToChildBranch(data);
            data.EndOfTrunkCloseAction = null;
        }
    }

    private static void NavigateToChildBranch(HistoryData data)
    {
        //if(data.NewNode.HasChildBranch.IsNull()) return;
        
        if(TrunkTracker.MovingToNewTrunk(data)) return;
        
        data.NewNode.MyBranch.ExitThisBranch(OutTweenType.MoveToChild, StartChild);
        
        void StartChild() => data.NewNode.HasChildBranch.OpenThisBranch(data.NewNodesBranch);
    }

    private static void NodeInDifferentBranchAndNotAChildObject(HistoryData data, Action endOfTrunkClose)
    {
        bool NodeIsInSameHierarchy() => data.LastSelected().HasChildBranch.IsNotNull() && 
                                        data.LastSelected().HasChildBranch.MyParentBranch == data.NewNode.MyBranch.MyParentBranch;
        
        if (data.NoHistory || NodeIsInSameHierarchy())
        {
            endOfTrunkClose?.Invoke();
            return;
        }

        data.EndOfTrunkCloseAction = endOfTrunkClose;
        NewBranchIsAlreadyOpen(data);
        data.AddStopPoint(data.NewNode);
        data.TweenType = OutTweenType.MoveToChild;
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
    }

    private static void NewBranchIsAlreadyOpen(HistoryData data)
    {
        if (data.NewNode.HasChildBranch == data.ActiveBranch)
            data.ActiveBranch.DoNotTween();
    }
}