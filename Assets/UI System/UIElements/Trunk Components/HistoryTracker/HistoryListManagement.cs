
using UnityEngine;

public static class HistoryListManagement
{
    public static void CloseToThisPointInHistory(SelectData dataSet)
    {
        CloseAllChildNodesAfterPoint(dataSet);
        CloseThisLevel(dataSet, dataSet.NewNode);
    }

    private static void CloseAllChildNodesAfterPoint(SelectData data)
    {
        if (!data.History.Contains(data.NewNode)) return;
        
        for (int i = data.History.Count -1; i >= 0; i--)
        {
            if (data.History[i] == data.NewNode) break;
            CloseThisLevel(data, data.History[i]);
        }
    }

    public static void CloseThisLevel(SelectData data, INode node)
    {
        RemoveFromHistoryData(data, node);
        
        if(node.HasChildBranch.IsNull()) return;
        node.HasChildBranch.LastSelected.ExitNodeByType();
        node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
    }
    
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
            ExitNode(currentNode);
        }
        ClearHistoryData(data);
    }

    private static void ClearHistoryWithStopPoint(SelectData data)
    {
        for (int i = data.History.Count -1; i >= 0; i--)
        {
            var currentNode = data.History[i];
            ExitNode(currentNode);
            RemoveFromHistoryData(data, currentNode);
            if (CheckIfAtStopPoint(data, data.NewNode, currentNode)) break;
        }
    }

    private static void ClearHistoryWithSkippedNode(SelectData data)
    {
        for (int i = data.History.Count -1; i >= 0; i--)
        {
            var currentNode = data.History[i];
            if (CheckIfToSkip(currentNode, data.NewNode)) continue;
            ExitNode(currentNode);
        }
        ClearHistoryData(data);
    }

    private static void ExitNode(INode currentNode)
    {
        if(currentNode.HasChildBranch.IsNotNull())
            currentNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
        
        currentNode.ExitNodeByType();
    }

    private static bool CheckIfToSkip(INode currentNode, INode toSkip) => currentNode == toSkip;

    private static bool CheckIfAtStopPoint(SelectData data, INode stopAtThisNode, INode currentNode)
    {
        if (currentNode.MyBranch != stopAtThisNode.MyBranch) return false;
        
        currentNode.ExitNodeByType();
        return true;
    }

    private static void RemoveFromHistoryData(SelectData data, INode currentNode)
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