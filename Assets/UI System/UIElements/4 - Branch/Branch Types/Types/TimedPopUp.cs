using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimedPopUpBranch : IBranchBase { }

public class TimedPopUp : BranchBase, ITimedPopUpBranch
{
    public TimedPopUp(IBranch branch) : base(branch) { }

    //Variables
    private bool _running;
    private Coroutine _coroutine;
    private static readonly List<Canvas> timedPopUps = new List<Canvas>();

    //Main
    public override bool CanStartBranch()
    {
        if (GameIsPaused || !CanStart || !NoResolvePopUps) return false;
        if (!OnHomeScreen && ThisBranch.CanOnlyAllowOnHomeScreen) return false;
        AddActiveBranch?.Invoke(this);
        SetIfRunningOrNot();
        ThisBranch.DontSetAsActiveBranch();
        return true;
    }

    private void SetIfRunningOrNot()
    {
        if (!_running)
        {
            SetCanvas(ActiveCanvas.Yes);
            _running = true;
            AdjustCanvasOrderAdded();
        }
        else
        {
            ThisBranch.DoNotTween();
        }
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        StaticCoroutine.StopCoroutines(_coroutine);
        _coroutine = StaticCoroutine.StartCoroutine(TimedPopUpProcess());
    }

    private IEnumerator TimedPopUpProcess()
    {
        yield return new WaitForSeconds(ThisBranch.Timer);
        ExitTimedPopUp();
    }
    
    private void ExitTimedPopUp()
    {
        AdjustCanvasOrderRemoved();
        _running = false;
        ThisBranch.StartBranchExitProcess(OutTweenType.Cancel);
    }

    private void AdjustCanvasOrderAdded()
    {
        timedPopUps.Add(ThisBranch.MyCanvas);
        _canvasOrderCalculator.ProcessActiveCanvasses(timedPopUps);
    }

    private void AdjustCanvasOrderRemoved()
    {
        timedPopUps.Remove(ThisBranch.MyCanvas);
        _canvasOrderCalculator.ProcessActiveCanvasses(timedPopUps);
    }
}
