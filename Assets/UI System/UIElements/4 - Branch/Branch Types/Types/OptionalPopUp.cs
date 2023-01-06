using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IOptionalPopUpBranch : IBranchBase { } 

public class OptionalPopUpPopUp : BranchBase, IOptionalPopUpBranch
                                  
{
    public OptionalPopUpPopUp(IBranch branch) : base(branch) { }
    private bool _running;
    private Coroutine _coroutine;


    // //Events
     private static Action OptionalPopUpExited { get; set; }

    //Main
    public override void OnDisable()
    {
        base.OnDisable();
        StaticCoroutine.StopCoroutines(_coroutine);
        AdjustCanvasOrderRemoved();
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        BranchEvent.Do.Subscribe<IClearScreen>(ClearBranchForFullscreen);
        OptionalPopUpExited += CheckIfCanRestore;
        RestoreBranches += CheckIfCanRestore;
    }

    public override void UnObserveEvents()
    {
        base.UnObserveEvents();
        BranchEvent.Do.Unsubscribe<IClearScreen>(ClearBranchForFullscreen);
        OptionalPopUpExited -= CheckIfCanRestore;
        RestoreBranches -= CheckIfCanRestore;
    }

    protected override void CheckIfAtRootTrunk(IIsAtRootTrunk args)
    {
        if(GameIsPaused) return;
        
        if (IsAtRoot)
           CheckIfCanRestore();

        if (ThisBranch.CanvasIsEnabled && !ThisBranch.WhenAllowed.IsAllowed())
        {
            ThisBranch.ExitThisBranch(OutTweenType.Cancel);
        }
    }

    private void CheckIfCanRestore()
    {
        if (!ThisBranch.WhenAllowed.RestoreBranch ) return;
        
        ThisBranch.DontSetAsActiveBranch();
        ThisBranch.OpenThisBranch();
    }

    public override bool CanStartBranch()
    {
        if (!_myDataHub.SceneStarted) return false;
        
        return ThisBranch.WhenAllowed.IsAllowed(_myDataHub.ActiveOptionalPopUps, ThisBranch);
    }

    public override void SetUpBranch(/*IBranch newParentController = null*/)
    {
        base.SetUpBranch(/*newParentController*/);

        if(!ThisBranch.CanvasIsEnabled)
        {
            _myDataHub.AddOptionalPopUp(ThisBranch);
            if(!ThisBranch.WhenAllowed.RestoreBranch)
                AdjustCanvasOrderAdded();
            
            if (ThisBranch.Timer > 0)
            {
                if(_running)
                {
                    StaticCoroutine.StopCoroutines(_coroutine);
                    ThisBranch.DoNotTween();
                }                
                _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
                _running = true;
            }
        }
        IfActiveResolvePopUps();

        SetCanvas(ActiveCanvas.Yes);
        ThisBranch.WhenAllowed.Restored();
    }
    
    private IEnumerator TimedPopUpProcess()
    {
        yield return new WaitForSeconds(ThisBranch.Timer);
        _canvasOrderCalculator.ResetCanvasOrder();
        ThisBranch.ExitThisBranch(OutTweenType.Cancel, ()=> _historyTrack.MoveToLastBranchInHistory());
    }


    public override void StartBranchExit()
    {
        _myDataHub.RemoveOptionalPopUp(ThisBranch);
        base.StartBranchExit();
        StaticCoroutine.StopCoroutines(_coroutine);
        _running = false;
    }

    public override void EndOfBranchExit()
    {
        base.EndOfBranchExit();
        OptionalPopUpExited?.Invoke();
        
        if(ThisBranch.WhenAllowed.RestoreBranch) return;
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

