using EZ.Service;
using UIElements;
using UnityEngine;

public interface IStandardBranch : IBranchBase { }

public class StandardBranch : BranchBase, IStandardBranch
{
    public StandardBranch(IBranch branch) : base(branch) { }
    
    private ICanvasOrderData _canvasOrderData;
   // private bool _isOnRootTrunk;

    //private IBranch[] AllBranches => _myDataHub.AllBranches;
    private bool IsControlBar => ThisBranch.IsControlBar() && CanStart;
    
    // private bool OnlyTweenOnSceneStart => _myBranch.TweenOnSceneStart == DoTween.DoNothing 
    //                                   && _myDataHub.SceneStarted;


    public override void UseEZServiceLocator()
    {
        base.UseEZServiceLocator();
        _canvasOrderData = EZService.Locator.Get<ICanvasOrderData>(this);
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        HistoryEvents.Do.Subscribe<IOnStart>(SetUpOnStart);
    }

    public override void UnObserveEvents()
    {
        base.UnObserveEvents();
        HistoryEvents.Do.Unsubscribe<IOnStart>(SetUpOnStart);
    }

    public override void OnStart()
    {
        base.OnStart();
        SetCanvas(ActiveCanvas.No);
    }

    protected override void SetUpBranchesOnStart(ISetUpStartBranches args) => SetControlBarCanvasOrder();

    private void SetUpOnStart(IOnStart args)
    {
        if(_myCanvas.enabled)
            SetBlockRaycast(BlockRaycast.Yes);
    }

    private void SetControlBarCanvasOrder()
    {
        if(!IsControlBar) return;
        
        ThisBranch.MyCanvas.overrideSorting = true;
        ThisBranch.MyCanvas.sortingOrder = _canvasOrderData.ReturnControlBarCanvasOrder();
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        
        if(!IsControlBar)
            _canvasOrderCalculator.SetCanvasOrder();
        
        if(/*OnlyTweenOnSceneStart ||*/ IsControlBar || ThisBranch.IsAlreadyActive)
        {
            //Debug.Log($"Cant Tween : {ThisBranch} :  {_myCanvas.enabled} ");
            ThisBranch.DoNotTween();
        }        
        SetCanvas(ActiveCanvas.Yes);
        //CanGoToFullscreen();
        
       // Debug.Log($"New Parent : {newParentController} for {_myBranch}");
        //TODO Check I still need this
        // if(!_isOnRootTrunk)
        // {
            SetNewParentBranch(newParentController);
       // }
        // else
        // {
        //     // if(!OnHomeScreen && _isOnRootTrunk)
        //     //     InvokeOnHomeScreen();
        //     //_justReturnedHome = false;
        // }
        // if(_myBranch.ParentTrunk == ScreenType.FullScreen)
        //     _screenData.StoreClearScreenData(AllBranches, _myBranch, BlockRaycast.Yes);
    }

    // protected override void ClearBranchForFullscreen(IClearScreen args)
    // {
    //    // if(_isTabBranch) return;
    //     base.ClearBranchForFullscreen(args);
    // }

    private void SetNewParentBranch(IBranch newParentController) 
    {
        if(newParentController is null) return;
        ThisBranch.MyParentBranch = newParentController;
    }

    // public override void SetBlockRaycast(BlockRaycast active)
    // {
    //     if(!NoResolvePopUps) return;
    //     base.SetBlockRaycast(active);
    // }
    
    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(!GameIsPaused && NoResolvePopUps)
        {
            base.SetBlockRaycast(IsControlBar ? BlockRaycast.Yes: active);
        }
        else
        {
            base.SetBlockRaycast(active);
        }
    }
    
    public override void SetCanvas(ActiveCanvas active)
    {
        if(IsControlBar && CanStart) return;
        //TODO why is this check here
        if(!GameIsPaused && NoResolvePopUps)
        {
            base.SetCanvas(active);
        }
        else
        {
            base.SetCanvas(active);
        }
    }
}
