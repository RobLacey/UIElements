using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IOptionalPopUpBranch : IBranchBase { } 

public class OptionalPopUpPopUp : BranchBase, IAddOptionalPopUp, IOptionalPopUpBranch,
                                  IRemoveOptionalPopUp
{
    public OptionalPopUpPopUp(IBranch branch) : base(branch) { }

    //Variables
    //private static readonly List<Canvas> optionalPopUps = new List<Canvas>();

    //Properties
    public IBranch ThisPopUp => ThisBranch;
    private bool PopUpIsActive => ThisBranch.CanvasIsEnabled;
    private bool OnlyAllowOnHomeScreen => OnHomeScreen && ThisBranch.CanOnlyAllowOnHomeScreen;
    
    //Events
    private Action<IAddOptionalPopUp> AddOptionalPopUp { get; set; }
    private Action<IRemoveOptionalPopUp> RemoveOptionalPopUp { get; set; }

    //Main
    // public override void OnEnable()
    // {
    //     base.OnEnable();
    // }

    public override void OnDisable()
    {
        base.OnDisable();
         AdjustCanvasOrderRemoved();
    }

    public override void FetchEvents()
    {
        base.FetchEvents();
        AddOptionalPopUp = PopUpEvents.Do.Fetch<IAddOptionalPopUp>();
        RemoveOptionalPopUp = PopUpEvents.Do.Fetch<IRemoveOptionalPopUp>();

    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        BranchEvent.Do.Subscribe<IClearScreen>(ClearBranchForFullscreen);

    }

    public override void UnObserveEvents()
    {
        base.UnObserveEvents();
        BranchEvent.Do.Unsubscribe<IClearScreen>(ClearBranchForFullscreen);

    }

    //Make a active branch tracking list in mydata
    //

    protected override void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        //base.SaveIfOnHomeScreen(args);
        if (!ThisBranch.RestoreBranch || !OnHomeScreen) return;

        ThisBranch.DontSetAsActiveBranch();
        ThisBranch.MoveToThisBranch();
    }

    // private void ActivateWithTween()
    // {
    //     ThisBranch.DontSetBranchAsActive();
    //     ThisBranch.MoveToThisBranch();
    // }
    //
    // private void ActivateWithoutTween()
    // {
    //     ThisBranch.DontSetBranchAsActive();
    //     ThisBranch.MoveToThisBranch();
    //     // if (NoResolvePopUps)
    //     //     SetBlockRaycast(BlockRaycast.Yes);
    // }

    public override bool CanStartBranch() 
    {
        if (ThisBranch.CanBufferPopUp) ThisBranch.RestoreBranch = true;
        
        if (GameIsPaused || !OnlyAllowOnHomeScreen || !CanStart || !NoResolvePopUps) return false;
        AddActiveBranch?.Invoke(this);

        IfActiveResolvePopUps();        
        return true;
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        
        if(!PopUpIsActive)
        {
            AddOptionalPopUp?.Invoke(this);
            if(!ThisBranch.RestoreBranch)
                AdjustCanvasOrderAdded();
        }
        
        IfActiveResolvePopUps();
        SetCanvas(ActiveCanvas.Yes);
        ThisBranch.RestoreBranch = false;

    }
    
    public override void EndOfBranchExit()
    {
        //base.EndOfBranchExit();
        SetCanvas(ActiveCanvas.No);
        RemoveActiveBranch?.Invoke(this);
        RemoveOptionalPopUp?.Invoke(this);
        AdjustCanvasOrderRemoved();
    }

    private void IfActiveResolvePopUps()
    {
        if (NoResolvePopUps) return;
        ThisBranch.DontSetAsActiveBranch();
        SetBlockRaycast(BlockRaycast.No);
    }

    private void ClearBranchForFullscreen(IClearScreen args)
    {
        if(!PopUpIsActive) return;
        if (ThisBranch.CanOnlyAllowOnHomeScreen)
        {
            RemoveOrStorePopUp();
        }
    }

    private void RemoveOrStorePopUp()
    {
        if(ThisBranch.CanStoreAndRestoreOptionalPopUp)
        {
            ThisBranch.RestoreBranch = true;
        }            
        else
        {
            _canvasOrderCalculator.ResetCanvasOrder();
        }
        ThisBranch.StartBranchExitProcess(OutTweenType.Cancel);
        
        //EndOfBranchExit();
    }

    private void AdjustCanvasOrderAdded()
    {
        _canvasOrderCalculator.ProcessActiveCanvasses(ReturnActivePopUpsCanvases());
    }

    private void AdjustCanvasOrderRemoved()
    {
        _canvasOrderCalculator.ProcessActiveCanvasses(ReturnActivePopUpsCanvases());
    }

    private List<Canvas> ReturnActivePopUpsCanvases()
    {
        return _myDataHub.ActiveOptionalPopUps.Select(popUp => popUp.MyCanvas).ToList();
    }
}
