﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UINavigation : NodeFunctionBase
{
    public UINavigation(INavigationSettings settings, IUiEvents uiEvents): base(uiEvents)
    {
        _mySettings = settings;
        _up = settings.Up;
        _down = settings.Down;
        _left = settings.Left;
        _right = settings.Right;
    }

    //Variables
    private readonly Node _up, _down, _left, _right;
    private IBranch _myBranch;
    private INode _myNode;
    private readonly INavigationSettings _mySettings;

    //Properties
    private int Index => Array.IndexOf(_myBranch.ThisBranchesNodes, _myBranch.LastHighlighted);
    private NavigationType SetNavigation => _mySettings.NavType;
    private int NodeGroupSize => _myBranch.ThisBranchesNodes.Length;
    protected override bool CanBeHighlighted() => false;

    protected override bool CanBePressed() => false;
    protected override void SavePointerStatus(bool pointerOver) { }
    protected override bool FunctionNotActive() => SetNavigation == NavigationType.None;

    //Main
    public override void OnAwake()
    {
        base.OnAwake();
        _myNode = _uiEvents.ReturnMasterNode;
        _myBranch = _myNode.MyBranch;
    }
    
    public override void AxisMoveDirection(MoveDirection moveDirection)
    {
        base.AxisMoveDirection(moveDirection);
        ProcessMoves(moveDirection);
    }

    private void ProcessMoves(MoveDirection moveDirection)
    {
        if (FunctionNotActive()) return;

        if (DoAutoMove(moveDirection)) return;

        DoPresetMove(moveDirection);
    }

    private void DoPresetMove(MoveDirection moveDirection)
    {
        switch (moveDirection)
        {
            case MoveDirection.Down when _down:
            {
                _down.MenuNavigateToThisNode(moveDirection);
                break;
            }
            case MoveDirection.Up when _up:
            {
                _up.MenuNavigateToThisNode(moveDirection);
                break;
            }
            case MoveDirection.Left when _left:
            {
                _left.MenuNavigateToThisNode(moveDirection);
                break;
            }
            case MoveDirection.Right when _right:
            {
                _right.MenuNavigateToThisNode(moveDirection);
                break;
            }
        }
    }

    private bool DoAutoMove(MoveDirection moveDirection)
    {
        if (NodeGroupSize <= 1) return false;

        switch (moveDirection)
        {
            case MoveDirection.Down when SetNavigation == NavigationType.AutoUpDown:
                CheckMoveDirection(PositiveInteract, moveDirection, Index);
                return true;
            case MoveDirection.Up when SetNavigation == NavigationType.AutoUpDown:
                CheckMoveDirection(NegativeIterate, moveDirection, Index);
                return true;
            case MoveDirection.Left when SetNavigation == NavigationType.AutoRightLeft:
                CheckMoveDirection(NegativeIterate, moveDirection, Index);
                return true;
            case MoveDirection.Right when SetNavigation == NavigationType.AutoRightLeft:
                CheckMoveDirection(PositiveInteract, moveDirection, Index);
                return true;
            case MoveDirection.None:
                return false;
            default:
                return false;
        } 
    }

    private int PositiveInteract(int newIndex) => newIndex.PositiveIterate(NodeGroupSize);
    
    private int NegativeIterate(int newIndex) => newIndex.NegativeIterate(NodeGroupSize);

    private void CheckMoveDirection(Func<int,int> iterateMethod, MoveDirection moveDirection, int index)
    {
        _myBranch.ThisBranchesNodes[iterateMethod.Invoke(index)].MenuNavigateToThisNode(moveDirection);
    }

    private protected override void ProcessPress() { }
}
