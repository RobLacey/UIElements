using System.Linq;

public static class MoveBackInHistory 
{
    public static INode BackOneLevelProcess(SelectData data)
    {
        var lastNode = data.History.Last();
        
        if (IsHomeScreenBranch(lastNode, data.OnHomeScreen))
        {
            return BackToHomeProcess(data);
        }
        
        DoBackOneLevel(lastNode, data.ActiveBranch);
        data.HistoryTracker.UpdateHistoryData(lastNode);
        data.History.Remove(lastNode);
        return data.History.Count == 0 ? null : data.History.Last();
    }

    private static bool IsHomeScreenBranch(INode lastNode, bool onHomeScreen) 
        => lastNode.MyBranch.IsHomeScreenBranch() && !onHomeScreen;
    
    private static void DoBackOneLevel(INode lastNode, IBranch activeBranch)
    {
        if (lastNode.MyBranch.CanvasIsEnabled)
        {
            activeBranch.StartBranchExitProcess(OutTweenType.Cancel, ParentVisible);
        }
        else
        {
            activeBranch.StartBranchExitProcess(OutTweenType.Cancel, ParentNotVisible);
        }

        void ParentNotVisible()
        {
            if (activeBranch.AutoClose == IsActive.No)
                lastNode.ExitNodeByType();
            lastNode.MyBranch.MoveToThisBranch();
        }

        void ParentVisible()
        {
            lastNode.ExitNodeByType();
            lastNode.MyBranch.DoNotTween();
            lastNode.MyBranch.MoveToThisBranch();
        }
    }

    public static INode BackToHomeProcess(SelectData data)
    {
        var lastSelected = data.History.First();
        StopNodeFlash();
        data.ActiveBranch.StartBranchExitProcess(OutTweenType.Cancel, CallBack);
        return lastSelected;

        void CallBack() => BackHomeCallBack(data);
        void StopNodeFlash() { lastSelected.ExitNodeByType(); }
    }

    private static void BackHomeCallBack(SelectData data)
    {
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.All);
        data.HistoryTracker.BackToHomeScreen();
    }
}