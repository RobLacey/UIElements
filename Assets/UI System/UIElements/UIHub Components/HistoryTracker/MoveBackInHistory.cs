﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IMoveBackInHistory
{
    IMoveBackInHistory AddHistory(List<INode> history);
    IMoveBackInHistory IsOnHomeScreen(bool onHomeScreen);
    IMoveBackInHistory ActiveBranch(IBranch activeBranch);
    INode BackOneLevelProcess();
    INode BackToHomeProcess();
}

public class MoveBackInHistory : IMoveBackInHistory
{
    public MoveBackInHistory(IHistoryTrack historyTracker)
    {
        _historyTracker = historyTracker;
        _historyListManagement = historyTracker.HistoryListManagement;
    }
    
    //Variables
    private readonly IHistoryTrack _historyTracker;
    private readonly IHistoryManagement _historyListManagement;
    private IBranch _activeBranch;
    private List<INode> _history = new List<INode>();
    private bool _onHomeScreen, _hasHistory, _hasOnHome, _hasActiveBranch;

    //Main
    public IMoveBackInHistory AddHistory(List<INode> history)
    {
        _history = history;
        _hasHistory = true;
        return this;
    }

    public IMoveBackInHistory IsOnHomeScreen(bool onHomeScreen)
    {
        _onHomeScreen = onHomeScreen;
        _hasOnHome = true;
        return this;
    }

    public IMoveBackInHistory ActiveBranch(IBranch activeBranch)
    {
        _activeBranch = activeBranch;
        _hasActiveBranch = true;
        return this;
    }

    private void CheckForExceptionsOneLevel()
    {
        if (!_hasOnHome) throw new Exception("Missing On Home Data");
        if (!_hasHistory) throw new Exception("Missing Current History");
        if (!_hasActiveBranch) throw new Exception("Missing Active Branch");
    }
    
    private void CheckForExceptionsBackHome()
    {
        if (!_hasHistory) throw new Exception("Missing Current History");
        if (!_hasActiveBranch) throw new Exception("Missing Active Branch");
        ResetExceptionChecks();
    }

    private void ResetExceptionChecks()
    {
        _hasHistory = false;
        _hasActiveBranch = false;
        _hasOnHome = false;
    }

    public INode BackOneLevelProcess()
    {
        CheckForExceptionsOneLevel();
        if (_history.Count == 0) return null;
        
        var lastNode = _history.Last();
        
        if (IsHomeScreenBranch(lastNode))
        {
            return BackToHomeProcess();
        }
        
        _historyTracker.UpdateHistoryData(lastNode);

        _history.Remove(lastNode);
        DoMoveBackOneLevel(lastNode, _activeBranch);
        ResetExceptionChecks();
        return _history.Count == 0 ? null : _history.Last();
    }

    private bool IsHomeScreenBranch(INode lastNode) => lastNode.MyBranch.IsHomeScreenBranch() && !_onHomeScreen;
    
    private static void DoMoveBackOneLevel(INode lastNode, IBranch activeBranch)
    {
        if (lastNode.MyBranch.CanvasIsEnabled)
        {
            activeBranch.StartBranchExitProcess(OutTweenType.Cancel, ParentVisible);
        }
        else
        {
            activeBranch.StartBranchExitProcess(OutTweenType.Cancel, WithTween);
        }

        void WithTween()
        {
            if (activeBranch.AutoClose == IsActive.No)
                lastNode.DeactivateNode();
            lastNode.MyBranch.MoveToThisBranch();
        }

        void ParentVisible()
        {
            lastNode.DeactivateNode();
            lastNode.MyBranch.DoNotTween();
            lastNode.MyBranch.MoveToThisBranch();
        }
    }

    public INode BackToHomeProcess()
    {
        CheckForExceptionsBackHome();
        var lastSelected = _history.First();
        StopNodeFlash();
        _activeBranch.StartBranchExitProcess(OutTweenType.Cancel, CallBack);
        return lastSelected;

        void CallBack() => BackHomeCallBack(_history);
        void StopNodeFlash() { lastSelected.DeactivateNode(); }
    }

    private void BackHomeCallBack(List<INode> history)
    {
        if (history.Count <= 0) return;
        _historyListManagement.CurrentHistory(history)
                              .ClearAllHistory();
        _historyTracker.BackToHomeScreen();
    }
}