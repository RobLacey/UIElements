
using UnityEngine;

public static class MoveBackInHistory 
{
    public static void BackOneLevelProcess(SelectData data)
    {
        if(data.NoHistory) return;
        var lastSelected = data.LastSelected();
        data.AddStopPoint(lastSelected);
        data.TweenType = OutTweenType.Cancel;
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
        lastSelected.MyBranch.MoveToThisBranch();
    }

    public static void BackToHomeProcess(SelectData data)
    {
        //if(data.NoHistory) return;
        data.TweenType = OutTweenType.Cancel;
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.All);
        data.MyDataHub.RootTrunk.OnStartTrunk();
        data.MyDataHub.RootTrunk.ActiveBranch.MoveToThisBranch();
    }
}