﻿using UnityEngine;

public interface IGOUIBranch: IBranchBase { }

public class GOUIBranch : BranchBase, IGOUIBranch
{
    public GOUIBranch(IBranch branch) : base(branch) { }
    
    //Variables
    private IGOUIModule _myGOUIModule;
    private Vector3 _inGameObjectLastFrameScreenPosition;
    private bool _canStartGOUI;
    private ISetScreenPosition _setScreenPosition;


    //Properties & Getters / Setters
    private bool AlwaysOn => _myGOUIModule.AlwaysOnIsActive;

    public override void OnAwake()
    {
        base.OnAwake();
        _setScreenPosition = EZInject.Class.WithParams<ISetScreenPosition>(this);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        _setScreenPosition.OnEnable();
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        GOUIEvents.Do.Subscribe<IStartGOUIBranch>(StartGOUIBranch);
        BranchEvent.Do.Subscribe<IClearScreen>(ClearBranchForFullscreen);
    }

    public override void UnObserveEvents()
    {
        base.UnObserveEvents();
        GOUIEvents.Do.Unsubscribe<IStartGOUIBranch>(StartGOUIBranch);
        BranchEvent.Do.Unsubscribe<IClearScreen>(ClearBranchForFullscreen);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UnObserveEvents();
        _setScreenPosition.OnDisable();
        _canStartGOUI = false;
        SetCanvas(ActiveCanvas.No);
        SetBlockRaycast(BlockRaycast.No);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnObserveEvents();
        _setScreenPosition.OnDisable();
    }

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
       // base.SaveIfOnHomeScreen(args);
        
        if(_myGOUIModule.IsNull()) return;
        
        if(OnHomeScreen)
        {
            if(AlwaysOn)
            {
                SetCanvas(ActiveCanvas.Yes);
                SetBlockRaycast(BlockRaycast.Yes);
            }        
        }
        else
        {
            SetBlockRaycast(BlockRaycast.No);
        }
    }

    private void StartGOUIBranch(IStartGOUIBranch args)
    {
        if (ReferenceEquals(args.TargetBranch, ThisBranch))
        {
            _canStartGOUI = true;
        }
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(_myCanvasGroup == null) return;
        
        if (OnHomeScreen)
        {
            _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes;
        }
    }

    //TODO Check if I need this
    public override void SetCanvas(ActiveCanvas active)
    {
        if(!OnHomeScreen || _myCanvas == null) return;
        base.SetCanvas(active);
    }

    public override void SetUpGOUIBranch(IGOUIModule module)
    {
        _myGOUIModule = module;
        _setScreenPosition.InGameObjectPosition = module.GOUITransform;

        var nodes = ThisBranch.ThisBranchesGameObject.GetComponentsInChildren<INode>();
        
        foreach (var node in nodes)
        {
            node.SetGOUIModule(module);
        }
    }

    public override IGOUIModule ReturnGOUIModule() => _myGOUIModule;

    //Main
    public override bool CanStartBranch()
    {
        AddActiveBranch?.Invoke(this);
        return _canStartGOUI || AlwaysOn || CanAllowKeys;
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        bool AlwaysOnActivated() => AlwaysOn && ThisBranch.CanvasIsEnabled;

        base.SetUpBranch(newParentController);
        _canvasOrderCalculator.SetCanvasOrder();
        
        if(ThisBranch.CanvasIsEnabled || AlwaysOnActivated() || !_canStartGOUI )
        {
            ThisBranch.DoNotTween();
        }
        
        if(_myGOUIModule.PointerOver)
        {
            SetCanvas(ActiveCanvas.Yes);
            SetBlockRaycast(BlockRaycast.Yes);
        }  
        
        _setScreenPosition.StartSetting();
    }

    protected void ClearBranchForFullscreen(IClearScreen args)
    {
        _canvasOrderCalculator.ResetCanvasOrder();
    }

    public override void EndOfBranchStart()
    {
        base.EndOfBranchStart();
        _canStartGOUI = false;
    }

    public override bool CanExitBranch(OutTweenType outTweenType)
    {
        base.CanExitBranch(outTweenType);
        if (outTweenType == OutTweenType.Cancel)
        {
            _canStartGOUI = true;
        }
        return !AlwaysOn && !_myGOUIModule.PointerOver;
    }

    public override void StartBranchExit()
    {
        base.StartBranchExit();
        _setScreenPosition.Stop();
    }
    
    // public override void EndOfBranchExit()
    // {
    //     base.EndOfBranchExit();
    //     _canvasOrderCalculator.ResetCanvasOrder();
    // }
}