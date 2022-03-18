using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IResolvePopUpBranch : IBranchBase { }

public class ResolvePopUp : BranchBase, IAddResolvePopUp, IResolvePopUpBranch, IRemoveResolvePopUp
{
    public ResolvePopUp(IBranch branch) : base(branch) { }   

    //Variables
    //private static readonly List<Canvas> resolvePopUps = new List<Canvas>();
    
    //Properties
    public IBranch ThisPopUp => ThisBranch;
    private List<IBranch> AllBranches => _myDataHub.AllActiveBranches;

    //Events
    private Action<IAddResolvePopUp> AddResolvePopUp { get; set; }
    private Action<IRemoveResolvePopUp> RemoveResolvePopUps { get; set; }


    //Main

    public override void FetchEvents()
    {
        base.FetchEvents();
        AddResolvePopUp = PopUpEvents.Do.Fetch<IAddResolvePopUp>();
        RemoveResolvePopUps = PopUpEvents.Do.Fetch<IRemoveResolvePopUp>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        AddResolvePopUp?.Invoke(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        AdjustCanvasOrderRemoved();
    }

    public override bool CanStartBranch()
    {
        //TODO add to buffer goes here for when paused. trigger from SaveOnHome?
        if(!CanStart || GameIsPaused) return false;  
        if (!OnHomeScreen && ThisBranch.CanOnlyAllowOnHomeScreen) return false;
        AddActiveBranch?.Invoke(this);

        return true;
    }
    
    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        
        if (!ThisBranch.CanvasIsEnabled)
        {
            AdjustCanvasOrderAdded();
        }
        
        _screenData.StoreClearScreenData(AllBranches, ThisBranch, BlockRaycast.Yes);
        SetCanvas(ActiveCanvas.Yes);
        //CanGoToFullscreen();
    }

    public override void SetBlockRaycast(BlockRaycast active)
    {
        if(!CanStart) return;
        
        if (CanAllowKeys)
        {
            _myCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes;
        }
    }

    public override void EndOfBranchExit()
    {
        base.EndOfBranchExit();
        RemoveResolvePopUps?.Invoke(this);
        ActivateStoredPosition();
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
        return _myDataHub.ActiveOptionalPopUps.Select(popUp => popUp.MyCanvas).ToList();
    }
}
