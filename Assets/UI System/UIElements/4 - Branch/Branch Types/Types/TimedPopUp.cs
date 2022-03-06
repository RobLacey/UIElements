﻿using System.Collections;
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
        if (!OnHomeScreen && _myBranch.ReturnOnlyAllowOnHomeScreen == IsActive.Yes) return false;

        SetIfRunningOrNot();
        _myBranch.DontSetBranchAsActive();
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
            _myBranch.DoNotTween();
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
        yield return new WaitForSeconds(_myBranch.Timer);
        ExitTimedPopUp();
    }
    
    private void ExitTimedPopUp()
    {
        AdjustCanvasOrderRemoved();
        _running = false;
        _myBranch.StartBranchExitProcess(OutTweenType.Cancel);
    }

    private void AdjustCanvasOrderAdded()
    {
        timedPopUps.Add(_myBranch.MyCanvas);
        _canvasOrderCalculator.ProcessActiveCanvasses(timedPopUps);
    }

    private void AdjustCanvasOrderRemoved()
    {
        timedPopUps.Remove(_myBranch.MyCanvas);
        _canvasOrderCalculator.ProcessActiveCanvasses(timedPopUps);
    }
}
