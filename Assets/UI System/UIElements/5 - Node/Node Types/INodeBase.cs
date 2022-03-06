using System;
using UIElements;
using UnityEngine.EventSystems;

public interface INodeBase : IMono
{
    void ExitNodeByType();
    UINavigation Navigation { set; }
    void InMenuOrInGame();
    void SetNodeAsActive();
    void OnEnteringNode();
    void OnExitingNode();
    void NodeSelected();
    void SetNodeAsNotSelected_NoEffects();
    void DoMoveToNextNode(MoveDirection moveDirection);
    void MenuNavigateToThisNode(MoveDirection moveDirection);
    void EnableOrDisableNode(IDisableData isDisabled);
    void HotKeyPressed(bool setAsActive);
    void SetUpGOUIParent(IGOUIModule module);
}