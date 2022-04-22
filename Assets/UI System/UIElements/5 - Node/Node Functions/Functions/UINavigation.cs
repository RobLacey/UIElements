using System;
using EZ.Service;
using UnityEngine.EventSystems;

public class UINavigation : NodeFunctionBase
{
    public UINavigation(INavigationSettings settings, IUiEvents uiEvents): base(uiEvents)
    {
        _up = settings.Up;
        _down = settings.Down;
        _left = settings.Left;
        _right = settings.Right;
    }

    //Variables
    private readonly NavigateKeyPress _up, _down, _left, _right;
    private IBranch _myBranch;
    private INode _myNode;
    private IHistoryTrack _historyTrack;

    //Properties
    private int Index => Array.IndexOf(_myBranch.ThisBranchesNodes, _myNode);
    private int NodeGroupSize => _myBranch.ThisBranchesNodes.Length;
    protected override bool CanBeHighlighted() => false;
    protected override bool CanBePressed() => false;
    protected override void SavePointerStatus(bool pointerOver) { }

    //Main
    public override void OnAwake()
    {
        base.OnAwake();
        _myNode = _uiEvents.ReturnMasterNode;
        _myBranch = _myNode.MyBranch;
    }

    public override void UseEZServiceLocator()
    {
        base.UseEZServiceLocator();
        _historyTrack = EZService.Locator.Get<IHistoryTrack>(this);
    }

    public override void AxisMoveDirection(MoveDirection moveDirection)
    {
        base.AxisMoveDirection(moveDirection);
        ProcessMoves(moveDirection);
    }

    private void ProcessMoves(MoveDirection moveDirection)
    {
        if (FunctionNotActive()) return;

        DoPresetMove(moveDirection);
    }

    private void DoPresetMove(MoveDirection moveDirection)
    {
        switch (moveDirection)
        {
            case MoveDirection.Down:
            {
                ProcessMove(_down, MoveDirection.Down);
                break;
            }
            case MoveDirection.Up:
            {
                ProcessMove(_up, MoveDirection.Up);
                break;
            }
            case MoveDirection.Left:
            {
                ProcessMove(_left, MoveDirection.Left);
                break;
            }
            case MoveDirection.Right:
            {
                ProcessMove(_right, MoveDirection.Right);
                break;
            }
        }
    }

    private void ProcessMove(NavigateKeyPress keyPressed, MoveDirection direction)
    {
        switch (keyPressed.MoveType)
        {
            case NavPressMoveType.Navigate:
                if(keyPressed.Navigate)
                    keyPressed.Navigate.MenuNavigateToThisNode(direction);
                break;
            case NavPressMoveType.ToChildBranch:
                if (_myNode.HasChildBranch.IsNull()) break;
                _myNode.NodeSelected();
                break;
            case NavPressMoveType.Back:
                _historyTrack.CancelHasBeenPressed(EscapeKey.BackOneLevel, _myBranch);
                break;
            case NavPressMoveType.AutoNavigate:
                DoAutoMove(direction);
                break;
        }
    }

    private void DoAutoMove(MoveDirection moveDirection)
    {
        if (NodeGroupSize <= 1) return;

        switch (moveDirection)
        {
            case MoveDirection.Down:
                CheckMoveDirection(PositiveInteract, moveDirection, Index);
                break;
            case MoveDirection.Up:
                CheckMoveDirection(NegativeIterate, moveDirection, Index);
                break;
            case MoveDirection.Left:
                CheckMoveDirection(NegativeIterate, moveDirection, Index);
                break;
            case MoveDirection.Right:
                CheckMoveDirection(PositiveInteract, moveDirection, Index);
                break;
        } 
    }

    private int PositiveInteract(int newIndex) => newIndex.PositiveIterate(NodeGroupSize);
    
    private int NegativeIterate(int newIndex) => newIndex.NegativeIterate(NodeGroupSize);

    private void CheckMoveDirection(Func<int,int> iterateMethod, MoveDirection moveDirection, int index)
    {
        _myBranch.ThisBranchesNodes[iterateMethod.Invoke(index)].MenuNavigateToThisNode(moveDirection);
    }

    private protected override void ProcessPress()
    { 
    }
}
