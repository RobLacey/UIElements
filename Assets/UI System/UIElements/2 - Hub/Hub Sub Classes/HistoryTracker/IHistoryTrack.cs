using System;
using System.Collections.Generic;
using EZ.Inject;
using EZ.Service;

public interface IHistoryTrack : IParameters, IIsAService
{
    // IDataHub MyDataHub { get; }
    // List<INode> History { get; }
    void OnEnable();
    bool NodeNeededForMultiSelect(INode node);
    void CheckListsAndRemove(IBranch branchToClose);
    //void ReturnToNextHomeGroup();
    void SwitchGroupPressed();
    void ExitPause();
    void MoveToLastBranchInHistory();
    IBranch ReturnNextBranch();
    void CancelHasBeenPressed(EscapeKey cancelType, IBranch argsBranchToCancel);

    //void CancelUIButtonPressed(EscapeKey escapeKey, IBranch argsBranchToCancel);
    // void UpdateHistoryData(INode node);
    //void BackToHomeScreen();
}