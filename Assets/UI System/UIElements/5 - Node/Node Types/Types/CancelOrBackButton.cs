using System;
using UnityEngine;

public interface ICancelOrBack : INodeBase { }


public class CancelOrBackButton : NodeBase, ICancelActivated, ICancelOrBack
{
    public CancelOrBackButton(INode node) : base(node)
    {
        MyBranch = node.MyBranch;
        EscapeKeyType = node.EscapeKeyType;
        _dontAddToHistoryTracking = true;
    }

    public EscapeKey EscapeKeyType { get; }

    //Events
    private Action<ICancelActivated> CancelButtonActive { get; set; }
    public IBranch BranchToCancel => MyBranch;

    //Main
    public override void FetchEvents()
    {
        base.FetchEvents();
        CancelButtonActive = CancelEvents.Do.Fetch<ICancelActivated>();
    }

    public override void NodeSelected()
    {
        base.NodeSelected();
        CancelButtonActive?.Invoke(this);
    }
}

