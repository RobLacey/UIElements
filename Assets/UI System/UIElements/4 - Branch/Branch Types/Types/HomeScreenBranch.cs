using System;
using EZ.Service;
using UIElements;
using UnityEngine;

public interface IHomeScreenBranch : IBranchBase { }

[Obsolete("Not Need under new system", true)]
public class HomeScreenBranch: BranchBase, IHomeScreenBranch
{
    //Constructor
    public HomeScreenBranch(IBranch branch) : base(branch) { }

    // private ICanvasOrderData _canvasOrderData;
    private bool _justReturnedHome = false;
    
    //Properties
    private bool CannotTweenOnHome => /*_myBranch.TweenOnSceneStart == DoTween.DoNothing 
                                      && (*/!_justReturnedHome && ThisBranch.StayVisibleMovingToChild() == IsActive.Yes;
     private bool IsControlBar => ThisBranch.IsControlBar() && CanStart;

    // protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    // {
    //     // base.SaveIfOnHomeScreen(args);
    //     // _justReturnedHome = true;
    // }

    //Main
    // public override void UseEZServiceLocator()
    // {
    //     base.UseEZServiceLocator();
    //     // _canvasOrderData = EZService.Locator.Get<ICanvasOrderData>(this);
    // }

    // public override void ObserveEvents()
    // {
    //     base.ObserveEvents();
    //     HistoryEvents.Do.Subscribe<IOnStart>(SetUpOnStart);
    // }
    //
    // public override void UnObserveEvents()
    // {
    //     base.UnObserveEvents();
    //     HistoryEvents.Do.Unsubscribe<IOnStart>(SetUpOnStart);
    // }

    // public override void OnStart()
    // {
    //     base.OnStart();
    //     SetCanvas(ActiveCanvas.No);
    // }

    // protected override void SaveInMenu(IInMenu args)
    // {
    //     base.SaveInMenu(args);
    //     SetBlockRaycast(BlockRaycast.Yes);
    // }


    //Main
    // protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    // {
    //     if(args.ThisTrunk != _myBranch.ParentTrunk) return;
    //     SetControlBarCanvasOrder();
    //     _myBranch.DontSetBranchAsActive();
    //     _myBranch.MoveToThisBranch();
    // }

    // private void SetUpOnStart(IOnStart args) => SetBlockRaycast(BlockRaycast.Yes);

    // private void SetControlBarCanvasOrder()
    // {
    //     if(!IsControlBar) return;
    //     
    //     _myBranch.MyCanvas.overrideSorting = true;
    //     _myBranch.MyCanvas.sortingOrder = _canvasOrderData.ReturnControlBarCanvasOrder();
    // }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        
        if(!InMenu) return;
        
        if(!IsControlBar)
            _canvasOrderCalculator.SetCanvasOrder();
        
        if (CannotTweenOnHome || IsControlBar || ThisBranch.CanvasIsEnabled)
        {
            ThisBranch.DoNotTween();
        }     
        
        SetCanvas(ActiveCanvas.Yes);
        
        // if(!OnHomeScreen)
        //     InvokeOnHomeScreen();
        _justReturnedHome = false;
    }

    // public override void EndOfBranchExit()
    // {
    //     base.EndOfBranchExit();
    //    // CheckChildOfControlBar();
    // }

    // private void CheckChildOfControlBar()
    // {
    //     // if (_myBranch.LastSelected.HasChildBranch.ParentTrunk == ScreenType.Overlay && !OnHomeScreen)
    //     // {
    //     //     _historyTrack.BackToHomeScreen();
    //     // }
    // }

    // public override void SetBlockRaycast(BlockRaycast active)
    // {
    //     if(!GameIsPaused && NoResolvePopUps)
    //     {
    //         base.SetBlockRaycast(IsControlBar ? BlockRaycast.Yes: active);
    //     }
    //     else
    //     {
    //         base.SetBlockRaycast(active);
    //     }
    // }
    
    // public override void SetCanvas(ActiveCanvas active)
    // {
    //     if(IsControlBar && CanStart) return;
    //     
    //     if(!GameIsPaused && NoResolvePopUps)
    //     {
    //         base.SetCanvas(/*IsControlBar ? ActiveCanvas.Yes : */active);
    //     }
    //     else
    //     {
    //         base.SetCanvas(active);
    //     }
    // }
}


