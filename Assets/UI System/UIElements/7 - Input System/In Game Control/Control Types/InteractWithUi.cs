using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;


[Obsolete("IInteractWithUi has been replaced and this is for the old one that compares Rect's", true)]

public class InteractWithUi : /*IInteractWithUi,*/ IEZEventUser, IServiceUser
{
    private readonly Dictionary<Node, RectTransform> _activeNodes = new Dictionary<Node, RectTransform>();
    private readonly Dictionary<Node, RectTransform> _sortedNodesDict = new Dictionary<Node, RectTransform>();
    private readonly Dictionary<IBranch, RectTransform> _activeBranches = new Dictionary<IBranch, RectTransform>();
    private readonly Dictionary<IBranch, RectTransform> _sortedBranches = new Dictionary<IBranch, RectTransform>();
    private (Node node, RectTransform rect) _lastHit;
    private bool _canStart = false;
    private bool _onlyHitInGameObjects;
    private IDataHub _myDataHub;

    private void OnStart(IOnStart args)
    {
        _canStart = true;
        ProcessBranchAndNodeLists.SortNodeList(_sortedNodesDict, _activeNodes);
        ProcessBranchAndNodeLists.SortBranchList(_sortedBranches, _activeBranches);
    }
    public void ClearLastHit() => _lastHit = (null, null);

    public void SetCanOnlyHitInGameObjects() => _onlyHitInGameObjects = true;

    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        ObserveEvents();
    }
    
    public void ObserveEvents()
    {
        Debug.Log("UpTo Here : Make VC GOUI work then remove reduntenat code");
        HistoryEvents.Do.Subscribe<IOnStart>(OnStart);
        // **Removed Interfaces as not used anymore**
        // BranchEvent.Do.Subscribe<ICanInteractWithBranch>(AddNodes);
        // BranchEvent.Do.Subscribe<ICannotInteractWithBranch>(RemoveNodes);
        
       // BranchEvent.Do.Subscribe<ICloseBranch>(RemoveNodeAsGouiClosed);
    }

    public void UnObserveEvents() { }

    public void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }
    
    public bool CheckIfCursorOverUI(IVirtualCursor virtualCursor)
    {
        // var pointerOverNode = _sortedNodesDict.FirstOrDefault(node => PointerInsideUIObject(node.Value, virtualCursor));
        //
        // if (pointerOverNode.Key)
        // {
        //     if (UnderAnotherUIObject(pointerOverNode, virtualCursor)) return;
        //     if(pointerOverNode.Key == _lastHit.node) return;
        //     StartNewNode(virtualCursor, pointerOverNode);
        //     return true;
        // }
        // CheckIfNotOverLastHitNode(virtualCursor);
        return false;
    }

    private bool UnderAnotherUIObject(KeyValuePair<Node, RectTransform> node, IVirtualCursor virtualCursor)
    {
        foreach (var activeBranch in _sortedBranches)
        {
            if (PointerInsideUIObject(activeBranch.Value, virtualCursor))
            {
                return OverBranchButActiveNodeBelow(node, activeBranch.Key);
            }
        }
        return false;
    }
    
    private bool OverBranchButActiveNodeBelow(KeyValuePair<Node, RectTransform> nodeCursorIsOver, 
                                              IBranch branchCursorIsOver)
    {
         if (nodeCursorIsOver.Key.MyBranch.MyCanvas.sortingOrder < branchCursorIsOver.MyCanvas.sortingOrder)
         {
             nodeCursorIsOver.Key.OnPointerExit(null);
             CloseLastHitAsNotOver(); 
             return true;
         }
         return false;
    }
    
    private void StartNewNode(IVirtualCursor virtualCursor, KeyValuePair<Node, RectTransform> node)
    {
        CloseLastHitNodeAsDifferent();
        node.Key.OnPointerEnter(null);
        CloseLastBranchIfNotRelated(virtualCursor, node.Key.MyBranch);
        _lastHit = (node.Key, node.Value);
        // virtualCursor.OverAnyObject = _lastHit.node.MyBranch;
        // virtualCursor.OverAnyObject.AutoOpenCloseClass.OnPointerEnter();
    }
    
    private void CloseLastHitNodeAsDifferent()
    {
        if (!_lastHit.node) return;
        _lastHit.node.OnPointerExit(null);
    }
    
    private static void CloseLastBranchIfDifferent(IVirtualCursor virtualCursor, IBranch currentBranch)
    {
        // if (virtualCursor.OverAnyObject.IsNotNull() && virtualCursor.OverAnyObject != currentBranch)
        // {
        //     virtualCursor.OverAnyObject.AutoOpenCloseClass.OnPointerExit();
        // }
    }
    
    private static void CloseLastBranchIfNotRelated(IVirtualCursor virtualCursor, IBranch currentBranch)
    {
        // if(virtualCursor.OverAnyObject.IsNull() || currentBranch.IsNull()) return;
        //
        // if (virtualCursor.OverAnyObject.MyCanvas.sortingOrder > currentBranch.MyCanvas.sortingOrder)
        // {
        //     virtualCursor.OverAnyObject.AutoOpenCloseClass.OnPointerExit();
        // }
    }
    
    private void CheckIfNotOverLastHitNode(IVirtualCursor virtualCursor)
    {
        CloseLastHitAsNotOver();  
        
        if (SetOverAnActiveBranch(virtualCursor)) return;
        
        CloseLastBranchIfDifferent(virtualCursor, null);
       // virtualCursor.OverAnyObject = null;
    }
    
    private static bool PointerInsideUIObject(RectTransform nodeRect, IVirtualCursor virtualCursor)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(nodeRect,
                                                                 virtualCursor.CursorRect.position,
                                                                 null);
    }
    
    private void CloseLastHitAsNotOver()
    {
        if (!_lastHit.node) return;
        _lastHit.node.OnPointerExit(null);
        _lastHit = (null, null);
    }
    
    private bool SetOverAnActiveBranch(IVirtualCursor virtualCursor)
    {
        foreach (var branch in _sortedBranches)
        {
            if (PointerInsideUIObject(branch.Value, virtualCursor))
            {
                CloseLastBranchIfDifferent(virtualCursor, branch.Key);
                // if(virtualCursor.OverAnyObject != branch.Key)
                // {
                //     branch.Key.AutoOpenCloseClass.OnPointerEnter();
                //     virtualCursor.OverAnyObject = branch.Key;
                // }                
                return true;
            }
        }
        return false;
    }
    
    
    //**Interface doesn't exist anymore**
    // private void AddNodes(ICanInteractWithBranch args)
    // {
    //     if(_onlyHitInGameObjects && !args.MyBranch.IsInGameBranch()) return;
    //     
    //     var nodes = args.MyBranch.ThisBranchesNodes.Cast<UINode>().ToArray();
    //     
    //     var branchesNeedSort = ProcessBranchAndNodeLists.CheckAndAddNewBranch(args.MyBranch, _activeBranches);
    //     var needToSort = ProcessBranchAndNodeLists.AddNewNodesToList(nodes, _activeNodes);
    //     
    //     if(needToSort && _canStart) 
    //         ProcessBranchAndNodeLists.SortNodeList(_sortedNodesDict, _activeNodes);
    //     
    //     if(branchesNeedSort && _canStart)
    //     {
    //         ProcessBranchAndNodeLists.SortBranchList(_sortedBranches, _activeBranches);
    //     }    
    // }
    
    
    //**Interface doesn't exist anymore**

    // private void RemoveNodeAsGouiClosed(ICloseBranch args) => RemoveProcess(args.TargetBranch);
    //
    // private void RemoveNodes(ICannotInteractWithBranch args) => RemoveProcess(args.MyBranch);
    //
    // private void RemoveProcess(IBranch branch)
    // {
    //     if (_onlyHitInGameObjects && !branch.IsInGameBranch()) return;
    //
    //     var list = branch.ThisBranchesNodes.Cast<UINode>().ToArray();
    //
    //     ProcessBranchAndNodeLists.CheckAndRemoveBranch(branch, _activeBranches);
    //     var needSort = ProcessBranchAndNodeLists.RemoveNodeFromList(list, _activeNodes);
    //
    //     if (needSort & _canStart)
    //         ProcessBranchAndNodeLists.SortNodeList(_sortedNodesDict, _activeNodes);
    // }
}