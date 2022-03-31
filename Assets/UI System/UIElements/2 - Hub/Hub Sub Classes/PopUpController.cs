using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
///
public static class PopUpController
{
    public static IBranch NextPopUp(HistoryData data) => GetNextPopUp(data.HasActiveResolvePopUps 
                                                                          ? data.ActiveResolvePopUps : 
                                                                          data.ActiveOptionalPopUps);
    private static IBranch GetNextPopUp(List<IBranch> popUpList)
    {
        int index = popUpList.Count - 1;
        return popUpList[index];
    }
    
    public static void RemoveNextPopUp(HistoryData data, Action moveToLastBranchInHistory)
    {
        var popUpToRemove = NextPopUp(data);

        popUpToRemove.ExitThisBranch(OutTweenType.Cancel, moveToLastBranchInHistory);
    }

    public static void CloseExactPopUp(IBranch popUp, Action moveToLastBranchInHistory)
    {
        popUp.ExitThisBranch(OutTweenType.Cancel, moveToLastBranchInHistory);
    }
}
