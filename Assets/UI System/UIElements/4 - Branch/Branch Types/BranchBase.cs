using System;
using EZ.Events;
using EZ.Service;
using UnityEngine;


public class BranchBase : IEZEventUser, IServiceUser, IBranchBase, IBranchParams,
                          IEZEventDispatcher, ICanvasCalcParms, ISetPositionParms, 
                          IRemoveActiveBranch, IAddActiveBranch

{
    protected BranchBase(IBranch branch)
    {
        ThisBranch = branch.ThisBranch;
        _myCanvas = ThisBranch.MyCanvas;
        _myCanvasGroup = ThisBranch.MyCanvasGroup;
    }
    
    //Variables
    //protected readonly IBranch MyBranch;
    protected IScreenData _screenData;
    protected IHistoryTrack _historyTrack;
   // protected bool _isTabBranch;
    protected ICanvasOrderCalculator _canvasOrderCalculator;
    protected readonly Canvas _myCanvas;
    protected readonly CanvasGroup _myCanvasGroup;
    protected IDataHub _myDataHub;

    //Events
    protected  Action<IAddActiveBranch> AddActiveBranch { get; set; }        
    protected  Action<IRemoveActiveBranch> RemoveActiveBranch { get; set; }        

    // private Action<IOnHomeScreen> SetIsOnHomeScreen { get; set; }
    // private Action<IClearScreen> DoClearScreen { get; set; }

    //Properties & Set/Getters
    protected bool InMenu => _myDataHub.InMenu;
    protected bool CanStart => _myDataHub.SceneStarted;
    protected bool GameIsPaused => _myDataHub.GamePaused;
    protected bool NoResolvePopUps => _myDataHub.NoResolvePopUp;

    protected bool OnHomeScreen => _myDataHub.OnHomeScreen;
   // public IBranch IgnoreThisBranch => _myBranch;
    public IBranch ThisBranch { get; }

   // private ScreenType MyScreenType => _myBranch.ParentTrunk.ScreenType;
    protected bool CanAllowKeys => _myDataHub.AllowKeys;


    // protected void InvokeOnHomeScreen()
    // {
    //     SetIsOnHomeScreen?.Invoke(this);
    // }
    
   // private void InvokeDoClearScreen() => DoClearScreen?.Invoke(this);
    protected virtual void SaveInMenu(IInMenu args) { }
    protected virtual void SaveIfOnHomeScreen(IOnHomeScreen args) { }

    private void AllowKeys(IAllowKeys args)
    {
        if (!CanStart && CanAllowKeys)
        {
            _myCanvasGroup.blocksRaycasts = false;
            return;
        }
        var blockRaycast = CanAllowKeys ? BlockRaycast.No : BlockRaycast.Yes;
        SetBlockRaycast(blockRaycast);
    }

    //Main
    public virtual void OnAwake()
    {
        _canvasOrderCalculator = EZInject.Class.WithParams<ICanvasOrderCalculator>(this);
        _screenData = EZInject.Class.WithParams<IScreenData>(this);
        _myCanvas.enabled = false;
        _myCanvasGroup.blocksRaycasts = false;
    }

    public virtual void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
        UseEZServiceLocator();
        _screenData.OnEnable();
        _canvasOrderCalculator.OnEnable();
        LateStartUp();
    }

    private void LateStartUp()
    {
        if (_myDataHub.IsNull()) return;

        if (_myDataHub.SceneStarted)
        {
            _canvasOrderCalculator.OnStart();
        }
    }

    public virtual void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
        _historyTrack = EZService.Locator.Get<IHistoryTrack>(this);
    }
    
    public virtual void OnStart()
    {
        if(_myDataHub.SceneStarted) return;
        _canvasOrderCalculator.OnStart();
    }

    public virtual void FetchEvents()
    {
        AddActiveBranch = HistoryEvents.Do.Fetch<IAddActiveBranch>();
        RemoveActiveBranch = HistoryEvents.Do.Fetch<IRemoveActiveBranch>();

        // SetIsOnHomeScreen = HistoryEvents.Do.Fetch<IOnHomeScreen>();
        // DoClearScreen = BranchEvent.Do.Fetch<IClearScreen>();
    }

    public virtual void ObserveEvents()
    {
        BranchEvent.Do.Subscribe<ISetUpStartBranches>(SetUpBranchesOnStart);
        HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
        InputEvents.Do.Subscribe<IAllowKeys>(AllowKeys);
    }

    public virtual void UnObserveEvents()
    {
        BranchEvent.Do.Unsubscribe<ISetUpStartBranches>(SetUpBranchesOnStart);
        HistoryEvents.Do.Unsubscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
        InputEvents.Do.Unsubscribe<IAllowKeys>(AllowKeys);
        _screenData.OnDisable();
    }

    public virtual void OnDisable()
    {
        UnObserveEvents();
        //SetIsOnHomeScreen = null;
      //  DoClearScreen = null;
    }
    
    public virtual void OnDestroy()
    {
        UnObserveEvents();
        _myDataHub = null;
        _historyTrack = null;
    }

    public virtual void SetUpGOUIBranch(IGOUIModule module) { }
    public virtual IGOUIModule ReturnGOUIModule() => null;

    //public void SetUpAsTabBranch() => _isTabBranch = true;

    protected virtual void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetBlockRaycast(BlockRaycast.No);
        SetCanvas(ActiveCanvas.No);
    }

    public virtual bool CanStartBranch()
    {
        AddActiveBranch?.Invoke(this);
         return true;
    }
    
    public virtual bool CanExitBranch(OutTweenType outTweenType) 
        => ThisBranch.StayVisibleMovingToChild() == IsActive.No || outTweenType != OutTweenType.MoveToChild;

    public virtual void SetUpBranch(IBranch newParentController = null)
    {
        //ActivateChildTabBranches(ActiveCanvas.Yes);
    }

    // private void ActivateChildTabBranches(ActiveCanvas activeCanvas)
    // {
    //     if (HasChildTabBranches())
    //     {
    //         _myBranch.LastSelected.ToggleData.ReturnTabBranch.SetCanvas(activeCanvas);
    //     }
    //
    //     bool HasChildTabBranches()
    //     {
    //         return _myBranch.LastSelected.IsToggleGroup && _myBranch.LastSelected.ToggleData.ReturnTabBranch;
    //     }
    // }
    
    public virtual void EndOfBranchStart()
    {
        if(!CanStart) return;
        SetBlockRaycast(BlockRaycast.Yes);
    }

    public virtual void StartBranchExit()
    {
        SetBlockRaycast(BlockRaycast.No);
    }

    public virtual void EndOfBranchExit()
    {
        RemoveActiveBranch?.Invoke(this);

        //InvokeOnHomeScreen();
        SetCanvas(ActiveCanvas.No);
        _canvasOrderCalculator.ResetCanvasOrder();
        //ActivateChildTabBranches(ActiveCanvas.No);
    }
    
    public virtual void SetCanvas(ActiveCanvas active)
    {
        _myCanvas.enabled = active == ActiveCanvas.Yes;
    }

    public virtual void SetBlockRaycast(BlockRaycast active)
    {
        if (CanAllowKeys)
        {
            _myCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes;
        }
    }

    // protected virtual void ClearBranchForFullscreen(IClearScreen args)
    // {
    //     //if (args.IgnoreThisBranch == _myBranch || !_myBranch.CanvasIsEnabled) return;
    //     
    //    // SetCanvas(ActiveCanvas.No);
    //    // SetBlockRaycast(BlockRaycast.No);
    // }
    
    // protected void CanGoToFullscreen()
    // {
    //     //**** Put a check in that the Trunk isn't changing or remove this to trunk????****
    //     Debug.Log($"{_myBranch} : {MyScreenType}");
    //      if (MyScreenType != ScreenType.FullScreen/* || !OnHomeScreen*/) return;
    //      InvokeDoClearScreen();
    // }
    
    // protected void CanGoToFullscreen_Paused()
    // {
    //     // if (MyScreenType != ScreenType.FullScreen) return;
    //     // InvokeDoClearScreen();
    // }

    protected void ActivateStoredPosition()
    {
        _screenData.RestoreScreen();
        // if (_screenData.WasOnHomeScreen)
        //     InvokeOnHomeScreen();
    }

}