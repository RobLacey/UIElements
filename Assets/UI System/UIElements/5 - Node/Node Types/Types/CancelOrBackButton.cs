using System;
using UnityEngine;

public interface ICancelOrBack : INodeBase { }


public class CancelOrBackButton : NodeBase, ICancelButtonPressed, ICancelOrBack
{
    public CancelOrBackButton(INode node) : base(node)
    {
        MyBranch = node.MyBranch;
        EscapeKeyType = node.EscapeKeyType;
        _dontAddToHistoryTracking = true;
    }

    public EscapeKey EscapeKeyType { get; }

    //Events
    private Action<ICancelButtonPressed> CancelButtonActive { get; set; }
    public IBranch BranchToCancel => MyBranch;

    //Main
    public override void FetchEvents()
    {
        base.FetchEvents();
        CancelButtonActive = CancelEvents.Do.Fetch<ICancelButtonPressed>();
    }

    public override void NodeSelected()
    {
        base.NodeSelected();
        CancelButtonActive?.Invoke(this);
    }
}

