
using System.Linq;

public static class NewSelectionProcess 
{
    public static INode AddNewSelection(SelectData data)
    {
        if (data.History.Contains(data.NewNode))
        {
            ContainsNewNode(data);
            return data.History.Count == 0 ? null : data.History.Last();
        }
        DoesntContainNewNode(data);
        return data.NewNode;
    }

    private static void ContainsNewNode(SelectData data) => HistoryListManagement.CloseToThisPointInHistory(data);

    private static void DoesntContainNewNode(SelectData data)
    {
        if (!data.IsPaused & data.History.Count > 0)
            SelectedNodeInDifferentBranch(data);

        data.HistoryTracker.UpdateHistoryData(data.NewNode);
        data.History.Add(data.NewNode);
    }
    
    private static void SelectedNodeInDifferentBranch(SelectData data)
    {
        bool IfNewBranchIsNotAnInternalBranch() => data.LastSelected.HasChildBranch != data.NewNode.MyBranch;
    
        if (IfNewBranchIsNotAnInternalBranch())
        {
            HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.All);
        }
    }

}