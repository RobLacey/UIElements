
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
    }

    public static IBranch BackToHomeProcess(HistoryData data)
    {
        //TODO Fix this process so it's the dame as Back One level. Doesn't need return
        data.TweenType = OutTweenType.Cancel;
        data.SetToThisTrunkWhenFinished(data.RootTrunk);
        data.RootTrunk.SwitcherHistory.Clear();
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.All);
        return data.RootTrunk.ActiveBranch;
    }
}