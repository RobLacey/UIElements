using UIElements.Hub_Sub_Classes.HistoryTracker;
using UnityEngine;

public static class HistoryListManagement
{
    // public static void CloseToThisPointInHistory(SelectData dataSet)
    // {
    //     CloseAllChildNodesAfterPoint(dataSet);
    //     CloseThisLevel(dataSet, dataSet.NewNode);
    // }
    //
    // private static void CloseAllChildNodesAfterPoint(SelectData data)
    // {
    //     for (int i = data.History.Count -1; i >= 0; i--)
    //     {
    //         if (data.History[i] == data.NewNode) break;
    //         CloseThisLevel(data, data.History[i]);
    //     }
    // }
    //
    // public static void CloseThisLevel(SelectData data, INode node)
    // {
    //     RemoveFromHistoryData(data, node);
    //     ExitNode(data, node);
    //     // if(node.HasChildBranch.IsNull()) return;
    //     //
    //     // if(TrunkTracker.MovingToNewTrunk(data, CloseTrunkProcess)) return;
    //     //
    //     // node.HasChildBranch.LastSelected.ExitNodeByType();
    //     // node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
    //     //
    //     // void CloseTrunkProcess()
    //     // {
    //     //     node.HasChildBranch.LastSelected.ExitNodeByType();
    //     //     //node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
    //     // }
    // }
    
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
        data.MyDataHub.RootTrunk.StartRootTrunk();
    }

    private static void ClearHistoryWithStopPoint(SelectData data)
    {
        var stopNodeToExit = data.StopPoint;
        for (int i = data.History.Count -1; i >= 0; i--)
        {
            var currentNode = data.History[i];

            if (InSameTrunkButNothingNewSelectedYet(data, currentNode)) break;
            
            ExitNode(data, currentNode);
            RemoveFromHistoryData(data, currentNode);
            
            if (CheckIfOnSameParent(currentNode.MyBranch, data.NewNode.MyBranch))
            {
                stopNodeToExit = currentNode;
                break;
            }

            if (CheckIfAtStopPoint(data.StopPoint, currentNode)) break;
        }
        stopNodeToExit.ExitNodeByType();
        stopNodeToExit.MyBranch.MoveToThisBranch();
    }

    private static bool InSameTrunkButNothingNewSelectedYet(SelectData data, INode currentNode)
    {
        var inDifferentTrunks = currentNode.MyBranch.ParentTrunk != data.NewNodesTrunk;
        
        return data.SameTrunkButNothingSelected & inDifferentTrunks;
    }

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

    private static void ExitNode(SelectData data,INode node)
    {
        if(node.HasChildBranch.IsNull())return;
        
        if(TrunkTracker.MoveBackATrunk(data, EndOfExit)) return;

        //node.ExitNodeByType();
        node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel, EndOfExit);

        void EndOfExit()
        {
            node.ExitNodeByType();
            //node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
        }

        // if(currentNode.HasChildBranch.IsNotNull())
        //     currentNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
        //
        // currentNode.ExitNodeByType();
    }

    private static bool CheckIfToSkip(INode currentNode, INode toSkip) => currentNode == toSkip;

    private static bool CheckIfAtStopPoint(INode stopAtThisNode, INode currentNode)
    {
        return currentNode.MyBranch == stopAtThisNode.MyBranch;
    }
    
    private static bool CheckIfOnSameParent(IBranch newNodesParent, IBranch currentNodesParent)
    {
        return newNodesParent == currentNodesParent;
    }

    public static void RemoveFromHistoryData(SelectData data, INode currentNode)
    {
        data.HistoryTracker.UpdateHistoryData(currentNode);
        data.History.Remove(currentNode);
    }

    private static void ClearHistoryData(SelectData data)
    {
        data.HistoryTracker.UpdateHistoryData(null); // The null allows for the history to be cleared
        data.History.Clear();
    }
}