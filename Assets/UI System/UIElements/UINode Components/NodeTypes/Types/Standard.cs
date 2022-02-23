
public interface IStandard : INodeBase { }

public class Standard : NodeBase, IStandard
{
    public Standard(INode uiNode) : base(uiNode)
    {
        _autoOpenDelay = _uiNode.AutoOpenDelay;
    }

    private bool _isToggle;
    private readonly IDelayTimer _delayTimer = EZInject.Class.NoParams<IDelayTimer>();
    private readonly float _autoOpenDelay;

    public override void OnEnteringNode()
    {
        base.OnEnteringNode();

        if (_uiNode.CanAutoOpen && !IsSelected)
        {
            _delayTimer.SetDelay(_autoOpenDelay)
                       .StartTimer(StartAutoOpen);
        }
    }

    public override void OnExitingNode()
    {
        base.OnExitingNode();
        if (_uiNode.CanAutoOpen)
        {
            _delayTimer.StopTimer();
        }
    }

    private void StartAutoOpen()
    {
        MyBranch.AutoOpenCloseClass.ChildNodeHasOpenChild = _uiNode.HasChildBranch;
        NodeSelected();
    }

    public override void ExitNodeByType()
    {
        if (!IsSelected) return;
        SetNodeAsNotSelected_NoEffects();

        if(PointerOverNode && !AllowKeys) return;
        OnExitingNode();
    }
}
