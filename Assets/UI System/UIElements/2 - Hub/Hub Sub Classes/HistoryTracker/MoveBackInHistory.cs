
using UnityEngine;

public static class MoveBackInHistory 
{
    public static void BackOneLevelProcess(HistoryData data)
    {
        if(data.NoHistory) return;
        var lastSelected = data.LastSelected();
        data.AddStopPoint(lastSelected);
        data.TweenType = OutTweenType.Cancel;
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
        lastSelected.MyBranch.OpenThisBranch();
    }

    public static void BackToHomeProcess(HistoryData data)
    {
        data.TweenType = OutTweenType.Cancel;
        data.SetToThisTrunkWhenFinished(data.RootTrunk);
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.All);
        data.RootTrunk.ActiveBranch.OpenThisBranch();
    }
}