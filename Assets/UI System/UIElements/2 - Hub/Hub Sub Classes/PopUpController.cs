using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Class Looks after managing switching between PopUps
/// </summary>
///
public static class PopUpController
{
    public static IBranch NextPopUp(IDataHub data)
    {
        bool hasResolvePopUp = data.ActiveResolvePopUps.Count > 0;

        return GetNextPopUp(hasResolvePopUp ? data.ActiveResolvePopUps : data.ActiveOptionalPopUps);
    }

    private static IBranch GetNextPopUp(List<IBranch> popUpList)
    {
        int index = popUpList.Count - 1;
        return popUpList[index];
    }
    
    public static void RemoveNextPopUp(IDataHub data, Action moveToLastBranchInHistory)
    {
        bool hasResolvePopUp = data.ActiveResolvePopUps.Count > 0;

        var popUpToRemove = NextPopUp(data);

        if (hasResolvePopUp)
            popUpToRemove.StartBranchExitProcess(OutTweenType.Cancel, moveToLastBranchInHistory);
        
        popUpToRemove.StartBranchExitProcess(OutTweenType.Cancel, moveToLastBranchInHistory);
    }
}
