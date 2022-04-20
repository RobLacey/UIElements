using System;
using UnityEngine;

public partial class Branch
{
    private const string CanAutoOpenClose = nameof(CheckAutoOpenCloseStatus);
    //private const string HomeScreenBranch = nameof(IsHomeScreenBranch);
    private const string StandardBranch = nameof(IsStandardBranch);
    private const string ControlBarBranch = nameof(IsControlBar);
    private const string OptionalBranch = nameof(IsOptional);
   // private const string OnlyAllowOnHomeScreen = nameof(OnlyOnHomeScreen);
    private const string InGamUIBranch = nameof(InGameUI);
    private const string TimedBranch = nameof(IsTimedPopUp);
    private const string ResolveBranch = nameof(IsResolve);
    private const string AnyPopUpBranch = nameof(IsAPopUpBranch);
    private const string SetForResolve = nameof(DoSetForResolve);
    //private const string NotControlBar = nameof(IsNotControlBar);
    //private const string Stored = nameof(IsStored);
    //private const string Fullscreen = nameof(IsFullScreen);
    private const string ShowManualOrder = nameof(IsManualOrder);
   // private const string ValidInAndOutTweens = nameof(AllowableInAndOutTweens);
    private const string SetUpCanvasOrder = nameof(ChangeCanvasOrder);
    private bool CheckAutoOpenCloseStatus()
    {
        if (!IsStandardBranch() && !IsInGameBranch() && !IsInternalBranch()/* || IsFullScreen()*/)
        {
            _autoClose = IsActive.No;
            return false;
        }
        
        return true;
    }

    private void ChangeCanvasOrder()
    {
        var temp = GetComponent<Canvas>();
        temp.overrideSorting = true;
        temp.sortingOrder = _orderInCanvas;
    }

    private bool InGameUI => _branchType == BranchType.InGameObject;
   // private bool PauseMenu => _branchType == BranchType.PauseMenu;
    private bool IsManualOrder => _canvasOrderSetting == OrderInCanvas.Manual && IsStandardBranch();
    private bool IsResolve => _branchType == BranchType.ResolvePopUp;
    private bool IsOptional() => _branchType == BranchType.OptionalPopUp;

    private bool DoSetForResolve(WhenActiveDo whenActiveDo)
    {
        if (IsResolve && whenActiveDo == WhenActiveDo.Nothing)
        {
            _whenActiveDo = WhenActiveDo.BlockAllOtherRaycasts;
        }

        return true;
    }

}

