using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IResolvePopUpBranch : IBranchBase { }

public class ResolvePopUp : BranchBase, IResolvePopUpBranch
{
    public ResolvePopUp(IBranch branch) : base(branch) { }

    private static Action RemovedAResolvePopUp { get; set; }

    public override void OnEnable()
    {
        base.OnEnable();
        BranchEvent.Do.Subscribe<IClearScreen>(ClearBranchForFullscreen);
        RemovedAResolvePopUp += CheckIfCanRestore;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        BranchEvent.Do.Unsubscribe<IClearScreen>(ClearBranchForFullscreen);
        RemovedAResolvePopUp -= CheckIfCanRestore;
        AdjustCanvasOrderRemoved();
    }

    protected override void CheckIfAtRootTrunk(IIsAtRootTrunk args)
    {
        if (IsAtRoot)
            CheckIfCanRestore();
        
        // if(_myCanvas.enabled)
        //     ThisBranch.WhenAllowed.IsAllowed();
    }

    public override bool CanStartBranch()
    {
        if (!_myDataHub.SceneStarted) return false;

        return _myCanvas.enabled || ThisBranch.WhenAllowed.IsAllowed();
    }

    
    private void CheckIfCanRestore()
    {
        if (!ThisBranch.WhenAllowed.RestoreBranch /*|| ThisBranch.AllowWithActiveResolvePopUp*/) return;
        
        ThisBranch.DontSetAsActiveBranch();
        ThisBranch.OpenThisBranch();
        //ThisBranch.RestoreBranch = false;
    }
    
    private void ClearBranchForFullscreen(IClearScreen args)
    {
        if (_myDataHub.GamePaused) return;

        if(ThisBranch.CanvasIsEnabled && !ThisBranch.WhenAllowed.IsAllowed(ExitAndResetBranch))
        {
            ThisBranch.ExitThisBranch(OutTweenType.Cancel);
        }
        void ExitAndResetBranch()
        {
            _canvasOrderCalculator.ResetCanvasOrder();
            ThisBranch.ExitThisBranch(OutTweenType.Cancel);
        }
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        
        if (!ThisBranch.CanvasIsEnabled)
        {
            _myDataHub.AddResolvePopUp(ThisBranch);
           // AddResolvePopUp?.Invoke(this);
            if(!ThisBranch.WhenAllowed.RestoreBranch)
                AdjustCanvasOrderAdded();
        }
        
        //TODO Update this to just use an event instead
        //_screenData.StoreClearScreenData(AllBranches, ThisBranch, BlockRaycast.Yes);
        SetCanvas(ActiveCanvas.Yes);
        ThisBranch.WhenAllowed.Restored();
        //CanGoToFullscreen();
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes;
    }

    public override void StartBranchExit()
    {
        _myDataHub.RemoveResolvePopUp(ThisBranch);
        RemovedAResolvePopUp?.Invoke();
        //RemoveResolvePopUps?.Invoke(this);
        base.StartBranchExit();
    }

    public override void EndOfBranchExit()
    {
        base.EndOfBranchExit();
        RestoreBranches?.Invoke();
       // ActivateStoredPosition();
    }

    //TODO Repeated in Optional Class so move to base or new class
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
        return _myDataHub.ActiveResolvePopUps.Select(popUp => popUp.MyCanvas).ToList();
    }
}
