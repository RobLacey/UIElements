﻿using System;
using System.Collections.Generic;
using UIElements;
using UnityEngine;

/// <summary>
/// This partial class holds all the classes properties as UIBranch is an important link between higher and lower parts of the system
/// </summary>
public partial class Branch 
{
    //Set / Getters
    public bool IsAPopUpBranch()
    {
        return _branchType == BranchType.OptionalPopUp
               || _branchType == BranchType.ResolvePopUp
               || _branchType == BranchType.TimedPopUp;
    }

    public bool IsControlBar() => _controlBar == IsActive.Yes;
    public bool IsPauseMenuBranch() => _branchType == BranchType.PauseMenu;
    public bool IsInternalBranch() => _branchType == BranchType.InternalObsolete;
    public bool IsHomeScreenBranch() => _branchType == BranchType.ControlBar;
    public bool IsStandardBranch() => _branchType == BranchType.Standard;
    public bool IsInGameBranch() => _branchType == BranchType.InGameObject;
    public void DoNotTween() => _tweenOnChange = false;
    public void DontSetAsActiveBranch() => _canActivateBranch = false;
    public bool IsTimedPopUp() => _branchType == BranchType.TimedPopUp;
    public bool StayVisibleMovingToChild() => _stayVisible == IsActive.Yes;
    public void SetNotAControlBar() => _controlBar = IsActive.No;
    public IsActive SetStayOn { set => _stayVisible = value; }
    
   //Properties
    public BranchType ReturnBranchType => _branchType;
    public bool CanvasIsEnabled => MyCanvas.enabled;
    public WhenAllowed WhenAllowed => _whenAllowed;

    //public StoreAndRestorePopUps StoreCloseOrDoNothing => _storeOrResetOptional;
    //public bool AllowWithActiveResolvePopUp => _allowWithActiveResolve   == IsActive.Yes;
   // public bool CanBufferPopUp => _canAddToHomeScreenBuffer              == IsActive.Yes;
    public INode DefaultStartOnThisNode { get; set; }
    public INode LastSelected { get; set; }
    public INode LastHighlighted { get; set; }
    public INode[] ThisBranchesNodes { get; private set; } = Array.Empty<INode>();
    public void SetThisGroupsNode(INode[] groupsNodes) => ThisBranchesNodes = groupsNodes;
    public IsActive GetSaveOnExit => _saveExitSelection;
    public Canvas MyCanvas { get; private set; } 
    public CanvasGroup MyCanvasGroup { get; private set; }
    public IBranch MyParentBranch { get; private set; }
    public IBranch ThisBranch => this;
    public GameObject ThisBranchesGameObject => gameObject;
    //public IBranch ActiveBranch => this;
    public IAutoOpenClose AutoOpenCloseClass { get; private set; }
    public bool PointerOverBranch => AutoOpenCloseClass.PointerOverBranch;
    public float Timer => _timer;
    public Trunk ParentTrunk { get; set; }
    public bool IsAlreadyActive => !_tweening && CanvasIsEnabled;
    public WhenActiveDo WhenActiveDoThis => _whenActiveDo;


    public IsActive SetSaveLastSelectionOnExit
    {
        set => _saveExitSelection = value;
    }

    public EscapeKey EscapeKeyType
    {
        get => _escapeKeyFunction;
        set => _escapeKeyFunction = value;
    }

    // public WhenToMove WhenToMoveToChild
    // {
    //     get => _moveToChild;
    //     set => _moveToChild = value;
    // }

    // public DoTween TweenOnSceneStart
    // {
    //     get => _tweenOnReturn;
    //     set => _tweenOnReturn = value;
    // }
    
    public IsActive AutoClose
    {
        get =>_autoClose; 
        set => _autoClose = value;
    }

    public float AutoCloseDelay => _autoCloseDelay;

    public OrderInCanvas CanvasOrder
    {
        get => _canvasOrderSetting;
        set => _canvasOrderSetting = value;
    }
    
    public int ReturnManualCanvasOrder => _orderInCanvas;
    public IBranch TargetBranch => this;
    public GOUIModule ReturnGOUIModule => _branchTypeBaseClass.ReturnGOUIModule() as GOUIModule;

  //  public bool DontApplyFocus => _applyFocus == ApplyFocus.No;
    public int FocusSortingOrder => _whenFocusedSortingOrder;


}