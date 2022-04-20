using System;
using EZ.Events;
using EZ.Service;
using UnityEngine;


public class BranchBase : IEZEventUser, IServiceUser, IBranchBase, IBranchParams,
                          IEZEventDispatcher, ICanvasCalcParms, ISetPositionParms
{
    protected BranchBase(IBranch branch)
    {
        ThisBranch = branch.ThisBranch;
        _myCanvas = ThisBranch.MyCanvas;
        _myCanvasGroup = ThisBranch.MyCanvasGroup;
    }
    
    //Variables
    protected IHistoryTrack _historyTrack;
    protected ICanvasOrderCalculator _canvasOrderCalculator;
    protected readonly Canvas _myCanvas;
    protected readonly CanvasGroup _myCanvasGroup;
    protected IDataHub _myDataHub;
    private bool _restoreAfterPause;
    private bool _ignoreThis;
    private bool _temporaryRaycastBlock;

    //Events
    private static Action<BlockRaycast> BlockRaycasts { get; set; }
    private static Action<ActiveCanvas> ClearCanvas { get; set; }
    protected static Action RestoreBranches { get; set; }

    //Properties & Set/Getters
    protected bool InMenu => _myDataHub.InMenu;
    protected bool CanStart => _myDataHub.SceneStarted;
    protected bool GameIsPaused => _myDataHub.GamePaused;
    protected bool NoResolvePopUps => _myDataHub.NoResolvePopUp;
    private void SetFocus() => _canvasOrderCalculator.SetFocusCanvasOrder(ThisBranch.FocusSortingOrder);
    private void ResetFocus() => _canvasOrderCalculator.ResetFocus();
    protected bool IsAtRoot => _myDataHub.IsAtRoot;
    public IBranch ThisBranch { get; }
    protected bool CanAllowKeys => _myDataHub.AllowKeys;
    protected virtual void SaveInMenu(IInMenu args) { }
    protected virtual void CheckIfAtRootTrunk(IIsAtRootTrunk args) { }

    private void AllowKeys(IAllowKeys args)
    {
        if (!CanStart && CanAllowKeys)
        {
            _myCanvasGroup.blocksRaycasts = false;
            return;
        }
        if(_temporaryRaycastBlock) return;
        
        var blockRaycast = CanAllowKeys ? BlockRaycast.No : BlockRaycast.Yes;
        SetBlockRaycast(blockRaycast);
    }

    //Main
    public virtual void OnAwake()
    {
        _canvasOrderCalculator = EZInject.Class.WithParams<ICanvasOrderCalculator>(this);
        SetCanvas(ActiveCanvas.Yes);
        _myCanvasGroup.blocksRaycasts = false;
    }

    public virtual void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
        UseEZServiceLocator();
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

    public virtual void FetchEvents() { }
    
    public virtual void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IIsAtRootTrunk>(CheckIfAtRootTrunk);
        InputEvents.Do.Subscribe<IAllowKeys>(AllowKeys);
        InputEvents.Do.Subscribe<IPausePressed>(PausedPressed);
        ThisBranch.OpenBranchStartEvent += SetUpBranch;
        ThisBranch.OpenBranchEndEvent += EndOfBranchStart;
        ThisBranch.ExitBranchStartEvent += StartBranchExit;
        ThisBranch.ExitBranchEndEvent += EndOfBranchExit;
        
        if(!ThisBranch.ApplyFocus) return;
        ThisBranch.OnMouseEnterEvent += SetFocus;
        ThisBranch.OnMouseExitEvent += ResetFocus;
    }

    public virtual void UnObserveEvents()
    {
        HistoryEvents.Do.Unsubscribe<IIsAtRootTrunk>(CheckIfAtRootTrunk);
        InputEvents.Do.Unsubscribe<IAllowKeys>(AllowKeys);
        InputEvents.Do.Unsubscribe<IPausePressed>(PausedPressed);
        ThisBranch.OpenBranchStartEvent -= SetUpBranch;
        ThisBranch.OpenBranchEndEvent -= EndOfBranchStart;
        ThisBranch.ExitBranchStartEvent -= StartBranchExit;
        ThisBranch.ExitBranchEndEvent -= EndOfBranchExit;
        ThisBranch.OnMouseEnterEvent -= SetFocus;
        ThisBranch.OnMouseExitEvent -= ResetFocus;
    }

    public virtual void OnDisable()
    {
        UnObserveEvents();
        _restoreAfterPause = false;
        _temporaryRaycastBlock = false;
        _ignoreThis = false;
    }
    
    public virtual void OnDestroy()
    {
        UnObserveEvents();
        _myDataHub = null;
        _historyTrack = null;
    }

    public virtual void SetUpGOUIBranch(IGOUIModule module) { }
    
    public virtual IGOUIModule ReturnGOUIModule() => null;

    public virtual bool CanStartBranch() => true;

    public virtual bool DontExitBranch(OutTweenType outTweenType) 
        => outTweenType == OutTweenType.MoveToChild & ThisBranch.StayVisibleMovingToChild();

    public virtual void SetUpBranch(/*IBranch newParentController = null*/)
    {
        if(ThisBranch.CanvasIsEnabled) return;
        ClearCanvas += SetCanvas;
        BlockRaycasts += TempBlockRaycast;
    }

    public virtual void EndOfBranchStart()
    {
        if(!CanStart) return;

        HandleWhenActiveConditions(BlockRaycast.No, ActiveCanvas.No);
        SetBlockRaycast(CanAllowKeys? BlockRaycast.No : BlockRaycast.Yes);
    }

    public virtual void StartBranchExit() { }

    public virtual void EndOfBranchExit()
    {
        ClearCanvas -= SetCanvas;
        BlockRaycasts -= TempBlockRaycast;

        HandleWhenActiveConditions(BlockRaycast.Yes, ActiveCanvas.Yes);
        SetBlockRaycast(BlockRaycast.No);
        SetCanvas(ActiveCanvas.No);
        _canvasOrderCalculator.ResetCanvasOrder();
    }

    private void HandleWhenActiveConditions(BlockRaycast blockRaycast, ActiveCanvas activeCanvas)
    {
        _ignoreThis = true;
        
        if (ThisBranch.WhenActiveDoThis != WhenActiveDo.Nothing)
            BlockRaycasts?.Invoke(blockRaycast);

        if (ThisBranch.WhenActiveDoThis == WhenActiveDo.TurnOffAllActiveBranches)
            ClearCanvas?.Invoke(activeCanvas);
        
        _ignoreThis = false;
    }

    public virtual void SetCanvas(ActiveCanvas active)
    {
        if(_ignoreThis) return;
        _myCanvas.enabled = active == ActiveCanvas.Yes;
    }

    private void TempBlockRaycast(BlockRaycast blockRaycast)
    {
        SetBlockRaycast(blockRaycast);
        _temporaryRaycastBlock = blockRaycast == BlockRaycast.No;
    }

    public virtual void SetBlockRaycast(BlockRaycast active)
    {
        if (!_myCanvas.enabled || _ignoreThis) return;
        
        _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes;
    }
    
    private void PausedPressed(IPausePressed args)
    {
        if(_myDataHub.PausedOrEscapeTrunk(ThisBranch.ParentTrunk)) return;
        
        if (_myDataHub.GamePaused && ThisBranch.CanvasIsEnabled)
        {
            OnPause(args.ClearScreen);
        }
        else if(_restoreAfterPause)
        {
            OnUnpaused();
        }
    }

    protected virtual void OnPause(bool clearScreen)
    {
        _restoreAfterPause = true;
        if(clearScreen)
            SetCanvas(ActiveCanvas.No);
        SetBlockRaycast(BlockRaycast.No);
    }

    protected virtual void OnUnpaused()
    {
        _restoreAfterPause = false;
        SetCanvas(ActiveCanvas.Yes);
        SetBlockRaycast(BlockRaycast.Yes);
    }

}