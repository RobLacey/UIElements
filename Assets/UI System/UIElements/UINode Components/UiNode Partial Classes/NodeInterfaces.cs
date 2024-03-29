﻿using EZ.Inject;
using UIElements.Input_System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface INode : IToggles, IParameters, IDisableData
{
    EscapeKey EscapeKeyType { get; }
    void SetAsHotKeyParent(bool setAsActive);
    IBranch MyBranch { get; }
    IBranch HasChildBranch { get; set; }
    bool CanNotStoreNodeInHistory { get; }
    GameObject ReturnGameObject { get; }
    GameObject InGameObject { get; set; }
    void SetNodeAsActive();
    void ExitNodeByType();
    void ThisNodeIsHighLighted();
    void ThisNodeNotHighLighted();
    IUiEvents UINodeEvents { get; }
    MultiSelectSettings MultiSelectSettings { get; }
    void ClearNode();
    float AutoOpenDelay { get; }
    bool CanAutoOpen { get; }
    IRunTimeSetter MyRunTimeSetter { get; }
    void NavigateToNextMenuItem(AxisEventData eventData);
    void MenuNavigateToThisNode(MoveDirection moveDirection);
    void SetGOUIModule(IGOUIModule module);
}

public interface IToggles
{
    ToggleData ToggleData { get; }
    bool IsToggleGroup { get; }
}

public interface IDisableData
{
    bool IsNodeDisabled();
    bool PassOver();
}
        
