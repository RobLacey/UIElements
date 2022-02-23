using System;
using EZ.Inject;
using EZ.Service;

public interface IHistoryTrack : IParameters, IIsAService
{
    void OnEnable();
    bool NoHistory { get; }
    bool NodeNeededForMultiSelect(INode node);
    void GOUIBranchHasClosed(IBranch branchToClose, bool noGOUILeft = false);
    void ReturnToNextHomeGroup();
    void BackToHome();
    void BackOneLevel();
    void MoveToLastBranchInHistory();
    void CheckForPopUpsWhenCancelPressed(Action endOfCancelAction);
    void UpdateHistoryData(INode node);
    void BackToHomeScreen();
}