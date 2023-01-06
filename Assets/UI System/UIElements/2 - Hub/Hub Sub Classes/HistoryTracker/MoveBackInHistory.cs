
using System.Linq;
using UnityEngine;

public static class MoveBackInHistory 
{
    public static void BackOneLevelProcess(HistoryData data)
    {
        var lastSelected = data.LastSelected();
        data.AddStopPoint(lastSelected);
        data.TweenType = OutTweenType.Cancel;
        data.EndOfTrunkCloseAction = () => lastSelected.MyBranch.ParentTrunk.OnStartTrunk();
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
    }

    public static void BackToRootProcess(HistoryData data)
    {
        data.TweenType = OutTweenType.Cancel;
        data.SetToThisTrunkWhenFinished(data.RootTrunk);
        data.RootTrunk.SwitcherHistory.Clear();
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.All);
    }

    public static void BackToThisTrunkProcess(HistoryData data)
    {
        data.TweenType = OutTweenType.Cancel;
        if(data.CurrentSwitchHistory.IsEmpty())  return;
        data.AddStopPoint(data.CurrentSwitchHistory.First());
        HistoryListManagement.ResetAndClearHistoryList(data, ClearAction.StopAt);
    }
}