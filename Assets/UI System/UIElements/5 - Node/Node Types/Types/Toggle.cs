
using UnityEngine;

public interface IToggle : INodeBase { }

public class Toggle : NodeBase, IToggle
{
    public Toggle(INode node) : base(node)
    {
        _toggleData = _uiNode.ToggleData;
        _dontAddToHistoryTracking = true;
        _toggleAsTab = new ToggleAsTab(_toggleData.LinkBranch, MyBranch, _uiNode, this);
        
        if(_toggleData.HasToggleGroup)
            _toggleData.ToggleGroupData.OnAwake();
    }

    //Variables
    private readonly ToggleData _toggleData;
    private readonly ToggleAsTab _toggleAsTab;
    private readonly IDelayTimer _delayTimer = EZInject.Class.NoParams<IDelayTimer>();
    
    //Properties, Getters & Setters
    public bool StartAsSelected => _toggleData.StartAsSelected == IsActive.Yes;
    private bool IfIsNotInToggleGroup() => !_toggleData.HasToggleGroup;
    public bool IsToggleSelected => IsSelected;
    public INode MyNode => _uiNode;

    //Main
    public override void OnEnable()
    {
        base.OnEnable();
        _toggleAsTab.OnEnable();
        if (StartAsSelected)
            IsSelected = true;
        if(_toggleData.HasToggleGroup)
        {
            _toggleData.ToggleGroupData.AddToGroupAndIsStartingPoint(this);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        _toggleAsTab.OnDisable();
    }

    public override void OnStart()
    {
        base.OnStart();
        _toggleAsTab.OnStart();
        
        if(_toggleData.HasToggleGroup)
        {
            _toggleData.ToggleGroupData.OnStart();
            SetUpToggleGroup();
        }
        
    }

    private void SetUpToggleGroup()
    {
        if(_toggleData.ToggleGroupData.StartingNode == MyNode)
            SetNodeAsSelected_NoEffects();
    }

    public override void OnEnteringNode()
    {
        base.OnEnteringNode();
        
        ActivateToggleSwitcher();     
        
        if(IfIsNotInToggleGroup()) return;
        if (_uiNode.CanAutoOpen && !IsSelected)
        {
            _delayTimer.SetDelay(_uiNode.AutoOpenDelay)
                       .StartTimer(StartAutoOpen);
        }
    }

    private void ActivateToggleSwitcher()
    {
        if (!_toggleData.HasToggleGroup) return;
        
        _toggleData.ToggleGroupData.ActivateTabSwitcher();
        _toggleData.ToggleGroupData.SetNewSwitcherIndex(this);
    }

    private void StartAutoOpen()
    {
        MyBranch.AutoOpenCloseClass.ChildNodeHasOpenChild = _uiNode.HasChildBranch;
        NodeSelected();
    }

    public override void OnExitingNode()
    {
        base.OnExitingNode();

        if(IfIsNotInToggleGroup()) return;
        
        if (_uiNode.CanAutoOpen)
            _delayTimer.StopTimer();
    }

    public override void SetNodeAsActive()
    {
        base.SetNodeAsActive();
        ActivateToggleSwitcher();
        
        if (!IsSelected) return;
        
        if(IfIsNotInToggleGroup()) return;
        _toggleAsTab.NavigateToChildBranch();
    }

    public override void SetNodeAsNotSelected_NoEffects()
    {
        base.SetNodeAsNotSelected_NoEffects();
        
       if(IfIsNotInToggleGroup()) return;
       _toggleAsTab.ClearOpenLinkedBranches();
    }


    public override void NodeSelected()
    {
        if (IfIsNotInToggleGroup())
        {
            base.NodeSelected();
            return;
        }
        
        if (IsSelected) return;
       _toggleData.ToggleGroupData.TurnOffOtherTogglesInGroup(this);
        _toggleAsTab.NavigateToChildBranch();
        Activate();
    }
}