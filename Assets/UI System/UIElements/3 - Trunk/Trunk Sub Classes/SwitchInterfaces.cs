using System;
using System.Collections.Generic;
using UIElements;

public interface ISwitch
{
    List<Node> SwitchHistory { get; }
    void ClearSwitchHistory();
    void DoSwitch(SwitchInputType switchInputType);
    bool HasOnlyOneMember { get; }

}

public interface ISwitchTrunkGroup : IMonoEnable, IMonoDisable, ISwitch
{
    Trunk ThisTrunk { set; }
    List<IBranch> ThisGroup { set; }
    IBranch CurrentBranch { get; }
    void SetNewIndex(INode newNode);
    void UpdateSwitchHistory();
    void ActivateCurrentBranch();
    void OpenAllBranches(IBranch newParent, bool trunkCanTween, Action endAction);
    void CloseAllBranches(Action endOfClose, bool trunkCanTween);
}