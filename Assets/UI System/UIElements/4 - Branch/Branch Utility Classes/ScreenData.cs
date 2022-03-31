﻿using System;
using System.Collections.Generic;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

[Obsolete("Not Needed", true)]

public interface IScreenData : IMonoEnable,IMonoDisable
{
    bool WasOnHomeScreen { get; }
    void RestoreScreen();
    void StoreClearScreenData(List<IBranch> allBranches, IBranch thisBranch, BlockRaycast blockRaycast);
}

[Obsolete("Not Needed", true)]
public class ScreenData : IScreenData, IServiceUser, IEZEventUser
{
    public ScreenData(IBranchParams branch)
    {
        _myBranch = branch.ThisBranch;
    }

    //Variables
    private readonly List<IBranch> _clearedBranches = new List<IBranch>();

    private bool IsFullscreen()
    {
        if (_myBranch.ParentTrunk.IsNull()) return false;
        return _myBranch. ParentTrunk. ScreenType == ScreenType. FullScreen;
    }
    private IDataHub _myDataHub;
    private IBranch _myBranch;

    //Propertues
    public bool WasOnHomeScreen { get; private set; } = true;

    public void OnEnable()
    {
        UseEZServiceLocator();
        ObserveEvents();
    }
    
    public void ObserveEvents() => BranchEvent.Do.Subscribe<ICloseBranch>(RemoveBranch);
    public void UnObserveEvents() => BranchEvent.Do.Unsubscribe<ICloseBranch>(RemoveBranch);

    private void RemoveBranch(ICloseBranch args)
    {
        if (_clearedBranches.Contains(args.TargetBranch))
            _clearedBranches.Remove(args.TargetBranch);
    }

    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    public void OnDisable()
    {
        UnObserveEvents();
        _clearedBranches.Clear();
    }

    //Main
    public void StoreClearScreenData(List<IBranch> allBranches, IBranch thisBranch, BlockRaycast blockRaycast)
    {
        WasOnHomeScreen = _myDataHub.IsAtRoot;
        StoreActiveBranches(allBranches, thisBranch, blockRaycast == BlockRaycast.Yes);
    }

    private void StoreActiveBranches(List<IBranch> allBranches, IBranch thisBranch, bool blockRaycast)
    {
        foreach (var branchToClear in allBranches)
        {
            if(branchToClear == thisBranch) continue;
            
            if (branchToClear.CanvasIsEnabled)
                _clearedBranches.Add(branchToClear);
            
            if (blockRaycast) 
                branchToClear.SetBlockRaycast(BlockRaycast.No);
        }
    }

    public void RestoreScreen()
    {
        if(_clearedBranches.Count == 0) return;
        
        foreach (var branch in _clearedBranches)
        {
            if(IsFullscreen())
                branch.SetCanvas(ActiveCanvas.Yes);
            branch.SetBlockRaycast(BlockRaycast.Yes);
        }
        _clearedBranches.Clear();
    }
}
