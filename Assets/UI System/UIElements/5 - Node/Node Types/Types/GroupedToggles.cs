using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Service;
using UnityEngine;

public interface IGroupedToggles : INodeBase { }

public class GroupedToggles : NodeBase, IGroupedToggles
{
    public GroupedToggles(INode node) : base(node)
    {
        _toggleData = _uiNode.ToggleData;
        SetUpOtherVariables();
       // SelectedToggle += SaveSelectedNode;
        _delayTimer = EZInject.Class.NoParams<IDelayTimer>();
    }

    // private void SetUpToggleData()
    // {
    //     _toggleData = _uiNode.ToggleData;
    //     //TabBranch = _uiNode.HasChildBranch;
    //     // _tabBranch = toggleData.ReturnTabBranch;
    //     //MyToggleGroupId = toggleData.ReturnToggleId;
    //     _startAsSelected = toggleData.StartAsSelected == IsActive.Yes;
    // }

    private void SetUpOtherVariables()
    {
        //_hasATabBranch = TabBranch != null;
        _autoOpenDelay = _uiNode.AutoOpenDelay;
       // _dontAddToHistoryTracking = true;
         _dontAddToHistoryTracking = _uiNode.HasChildBranch.IsNull();
    }

    //Variables
    private readonly List<INode> _toggleGroupMembers = new List<INode>();
    private int _hasAGroupStartPoint;
    private ToggleData _toggleData;
    private float _autoOpenDelay;
    private readonly IDelayTimer _delayTimer;
    
    //Properties
    private List<INode> AllNodes => _uiNode.MyBranch.ParentTrunk.GetComponentsInChildren<INode>().ToList();
    private IBranch TabBranch => _uiNode.HasChildBranch;
    private ToggleGroup MyToggleGroupId => _toggleData.ReturnToggleId;
    private bool StartAsSelected => _toggleData.StartAsSelected == IsActive.Yes;

    //Events
   // private static event Action<INode, Action> SelectedToggle;

    //Main
    public override void OnStart()
    {
        base.OnStart();
        SetUpToggleGroup();
        //SetUpTabBranch();
        if (StartAsSelected)
        {
            SetNodeAsSelected_NoEffects();
            OnExitingNode();
            //TurnOnTab();
        }
    }
    
    private void SetUpToggleGroup()
    {
        foreach (var node in AllNodes.Where(node => node.IsToggleGroup)
                                      .Where(node => MyToggleGroupId == node.ToggleData.ReturnToggleId))
        {
            _toggleGroupMembers.Add(node);
            CheckForStartPosition(node);
        }

        AssignStartIfNonExists();
        _toggleGroupMembers.Remove(_uiNode); //Removes themselves and not needed
    }

    private void AssignStartIfNonExists()
    {
        if (_hasAGroupStartPoint != 0 || _toggleGroupMembers.First() != _uiNode) return;
        SetStartAsSelected();
    }

    private void SetStartAsSelected()
    {
        _uiNode.ToggleData.SetStartAsSelected();
        //_startAsSelected = true;
    }

    private void CheckForStartPosition(INode node)
    {
        if (node.ToggleData.StartAsSelected == IsActive.Yes)
        {
            _hasAGroupStartPoint++;
        }        
        else if (_hasAGroupStartPoint > 1)
        {
            throw new Exception("To many start Point : " + _uiNode);
        }
    }

    // private void SetUpTabBranch()
    // {
    //     if (_hasATabBranch)
    //         TabBranch.SetUpAsTabBranch();
    // }

    public override void SetNodeAsActive()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Add a check that cancel is pressed when it comes here so it can move back again");
            Debug.Log("Escape pressed : " + _uiNode);
        }
       base.SetNodeAsActive();
       // if (TabBranch.IsNotNull() && !TabBranch.MyCanvas.enabled)
       // {
       //     Activate(DoPressOnNode);
       //     Debug.Log("Activate : " + _uiNode);
       // }
        if(IsSelected)
        {
            SetNodeAsSelected_NoEffects();
        }
    }

    public override void OnEnteringNode()
    {
        base.OnEnteringNode();
        
        if (_uiNode.CanAutoOpen && !IsSelected)
        {
            _delayTimer.SetDelay(_autoOpenDelay)
                       .StartTimer(NodeSelected);
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
    
    // private void SaveSelectedNode(INode newNode, Action callback)
    // {
    //     Debug.Log(_uiNode);
    //     if (!_toggleGroupMembers.Contains(newNode)) return;
    //     if (!IsSelected) return;
    //     SetAsNotActive(callback);
    // }

    public override void NodeSelected()
    {
        if (IsSelected) return;
        TurnOffOtherTogglesInGroup();
        Activate(DoPressOnNode);
        //TurnOnTab();
    }

    public override void ExitNodeByType()
    {
        //base.ExitNodeByType();
        if (_uiNode.CanAutoOpen)
        {
            _delayTimer.StopTimer();
        }
    }

    // private void SetAsNotActive(Action callback = null)
    // {
    //     SetNodeAsNotSelected_NoEffects();
    //     if (!_hasATabBranch) return;
    //     TabBranch.StartBranchExitProcess(OutTweenType.Cancel, callback);
    // }

    private void TurnOffOtherTogglesInGroup()
    {
        foreach (var toggleGroupMember in _toggleGroupMembers)
        {

            toggleGroupMember.ClearNode();
            
        }
        //SelectedToggle?.Invoke(_uiNode, TurnOnTab);
    }
    
    

    // private void TurnOnTab()
    // {
    //     if (!_hasATabBranch) return;
    //     TabBranch.DontSetBranchAsActive();
    //     TabBranch.MoveToThisBranch();
    // }
}
