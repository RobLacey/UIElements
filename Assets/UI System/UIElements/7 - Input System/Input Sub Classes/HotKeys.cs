﻿using System;
using System.Linq;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

[Serializable]
public partial class HotKeys : IHotKeyPressed, IEZEventDispatcher, IServiceUser,
                               IMonoEnable
{
    [SerializeField] 
    private string _name = SetName;
    [SerializeField] 
    private HotKey _hotKeyInput  = default;
    [SerializeField] 
    [AllowNesting] [OnValueChanged(IsAllowed)]
    private UIBranch _myBranch  = default;
    
    //Variables
    private bool _hasParentNode;
    private INode _parentNode;
    private InputScheme _inputScheme;
    private bool _makeParentActive;
    private IDataHub _myDatHub;
    private const string SetName = "Set My Name";

    //Events
    private Action<IHotKeyPressed> HotKeyPressed { get; set; }

    //Properties
   // private IBranch ActiveBranch => _myDatHub.ActiveBranch;
    public INode ParentNode => _parentNode;
    public IBranch MyBranch => _myBranch;
   // private INode[] ThisGroupsUiNodes => _myBranch.MyParentBranch.ThisBranchesNodes;

    //Main
    public void OnEnable()
    {
        IsAllowedType();
        FetchEvents();
        UseEZServiceLocator();
    }
    
    public void UseEZServiceLocator()  
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _myDatHub = EZService.Locator.Get<IDataHub>(this);
    }

    public void FetchEvents() => HotKeyPressed = InputEvents.Do.Fetch<IHotKeyPressed>();

    public bool CheckHotKeys()
    {
        if (!_inputScheme.HotKeyChecker(_hotKeyInput)) return false;
        if (_myBranch.CanvasIsEnabled) return false;
        HotKeyActivation();
        return true;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void HotKeyActivation()
    {    
        if(ReferenceEquals(_myDatHub.ActiveBranch, _myBranch)) return;
        if(!_hasParentNode)
        {
            GetParentNode();
        }
        
        // if (ActiveBranch.IsHomeScreenBranch())
        // {
            StartThisHotKeyBranch();
        // }
        // else
        // {
        //     ActiveBranch.StartBranchExitProcess(OutTweenType.Cancel, StartThisHotKeyBranch);
        // }
    }

    private void GetParentNode()
    {
        // if (!_myBranch.IsHomeScreenBranch())
        // {
        //     GetImmediateParentNode();
        // }
        // else
        // {
            FindHomeScreenParentNode();
       // }
        _hasParentNode = true;
    }

    // private void GetImmediateParentNode()
    // {
    //     var branchesNodes = ThisGroupsUiNodes;
    //     _parentNode = branchesNodes.First(node => ReferenceEquals(_myBranch, node.HasChildBranch));
    //     _makeParentActive = true;
    // }

    private void FindHomeScreenParentNode()
    {
        //Todo Need to review all of this. Need to work out what to select and what to add to history
        var myTrunk = _myBranch.ParentTrunk;
        var toTest = _myBranch.MyParentBranch;

        while (!toTest.ParentTrunk == myTrunk)
        {
            toTest = toTest.MyParentBranch;
        }
        
        _makeParentActive = true;
        _parentNode = toTest.LastHighlighted;
    }
    
    private void StartThisHotKeyBranch()
    {
        HotKeyPressed?.Invoke(this);
        _parentNode.SetAsHotKeyParent(_makeParentActive);
        _myBranch.MoveToThisBranch(_parentNode.MyBranch);
    }
}