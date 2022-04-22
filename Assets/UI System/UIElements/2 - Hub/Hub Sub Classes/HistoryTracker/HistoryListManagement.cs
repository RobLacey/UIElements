using System.Linq;
using UIElements.Hub_Sub_Classes.HistoryTracker;
using UnityEngine;

public static class HistoryListManagement
{
    public static void ResetAndClearHistoryList(HistoryData data, ClearAction action)
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
    
    private static void ClearHistory(HistoryData data)
    {
        for (int i = data.History.Count -1; i >= 0; i--)
        {
            var currentNode = data.History[i];
            ExitNode(data, currentNode);
        }
        data.SetToThisTrunk.OnStartTrunk();
    }

    private static void ClearHistoryWithStopPoint(HistoryData data)
    {
        var currentNode = data.History.Last();
        
        for (int i = data.History.Count -1; i >= 0; i--)
        {
            currentNode = data.History[i];
            ExitNode(data, currentNode);

            if (CheckIfAtStopPoint(data.StopPoint, currentNode)) break;

           // if (CheckIfStillInSameTrunk(data.NewNodesBranch, currentNode.MyBranch)) break;
        }
        data.EndOfTrunkCloseAction?.Invoke();
        //currentNode.MyBranch.ParentTrunk.OnStartTrunk();
    }
    private static bool CheckIfAtStopPoint(INode stopAtThisNode, INode currentNode) 
        => currentNode.MyBranch == stopAtThisNode.MyBranch;

    // private static bool CheckIfStillInSameTrunk(IBranch newNodeTrunk, IBranch currentTrunk) 
    //     => newNodeTrunk.ParentTrunk == currentTrunk.ParentTrunk;

    private static void ClearHistoryWithSkippedNode(HistoryData data)
    {
        for (int i = data.History.Count -1; i >= 0; i--)
        {
            var currentNode = data.History[i];
            if (CheckIfToSkip(currentNode, data.NewNode))
            {
                currentNode.MyBranch.OpenThisBranch();
                continue;
            }
            ExitNode(data, currentNode);
        }
    }
    
    private static bool CheckIfToSkip(INode currentNode, INode toSkip) => currentNode == toSkip;

    private static void ExitNode(HistoryData data,INode currentNode)
    {
        if(currentNode.HasChildBranch.IsNull())return;
        if(TrunkTracker.MoveBackATrunk(data, currentNode)) return;

        currentNode.HasChildBranch.ExitThisBranch(OutTweenType.Cancel, EndAction);

        void EndAction()
        {
            currentNode.ExitNodeByType();
            data.RemoveFromHistory(currentNode);
            if(currentNode != data.StopPoint) return;
            currentNode.MyBranch.OpenThisBranch();
        }
    }
}