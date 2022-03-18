using UIElements.Hub_Sub_Classes.HistoryTracker;
using UnityEngine;

public static class HistoryListManagement
{
    public static void ResetAndClearHistoryList(SelectData data, ClearAction action)
    {
        switch (action)
        {
            case ClearAction.All:
                ClearHistory(data);
                break;
            case ClearAction.StopAt:
                ClearHistoryWithStopPoint(data);
                break;
            case ClearAction.SkipOne:
                ClearHistoryWithSkippedNode(data);
                break;
        }
    }
    
    private static void ClearHistory(SelectData data)
    {
        for (int i = data.History.Count -1; i >= 0; i--)
        {
            var currentNode = data.History[i];
            ExitNode(data, currentNode);
        }
        ClearHistoryData(data);
        data.MyDataHub.RootTrunk.OnStartTrunk();
    }

    private static void ClearHistoryWithStopPoint(SelectData data)
    {
        for (int i = data.History.Count -1; i >= 0; i--)
        {
            var currentNode = data.History[i];

            ExitNode(data, currentNode);
            RemoveFromHistoryData(data, currentNode);
            
            if (CheckIfAtStopPoint(data.StopPoint, currentNode)) break;

            if (CheckIfStillInSameTrunk(data.NewNodesBranch, currentNode.MyBranch)) break;
        }
    }
    private static bool CheckIfAtStopPoint(INode stopAtThisNode, INode currentNode) 
        => currentNode.MyBranch == stopAtThisNode.MyBranch;

    private static bool CheckIfStillInSameTrunk(IBranch newNodeTrunk, IBranch currentTrunk) 
        => newNodeTrunk.ParentTrunk == currentTrunk.ParentTrunk;

    private static void ClearHistoryWithSkippedNode(SelectData data)
    {
        for (int i = data.History.Count -1; i >= 0; i--)
        {
            var currentNode = data.History[i];
            if (CheckIfToSkip(currentNode, data.NewNode))
            {
                currentNode.MyBranch.MoveToThisBranch();
                continue;
            }
            ExitNode(data, currentNode);
        }
        ClearHistoryData(data);
    }
    
    private static bool CheckIfToSkip(INode currentNode, INode toSkip) => currentNode == toSkip;

    private static void ExitNode(SelectData data,INode currentNode)
    {
        currentNode.ExitNodeByType();
        
        if(currentNode.HasChildBranch.IsNull())return;
        if(TrunkTracker.MoveBackATrunk(data, currentNode)) return;

        currentNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
    }

    private static void RemoveFromHistoryData(SelectData data, INode currentNode)
    {
        data.HistoryTracker.UpdateHistoryData(currentNode);
        data.History.Remove(currentNode);
    }
    
    public static void AddHistoryData(SelectData data, INode currentNode)
    {
        data.HistoryTracker.UpdateHistoryData(currentNode);
        data.History.Add(currentNode);
    }

    private static void ClearHistoryData(SelectData data)
    {
        data.HistoryTracker.UpdateHistoryData(null); // The null allows for the history to be cleared
        data.History.Clear();
    }
}