using System;
using System.Collections.Generic;
using EZ.Inject;
using UIElements;
using UnityEngine;
using UnityEngine.EventSystems;


public interface IBranch : IParameters, IAutoOpenCloseData, ICanvasOrder, IMonoDisable, IMonoEnable, IMonoOnDestroy,
                           IDynamicBranch, IToggleEvents, IPointerEnterHandler, IPointerExitHandler
{
    bool IsControlBar();
    bool IsPauseMenuBranch();
    bool IsAPopUpBranch();
    bool IsInternalBranch();
    bool IsHomeScreenBranch();
    bool IsTimedPopUp();
    bool IsInGameBranch();
    void SetNewSelected(INode newNode);
    void SetNewHighlighted(INode newNode);

    INode DefaultStartOnThisNode { get; }
    CanvasGroup MyCanvasGroup { get; }
    Trunk ParentTrunk { get; set; }
    EscapeKey EscapeKeyType { get; set; }
    WhenToMove WhenToMove { set; }
    bool CanvasIsEnabled { get; }
    WhenAllowed WhenAllowed { get; }
    //StoreAndRestorePopUps StoreCloseOrDoNothing { get; }
   // bool AllowWithActiveResolvePopUp { get; }
    //bool CanBufferPopUp { get; }
   // DoTween TweenOnSceneStart { get; set; }
    IBranch MyParentBranch { get; set; }
    float Timer { get; }
    INode[] ThisBranchesNodes { get; }
    bool PointerOverBranch { get;}
    IAutoOpenClose AutoOpenCloseClass { get; }
    IsActive SetSaveLastSelectionOnExit { set; }
    BranchType ReturnBranchType { get; }
    IsActive SetStayOn { set; }
    INode LastSelected { get; }
    INode LastHighlighted { get; }
    GameObject ThisBranchesGameObject { get; }
    //bool RestoreBranch { get; set; }
    bool IsAlreadyActive { get; }
    bool BlockRaycastToOpenBranches { get; }


    bool StayVisibleMovingToChild();
    void SetNotAControlBar();
    void StartPopUp_RunTimeCall(bool fromPool);
    void OpenThisBranch(IBranch newParentBranch = null);
    void SetBranchAsActive();
    void DontSetAsActiveBranch();
    void DoNotTween();
    void ExitThisBranch(OutTweenType outTweenType, Action endOfTweenCallback = null);
    void SetCanvas(ActiveCanvas activeCanvas);
    void SetBlockRaycast(BlockRaycast blockRaycast);
   // void SetUpAsTabBranch();
    void SetUpGOUIBranch(IGOUIModule module);
}

public interface IAutoOpenClose : IMonoEnable, IMonoDisable
{
    void OnPointerEnter();
    void OnPointerExit();
    bool PointerOverBranch { get;}
    IBranch ChildNodeHasOpenChild { set; }
}

public interface IAutoOpenCloseData : IThisBranch
{
    EscapeKey EscapeKeyType { get; }
    IsActive AutoClose { get; }
    float AutoCloseDelay { get; }
}

public interface IThisBranch
{
    IBranch ThisBranch { get; }
}

public interface ICanvasOrder
{
    OrderInCanvas CanvasOrder { get; set; }
    int ReturnManualCanvasOrder { get; }
    Canvas MyCanvas { get; }
}

public interface IDynamicBranch : IThisBranch
{
    IsActive GetSaveOnExit { get; }
    INode DefaultStartOnThisNode { get; set; }
    INode LastSelected { get; set; }
    INode LastHighlighted { get; set; }
    INode[] ThisBranchesNodes { get;}
    void SetThisGroupsNode(INode[] groupsNodes);
    bool IsInGameBranch();
}

public interface IToggleEvents
{
    event Action EnterBranchEvent;
    event Action ExitBranchEvent;
}




