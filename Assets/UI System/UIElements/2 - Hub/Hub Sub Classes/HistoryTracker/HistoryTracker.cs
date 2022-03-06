﻿using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UIElements.Input_System;
using UnityEngine;

public class HistoryTracker : IHistoryTrack, IEZEventUser,/* IReturnToHome,*/ IStoreNodeHistoryData, 
                              IEZEventDispatcher, IReturnHomeGroupIndex, IServiceUser, INoPopUps
{
    //Variables
    //private INode _lastSelected;
    private InputScheme _inputScheme;

    //Properties
    public IDataHub MyDataHub { get; private set; }
    public List<INode> History { get; } = new List<INode>();
    public INode NodeToUpdate { get; private set; }
    private bool IsPaused => MyDataHub.GamePaused;
    private bool CanStart => MyDataHub.SceneStarted;
    private bool OnHomeScreen => MyDataHub.OnHomeScreen;
    private IBranch ActiveBranch => MyDataHub.ActiveBranch;
    private SelectData SelectData { get; set; }
    public bool NodeNeededForMultiSelect(INode node)
    {
        return SelectData.MultiSelectIsActive & History.Contains(node) & !_inputScheme.MultiSelectPressed();
    }

    //Events
    // private Action<IReturnToHome> HasReturnedToHomeScreen { get; set; }
     private Action<IReturnHomeGroupIndex> ReturnHomeGroupBranch { get; set; }
     private Action<INoPopUps> NoPopUps { get; set; }

    
    //TODO Remove Test Rig
    private Action<IStoreNodeHistoryData> DoAddANode { get; set; }
    
    public void UpdateHistoryData(INode node)
    {
        //TODO Add Enum to Add or Remove from History and so move that functioality here
        NodeToUpdate = node;
        DoAddANode?.Invoke(this);
    }
    
    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        AddService();
        FetchEvents();
        ObserveEvents();
        SelectData = new SelectData(this);
    }
    
    public void UseEZServiceLocator()
    {
        MyDataHub = EZService.Locator.Get<IDataHub>(this);
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
    }

    public void AddService() => EZService.Locator.AddNew<IHistoryTrack>(this);

    public void OnRemoveService() { }
   
    public void FetchEvents()
    {
        //HasReturnedToHomeScreen = HistoryEvents.Do.Fetch<IReturnToHome>();
        ReturnHomeGroupBranch = HistoryEvents.Do.Fetch<IReturnHomeGroupIndex>();
        DoAddANode = HistoryEvents.Do.Fetch<IStoreNodeHistoryData>();
        NoPopUps = PopUpEvents.Do.Fetch<INoPopUps>();
    }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<ISelectedNode>(SetSelected);
        HistoryEvents.Do.Subscribe<IDisabledNode>(CloseNodesAfterDisabledNode);
        HistoryEvents.Do.Subscribe<IInMenu>(SwitchToGame);
        //InputEvents.Do.Subscribe<IHotKeyPressed>(SetFromHotkey);
    }

    public void UnObserveEvents() { }

    //Main
    private void SetSelected(ISelectedNode newNode)
    {
        if(!CanStart) return;
        
        if (CheckForMultiSelect(newNode))
        {
            DoMultiSelect(newNode);
            return;
        }
       
        if(SelectData.MultiSelectIsActive)
        {
            SelectData.AddData(newNode.SelectedNode);
            HistoryListManagement.ResetAndClearHistoryList(SelectData, ClearAction.SkipOne);
        }
        AddNewNodeToHistory(newNode);
    }

    private bool CheckForMultiSelect(ISelectedNode newNode)
    {
        bool MultiSelectAllowed(INode node) => node.MultiSelectSettings.AllowMultiSelect == IsActive.Yes;
        return _inputScheme.MultiSelectPressed() & MultiSelectAllowed(newNode.SelectedNode);
    }

    private void DoMultiSelect(ISelectedNode newNode)
    {
        SelectData.AddMultiSelectData(newNode.SelectedNode);
        MultiSelectSystem.MultiSelectPressed(SelectData);
        //_lastSelected = newNode.SelectedNode;
    }

    private void AddNewNodeToHistory(ISelectedNode newNode)
    {
        SelectData.AddData(newNode.SelectedNode);
        /*_lastSelected = */NewSelectionProcess.AddNewSelection(SelectData);
    }

    private void CloseNodesAfterDisabledNode(IDisabledNode args)
    {
        if(!History.Contains(args.ThisNode)) return;
        
        SelectData.AddStopPoint(args.ThisNode);
        HistoryListManagement.ResetAndClearHistoryList(SelectData, ClearAction.StopAt);
    }

    private void BackOneLevel()
    {
        //TODO Review if allow multiselect not on Root Trunk
        
        if (SelectData.MultiSelectIsActive)
        {
            //_lastSelected = History.First();
            ClearAllHistory();
        }
        else
        {
            if(History.Count == 0)
            {
                Debug.Log("Return as no history");
                return;
            }
            //SelectData.AddData(_lastSelected);
            /*_lastSelected = */MoveBackInHistory.BackOneLevelProcess(SelectData);
        }
    }

    private void SwitchToGame(IInMenu args)
    {
        if (!args.InTheMenu && CanStart)
            BackToHome();
    }

    private void BackToHome()
    {
        if(History.Count == 0) return;
        /*_lastSelected = */MoveBackInHistory.BackToHomeProcess(SelectData);
    }

    public void SwitchGroupPressed()
    {
        if (!OnHomeScreen && ActiveBranch.IsInternalBranch())
        {
            BackOneLevel();
        }
        
        if(OnHomeScreen)
        {
            ClearAllHistory();
        }
    }

    private void ClearAllHistory()
    {
        MultiSelectSystem.ClearMultiSelect(SelectData);
        HistoryListManagement.ResetAndClearHistoryList(SelectData, ClearAction.All);
    }
    
    // private void SetFromHotkey(IHotKeyPressed args)
    // {
    //     
    //     //ClearAllHistory();
    //     HotKeyReturnsToHomeScreen(args.MyBranch.ParentTrunk == MyDataHub.RootTrunk);
    //     _lastSelected = args.ParentNode;
    // }
    
    // private void HotKeyReturnsToHomeScreen(bool isRootTrunk)
    // {
    //     if (isRootTrunk)
    //     {
    //         HistoryListManagement.ResetAndClearHistoryList(SelectData, ClearAction.All);
    //         MyDataHub.RootTrunk.OnStartTrunk();
    //        // BackToHomeScreen();
    //     }
    //     
    // }
    // private void HotKeyReturnsToHomeScreen(ScreenType hotKeyScreenType)
    // {
    //     if (hotKeyScreenType != ScreenType.FullScreen && !OnHomeScreen)
    //         BackToHomeScreen();
    // }

    // public void BackToHomeScreen()
    // {
    //     //TODO Add Funcioality backi in here
    //     
    //     //HasReturnedToHomeScreen?.Invoke(this);
    // }
    // // public void BackToHomeScreen() => HasReturnedToHomeScreen?.Invoke(this);

    public void CancelHasBeenPressed(EscapeKey endOfCancelAction)
    {
        if (MyDataHub.NoPopups || MyDataHub.GamePaused)
        {
            switch (endOfCancelAction)
            {
                case EscapeKey.BackOneLevel:
                    BackOneLevel();
                    break;
                case EscapeKey.BackToHome:
                    BackToHome();
                    break;
            }
        }
        else
        {
            PopUpController.RemoveNextPopUp(MyDataHub, MoveToLastBranchInHistory);
            if (MyDataHub.NoPopups)
                NoPopUps?.Invoke(this);
        }
    }

    public void MoveToLastBranchInHistory()
    {
        if (!MyDataHub.NoPopups && !IsPaused)
        {
            PopUpController.NextPopUp(MyDataHub).MoveToThisBranch();
            return;
        }
        
        if (History.Count == 0 || IsPaused)
        {
            MoveBackInHistory.BackToHomeProcess(SelectData);
        }
        else
        {
            if(History.Last().HasChildBranch.IsNotNull())
            {
                History.Last().HasChildBranch.DoNotTween();
                History.Last().HasChildBranch.MoveToThisBranch();
            }
            else
            {
                History.Last().MyBranch.DoNotTween();
                History.Last().MyBranch.MoveToThisBranch();
            }
        }
    }
    
    //TODO need to look into this use from Dynamic Branch / improve functionality
    public void ReturnToNextHomeGroup()
    {
        ReturnHomeGroupBranch?.Invoke(this);
    }

    public void CheckListsAndRemove(IBranch branchToClose)
    {
        if (SelectData.MultiSelectIsActive)
        {
            MultiSelectSystem.RemoveFromMultiSelectHistory(SelectData);
        }
        SelectData.AddStopPoint(branchToClose.LastSelected);
        HistoryListManagement.ResetAndClearHistoryList(SelectData, ClearAction.StopAt);
        branchToClose.LastSelected.ExitNodeByType();
    }
}