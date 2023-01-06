﻿using System;
using EZ.Events;
using EZ.Service;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class NodeBase : INodeBase, IEZEventDispatcher, /*ISelectedNode, IHighlightedNode,*/ IServiceUser
{
    protected NodeBase(INode node)
    {
        _uiNode = node;
        _uiFunctionEvents = _uiNode.UINodeEvents;
        MyBranch = _uiNode.MyBranch;
        //_dontAddToHistoryTracking = node.CanNotStoreNodeInHistory;
    }
    
    //Variables
    protected readonly INode _uiNode;
    private bool _hasFinishedSetUp;
    protected IDataHub _myDataHub;
    private readonly IUiEvents _uiFunctionEvents;
    private bool _fromHotKey;
    private IHistoryTrack _historyTracker;
    protected bool _dontAddToHistoryTracking;
    private HotKeys _popUpBranch;

    //Events
    // private Action<IHighlightedNode> DoHighlighted { get; set; } 
    //
    // private Action<ISelectedNode> DoSelected { get; set; }

    //Properties
    private INode LastHighlighted => _myDataHub.Highlighted;
    private bool InMenu => _myDataHub.InMenu;
    protected bool AllowKeys => _myDataHub.AllowKeys;
    protected bool PointerOverNode { get; private set; }
    protected IBranch MyBranch { get; set; }
    public UINavigation Navigation { get; set; }
    protected bool IsSelected { get; set; }
    // public INode Highlighted => _uiNode;
    // public INode SelectedNode { get; private set; }

    public void InMenuOrInGame()
    {
        if (HasNotFinishedSetUp()) return;
        
        if(!ReferenceEquals(LastHighlighted, _uiNode)) return;
        
        if (!InMenu)
        {
            OnExitingNode();
            return;
        }
        
        if (ReferenceEquals(LastHighlighted, _uiNode))
            SetNodeAsActive();
    }
    
    private bool HasNotFinishedSetUp()
    {
        if (_hasFinishedSetUp) return false;
        _hasFinishedSetUp = true;
        return true;
    }

    private void DoPressOnNode() => _uiFunctionEvents.DoIsPressed();
    public virtual void ExitNodeByType() => OnExitingNode();

    
    //Main
    public virtual void OnAwake()
    {
        if (MyBranch.IsAPopUpBranch() && _uiNode.HasChildBranch.IsNotNull())
        {
            _popUpBranch = new HotKeys();
            _popUpBranch.SetBranchRunTime((Branch)_uiNode.HasChildBranch);
            _uiNode.HasChildBranch = null;
        }    
    }

    public virtual void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        LateStartSetUp();
    }

    public void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
        _historyTracker = EZService.Locator.Get<IHistoryTrack>(this);
    }

    private void LateStartSetUp()
    {
        if(_myDataHub.IsNull()) return;

        if (_myDataHub.SceneStarted)
        {
            PointerOverNode = false;
        }
    }

    public virtual void FetchEvents()
    {
        // DoHighlighted = HistoryEvents.Do.Fetch<IHighlightedNode>();
        // DoSelected = HistoryEvents.Do.Fetch<ISelectedNode>();
    }
    
    public virtual void OnDisable()
    {
        // DoHighlighted = null;
        // DoSelected = null;
        _myDataHub = null;
    }

    public void OnDestroy() => _myDataHub = null;
    

    public virtual void OnStart() { }

    public void EnableOrDisableNode(IDisableData isDisabled)
    {
        if(isDisabled.IsNodeDisabled())
            SetNodeAsNotSelected_NoEffects();
        _uiFunctionEvents.DoIsDisabled(isDisabled);
    }

    public virtual void SetNodeAsActive()
    {
        if(_uiNode.IsNodeDisabled() && _uiNode.PassOver()) return;
        
        if (AllowKeys && InMenu)
        {
            OnEnteringNode();
        }
        else
        {
            if(PointerOverNode) return;
            OnExitingNode();
        }
    }


    protected void SetNodeAsSelected_NoEffects()
    {
        _uiFunctionEvents.DoMuteAudio();
        // if (_fromHotKey)
        // {
        //     Activate();
        //     _fromHotKey = false;
        // }
        // else
        // {
            Activate();
       // }
        OnExitingNode();
    }

    public virtual void SetNodeAsNotSelected_NoEffects()
    {
        _uiFunctionEvents.DoMuteAudio();
        SetSelectedStatus(false, DoPressOnNode);
        OnExitingNode();
    }

   public virtual void OnEnteringNode() 
    {
        _myDataHub.SetAsHighLighted(_uiNode);
        PointerOverNode = true;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
        MyBranch.SetNewHighlighted(_uiNode);
        if (_myDataHub.AllowKeys)
            MyBranch.OnPointerEnter(null);
    }
   
    public virtual void OnExitingNode()
    {
        PointerOverNode = false;
        _uiFunctionEvents.DoWhenPointerOver(PointerOverNode);
        if (_myDataHub.AllowKeys)
            MyBranch.OnPointerExit(null);
    }

    public void DoMoveToNextNode(MoveDirection moveDirection) => Navigation.AxisMoveDirection(moveDirection);

    public void MenuNavigateToThisNode(MoveDirection moveDirection)
    {
        if(!MyBranch.CanvasIsEnabled)return;
        
        OnEnteringNode();
    }

    public virtual void NodeSelected()
    {
        if (IsSelected)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    protected void Activate()
    {
        if(ActivatePopUpChildNode()) return;
        SetSelectedStatus(true, DoPressOnNode);
        ThisNodeIsSelected();
        MyBranch.OnPointerExit(null);
    }

    private void Deactivate()
    {
        if (!_historyTracker.NodeNeededForMultiSelect(_uiNode))
            SetSelectedStatus(false, DoPressOnNode);
        ThisNodeIsSelected();
        MyBranch.OnPointerEnter(null);
    }
    
    private void ThisNodeIsSelected()
    {
        MyBranch.SetNewSelected(IsSelected ? _uiNode : null);
        if(_dontAddToHistoryTracking || !_myDataHub.SceneStarted) return;
        _myDataHub.SetSelected(_uiNode);
    }

    private void SetSelectedStatus(bool isSelected, Action endAction)
    {
        IsSelected = isSelected;
        _uiFunctionEvents.DoIsSelected(IsSelected);
        endAction?.Invoke();
    }

    private bool ActivatePopUpChildNode()
    {
        if (_popUpBranch.IsNull()) return false;
        
        Debug.Log("Hot Key activation as child???");
        _historyTracker.CancelHasBeenPressed(EscapeKey.BackOneLevel, MyBranch);
        _popUpBranch.HotKeyActivation();
        return true;
    }

    public void HotKeyPressed(bool setAsActive)
    {
       // if(setAsActive)
           _myDataHub.SetAsHighLighted(_uiNode); // Sets the switcher to the correct Branch
        MyBranch.SetNewHighlighted(_uiNode);

        if (setAsActive)
        {
            //SetNodeAsSelected_NoEffects();
            Activate();
            //OnExitingNode();
        }
        else
        {
            SetNodeAsSelected_NoEffects();
        }
    }
    
    public virtual void SetUpGOUIParent(IGOUIModule module) { }
}
