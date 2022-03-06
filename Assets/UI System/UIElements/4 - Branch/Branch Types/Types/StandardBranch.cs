using EZ.Service;
using UIElements;
using UnityEngine;

public interface IStandardBranch : IBranchBase { }

public class StandardBranch : BranchBase, IStandardBranch
{
    public StandardBranch(IBranch branch) : base(branch) { }
    
    private ICanvasOrderData _canvasOrderData;
    private bool _isOnRootTrunk;

    private IBranch[] AllBranches => _myDataHub.AllBranches;
    private bool IsControlBar => _myBranch.IsControlBar() && CanStart;
    
    private bool OnlyTweenOnSceneStart => _myBranch.TweenOnSceneStart == DoTween.DoNothing 
                                      && _myDataHub.SceneStarted;


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

    private void SetUpOnStart(IOnStart args)
    {
        if(_isOnRootTrunk)
            SetBlockRaycast(BlockRaycast.Yes);
    }

    protected override void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        if(!args.GroupsBranches.Contains(_myBranch))
        {
            base.SetUpBranchesOnStart(args);
            return;
        }

        _isOnRootTrunk = true;
        SetControlBarCanvasOrder();
        _myBranch.DontSetBranchAsActive();
        _myBranch.MoveToThisBranch();
    }

    private void SetControlBarCanvasOrder()
    {
        if(!IsControlBar) return;
        
        _myBranch.MyCanvas.overrideSorting = true;
        _myBranch.MyCanvas.sortingOrder = _canvasOrderData.ReturnControlBarCanvasOrder();
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        
        if(!IsControlBar)
            _canvasOrderCalculator.SetCanvasOrder();
        
        if(OnlyTweenOnSceneStart || IsControlBar || _myCanvas.enabled)
            _myBranch.DoNotTween();
        
        SetCanvas(ActiveCanvas.Yes);
        //CanGoToFullscreen();
        
        if(!_isOnRootTrunk)
        {
            SetNewParentBranch(newParentController);
        }
        // else
        // {
        //     // if(!OnHomeScreen && _isOnRootTrunk)
        //     //     InvokeOnHomeScreen();
        //     //_justReturnedHome = false;
        // }
        // if(_myBranch.ParentTrunk == ScreenType.FullScreen)
        //     _screenData.StoreClearScreenData(AllBranches, _myBranch, BlockRaycast.Yes);
    }

    protected override void ClearBranchForFullscreen(IClearScreen args)
    {
       // if(_isTabBranch) return;
        base.ClearBranchForFullscreen(args);
        _canvasOrderCalculator.ResetCanvasOrder();
    }

    private void SetNewParentBranch(IBranch newParentController) 
    {
        if(newParentController is null) return;
        _myBranch.MyParentBranch = newParentController;
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
