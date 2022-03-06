
using UnityEngine;

public static class MoveBackInHistory 
{
    public static void BackOneLevelProcess(SelectData data)
    {
        //var lastNode = data.History.Last();
        
        // if (IsHomeScreenBranch(lastNode, data.OnHomeScreen))
        // {
        //     return BackToHomeProcess(data);
        // }
        // if (TrunkTracker.MovingToNewTrunk(data, EndOfBackProcess))
        // {
        //     data.LastSelected.ExitNodeByType();
        //     data.NewNodesTrunk.OnStartTrunk();
        // }
        
        Debug.Log("Not right, calls things StartBranchExit twice and just needs to close the branches that are open");
        var lastSelected = data.LastSelected();
        data.LastSelected().ExitNodeByType();
        //data.MyDataHub.ActiveBranch.StartBranchExitProcess(OutTweenType.Cancel, EndOfBackProcess);
        data.ActiveBranch.StartBranchExitProcess(OutTweenType.Cancel,EndOfBackProcess);
       // DoBackOneLevel(data.LastSelected, data.MyDataHub);
        // EndOfBackProcess();
        //
        void EndOfBackProcess()
        {
            data.AddStopPoint(lastSelected);
            HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
            lastSelected.MyBranch.MoveToThisBranch();
            // data.HistoryTracker.UpdateHistoryData(data.LastSelected);
            // data.History.Remove(data.LastSelected);
            //return data.History.Count == 0 ? null : data.History.Last();
        }
    }

    // private static bool IsHomeScreenBranch(INode lastNode, bool onHomeScreen) 
    //     => lastNode.MyBranch.IsHomeScreenBranch() && !onHomeScreen;
    // private static bool IsHomeScreenBranch(INode lastNode, bool onHomeScreen) 
    //     => lastNode.MyBranch.IsHomeScreenBranch() && !onHomeScreen;
    
    // private static void DoBackOneLevel(INode lastNode, IDataHub myDataHub)
    // {
    //     if (lastNode.MyBranch.CanvasIsEnabled)
    //     {
    //         lastNode.ExitNodeByType();
    //         lastNode.MyBranch.DoNotTween();
    //     }
    //     else
    //     {
    //         if (myDataHub.ActiveBranch.AutoClose == IsActive.No)
    //             lastNode.ExitNodeByType();
    //     }
    //    // Debug.Log("Process : " + myDataHub.ActiveBranch);
    //
    //     myDataHub.ActiveBranch.StartBranchExitProcess(OutTweenType.Cancel, EndOfExit);
    //
    //     void EndOfExit()
    //     {
    //         // if (myDataHub.CurrentTrunk != lastNode.MyBranch.ParentTrunk)
    //         // {
    //         //     lastNode.MyBranch.ParentTrunk.OnStartTrunk();
    //         //     return;
    //         // }
    //         lastNode.MyBranch.MoveToThisBranch();
    //     }
    // }

    public static void BackToHomeProcess(SelectData data)
    {
        data.LastSelected().ExitNodeByType();
        data.MyDataHub.ActiveBranch.StartBranchExitProcess(OutTweenType.Cancel, CallBack);
       // return lastSelected;

        void CallBack() => BackHomeCallBack(data);
    }

    private static void BackHomeCallBack(SelectData data)
    {
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.All);
        data.MyDataHub.RootTrunk.OnStartTrunk();
        data.LastSelected().MyBranch.MoveToThisBranch();
       // data.HistoryTracker.BackToHomeScreen();
    }
}