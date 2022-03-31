using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITimedPopUpBranch : IBranchBase { }

public class TimedPopUp : BranchBase, ITimedPopUpBranch
{
    public TimedPopUp(IBranch branch) : base(branch) { }

    //Variables
    private bool _running;
    private Coroutine _coroutine;
    private static readonly List<IBranch> timedPopUps = new List<IBranch>();
    
    private static Action TimedFinished { get; set; }

    //Main

    public override void OnEnable()
    {
        base.OnEnable();
        BranchEvent.Do.Subscribe<IClearScreen>(ClearBranchForFullscreen);
        TimedFinished += CheckIfCanRestore;
        RestoreBranches += CheckIfCanRestore;
        BlockRaycasts += SetBlockRaycast;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        BranchEvent.Do.Unsubscribe<IClearScreen>(ClearBranchForFullscreen);
        timedPopUps.Remove(ThisBranch);
        TimedFinished += CheckIfCanRestore;
        RestoreBranches -= CheckIfCanRestore;
        BlockRaycasts -= SetBlockRaycast;
    }
    
    protected override void CheckIfAtRootTrunk(IIsAtRootTrunk args)
    {
        if (IsAtRoot)
            CheckIfCanRestore();
        
        // if(_myCanvas.enabled)
        //     ThisBranch.WhenAllowed.IsAllowed();

        //ThisBranch.RestoreBranch = false;
    }

    protected override void OnPause(bool clearScreen)
    {
        base.OnPause(clearScreen);
        StaticCoroutine.StopCoroutines(_coroutine);
    }

    protected override void OnUnpaused()
    {
        base.OnUnpaused();
        _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
    }

    private void CheckIfCanRestore()
    {
        if (!ThisBranch.WhenAllowed.RestoreBranch /*|| ThisBranch.AllowWithActiveResolvePopUp*/) return;
        
        //ThisBranch.DontSetAsActiveBranch();
        ThisBranch.OpenThisBranch();
        //ThisBranch.RestoreBranch = false;
    }


    public override bool CanStartBranch()
    {
        if (!_myDataHub.SceneStarted) return false;
        
        return !_running & ThisBranch.WhenAllowed.IsAllowed(timedPopUps, ThisBranch);
        //if (GameIsPaused || !CanStart || !NoResolvePopUps) return false;
        // if (ThisBranch.WhenAllowed.IsAllowed())
        // {
        //     SetIfRunningOrNot();
        //     ThisBranch.DontSetAsActiveBranch();
        //     //****Review this******
        //     //ThisBranch.RestoreBranch = true;
        //     return true;
        // }
        // //AddActiveBranch?.Invoke(this);
        // return false;
    }

    private void SetIfRunningOrNot()
    {
        // if (!_running)
        // {
            SetCanvas(ActiveCanvas.Yes);
            _running = true;
            ThisBranch.DontSetAsActiveBranch();
            AdjustCanvasOrderAdded();
      //  }
        // else
        // {
        //     ThisBranch.DoNotTween();
        // }
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        SetIfRunningOrNot();
        StaticCoroutine.StopCoroutines(_coroutine);
        _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
        ThisBranch.WhenAllowed.Restored();
    }

    private IEnumerator TimedPopUpProcess()
    {
        yield return new WaitForSeconds(ThisBranch.Timer);
        ThisBranch.ExitThisBranch(OutTweenType.Cancel);
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

    public override void StartBranchExit()
    {
        base.StartBranchExit();
        StaticCoroutine.StopCoroutines(_coroutine);
        AdjustCanvasOrderRemoved();
        _running = false;
    }

    public override void EndOfBranchExit()
    {
        base.EndOfBranchExit();
        TimedFinished?.Invoke();
    }

    private void AdjustCanvasOrderAdded()
    {
        timedPopUps.Add(ThisBranch);
        _canvasOrderCalculator.ProcessActiveCanvasses(ReturnActivePopUpsCanvases());
    }

    private void AdjustCanvasOrderRemoved()
    {
        timedPopUps.Remove(ThisBranch);
        _canvasOrderCalculator.ProcessActiveCanvasses(ReturnActivePopUpsCanvases());
    }
    
    private List<Canvas> ReturnActivePopUpsCanvases()
    {
        return timedPopUps.Select(popUp => popUp.MyCanvas).ToList();
    }

}
