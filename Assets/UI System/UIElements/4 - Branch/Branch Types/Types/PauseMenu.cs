using System;
using System.Collections.Generic;
using UnityEngine.Android;

/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>

[Obsolete("Reworked so works as a Standard branch", true)]
public interface IPauseBranch : IBranchBase { }

[Obsolete("Reworked so works as a Standard branch", true)]
public class PauseMenu : BranchBase, IGameIsPaused, IPauseBranch
{
    public PauseMenu(IBranch branch) : base(branch) { } 

    //Variables
    private IBranch ActiveBranch => _myDataHub.ActiveBranch;

    //Properties
   // private bool WasInGame() => !_screenData.WasOnHomeScreen;
    public bool IsPaused { get; private set; }
   // private List<IBranch> AllBranches => _myDataHub.AllActiveBranches;

    //Events
    private Action<IGameIsPaused> OnGamePaused { get; set; }
    
    public override void FetchEvents()
    {
        base.FetchEvents();
        OnGamePaused = HistoryEvents.Do.Fetch<IGameIsPaused>();
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        InputEvents.Do.Subscribe<IPausePressed>(StartPopUp);
    }

    //Main
    private void StartPopUp(IPausePressed args)
    {
        if(!CanStart) return;
        
        if (!GameIsPaused)
        {
            PauseGame();
            return;
        }
        
        if(GameIsPaused && ActiveBranch.IsPauseMenuBranch())
            UnPauseGame();
    }

    private void PauseGame()
    {
        IsPaused = true;
        OnGamePaused?.Invoke(this);
        EnterPause();
    }

    private void EnterPause()
    {
        //_screenData.StoreClearScreenData(AllBranches, ThisBranch, BlockRaycast.Yes);
        ThisBranch.OpenThisBranch();
    }

    private void UnPauseGame()
    {
        IsPaused = false;
        OnGamePaused?.Invoke(this);
        ExitPause();
    }

    private void ExitPause() => ThisBranch.ExitThisBranch(OutTweenType.Cancel);

    public override void SetUpBranch(/*IBranch newParentController = null*/)
    {
        base.SetUpBranch(/*newParentController*/);
        SetCanvas(ActiveCanvas.Yes);
        //CanGoToFullscreen();
    }

    public override void EndOfBranchExit()
    {
        base.EndOfBranchExit();
       RestoreLastStoredState();
    }

    private void RestoreLastStoredState()
    {
       // if (WasInGame()) return;
        RestoreBranches();
        _historyTrack.MoveToLastBranchInHistory();
    }
}

