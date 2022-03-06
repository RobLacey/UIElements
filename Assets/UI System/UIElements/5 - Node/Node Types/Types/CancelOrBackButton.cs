using System;
using UnityEngine;

public interface ICancelOrBack : INodeBase { }


public class CancelOrBackButton : NodeBase, ICancelButtonActivated, ICancelOrBack, ICancelHoverOver
{
    public CancelOrBackButton(INode node) : base(node)
    {
        MyBranch = node.MyBranch;
        EscapeKeyType = node.EscapeKeyType;
        _dontAddToHistoryTracking = true;
    }

    public EscapeKey EscapeKeyType { get; }

    //Events
    private Action<ICancelButtonActivated> CancelButtonActive { get; set; }
    private Action<ICancelHoverOver> CancelHoverOver { get; set; }

    //Main
    public override void FetchEvents()
    {
        base.FetchEvents();
        CancelButtonActive = CancelEvents.Do.Fetch<ICancelButtonActivated>();
        CancelHoverOver= CancelEvents.Do.Fetch<ICancelHoverOver>();
    }

    public override void NodeSelected()
    {
        if (CloseOnExit())
        {
            CancelHoverOver?.Invoke(this);
        }
        else
        {
            CancelButtonActive?.Invoke(this);
        }
        
        bool CloseOnExit() => MyBranch.AutoClose == IsActive.Yes;
    }
}

