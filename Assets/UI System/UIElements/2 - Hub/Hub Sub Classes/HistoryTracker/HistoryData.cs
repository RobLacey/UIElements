﻿using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Service;
using UIElements;

public class HistoryData : IServiceUser
{
    public HistoryData() => UseEZServiceLocator();

    public void UseEZServiceLocator()
    {
        MyDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    public void AddData(INode newNode)
    {
        NewNode = newNode;
        MyDataHub.SetMultiSelect(false);
    }
    
    public void AddStopPoint(INode newNode)
    {
        StopPoint = newNode;
        MyDataHub.SetMultiSelect(false);
    }

    public void SetToThisTrunkWhenFinished(Trunk trunk) => SetToThisTrunk = trunk;
    public void AddToHistory(INode node) => MyDataHub.ManageHistory(node);
    public void RemoveFromHistory(INode node) => MyDataHub.ManageHistory(node);
    public void AddMultiSelectData(INode newNode) => NewNode = newNode;
    public void SwitchPressed(bool pressed) => MyDataHub.SwitchPressed(pressed);
    public void MultiSelectOn() => MyDataHub.SetMultiSelect(true);
    public void MultiSelectOff() => MyDataHub.SetMultiSelect(false);
    public INode NewNode { get; private set; }
    public INode StopPoint { get; private set; }
    public INode LastSelected() => MyDataHub.History.IsNotEmpty() ? MyDataHub.History.Last() : null;

    public Action EndOfTrunkCloseAction { get; set; }
    public List<INode> History => MyDataHub.History;
    public bool GameIsPaused => MyDataHub.GamePaused;
    public bool CanStart => MyDataHub.SceneStarted;
    public bool NoPopUps => MyDataHub.NoPopUps;
    public bool NoHistory => MyDataHub.NoHistory;
    public List<IBranch> ActiveResolvePopUps => MyDataHub.ActiveResolvePopUps;
    public List<IBranch> ActiveOptionalPopUps => MyDataHub.ActiveOptionalPopUps;
    public bool HasActiveResolvePopUps => MyDataHub.ActiveResolvePopUps.IsNotEmpty();
    public ScreenType ScreenTypeOfDestinationTrunk => NewNode.HasChildBranch.ParentTrunk.ScreenType;
    public Trunk DestinationTrunk => NewNode.HasChildBranch.ParentTrunk;
    public IBranch NewNodesBranch => NewNode.MyBranch;
    public IBranch ActiveBranch => MyDataHub.ActiveBranch;
    public Trunk CurrentTrunk => MyDataHub.CurrentTrunk;
    public bool MultiSelectIsActive => MyDataHub.MultiSelectActive;
    private IDataHub MyDataHub { get; set; }
    public Trunk RootTrunk => MyDataHub.RootTrunk;
    public Trunk SetToThisTrunk { get; private set; }
    public List<Trunk> ActiveTrunks => MyDataHub.ActiveTrunks;
    public OutTweenType TweenType { get; set; }

}