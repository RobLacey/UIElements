using System;

public interface IInGameNode : INodeBase { }

public class InGameNode : NodeBase, IInGameNode, ICloseThisGOUIModule
{
    public InGameNode(INode node) : base(node)
    {
        _autoOpenDelay = _uiNode.AutoOpenDelay;
    }

    //Variables
    private readonly float _autoOpenDelay;
    private readonly IDelayTimer _delayTimer = EZInject.Class.NoParams<IDelayTimer>();

    //Properties
    public IBranch TargetBranch => MyBranch.MyParentBranch;

    //Events
    private Action<ICloseThisGOUIModule> CloseGOUIModule { get; set; }

    public override void FetchEvents()
    {
        base.FetchEvents();
        CloseGOUIModule = GOUIEvents.Do.Fetch<ICloseThisGOUIModule>();
    }

    public override void SetUpGOUIParent(IGOUIModule module) => _uiNode.InGameObject = module.GOUITransform.gameObject;

    public override void NodeSelected()
    {
        if(!AllowKeys)
            MyBranch.OpenThisBranch();
        base.NodeSelected();
    }

    public override void OnEnteringNode()
    {
        base.OnEnteringNode();
        
        if (_uiNode.CanAutoOpen && !IsSelected)
        {
            _delayTimer.SetDelay(_autoOpenDelay)
                       .StartTimer(StartAutoOpen);
        }
    }

    private void StartAutoOpen()
    {
        MyBranch.AutoOpenCloseClass.ChildNodeHasOpenChild = _uiNode.HasChildBranch;
        NodeSelected();
    }

    public override void OnExitingNode()
    {
        base.OnExitingNode();
        if (_uiNode.CanAutoOpen && IsSelected)
        {
            _delayTimer.StopTimer();
        }
    }
    
    public override void ExitNodeByType()
    {
        if (!IsSelected) return;
        SetNodeAsNotSelected_NoEffects();
        
        CloseGOUIModule?.Invoke(this);
        if(PointerOverNode && !AllowKeys) return;
        OnExitingNode();
    }

    public override void SetNodeAsNotSelected_NoEffects()
    {
        base.SetNodeAsNotSelected_NoEffects();
        CloseGOUIModule?.Invoke(this);
    }
}

