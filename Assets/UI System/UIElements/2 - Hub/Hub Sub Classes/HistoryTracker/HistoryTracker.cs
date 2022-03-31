using System;
using System.Collections.Generic;
using EZ.Events;
using EZ.Service;
using UIElements.Input_System;
using UnityEngine;

public class HistoryTracker : IHistoryTrack, IEZEventUser, /*IEZEventDispatcher,*/ IServiceUser/*, INoPopUps*/
{
    //Variables
    private InputScheme _inputScheme;

    //Properties
    private HistoryData HistoryData { get; } = new HistoryData();
    public bool NodeNeededForMultiSelect(INode node) => HistoryData.MultiSelectIsActive & 
                                                        HistoryData.History.Contains(node) & 
                                                        !_inputScheme.MultiSelectPressed();
    //Events
    // private Action<INoPopUps> OnNoPopUps { get; set; }
     
    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        AddService();
      //  FetchEvents();
        ObserveEvents();
    }
    
    public void UseEZServiceLocator() => _inputScheme = EZService.Locator.Get<InputScheme>(this);

    public void AddService() => EZService.Locator.AddNew<IHistoryTrack>(this);

    public void OnRemoveService() { }
   
   // public void FetchEvents() => OnNoPopUps = PopUpEvents.Do.Fetch<INoPopUps>();

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<ISelectedNode>(SetSelected);
        HistoryEvents.Do.Subscribe<IDisabledNode>(CloseNodesAfterDisabledNode);
        HistoryEvents.Do.Subscribe<IInMenu>(SwitchToGame);
    }

    public void UnObserveEvents()
    {
        HistoryEvents.Do.Unsubscribe<ISelectedNode>(SetSelected);
        HistoryEvents.Do.Unsubscribe<IDisabledNode>(CloseNodesAfterDisabledNode);
        HistoryEvents.Do.Unsubscribe<IInMenu>(SwitchToGame);
    }

    //Main
    private void SetSelected(ISelectedNode newNode)
    {
        if(!HistoryData.CanStart) return;
        
        if (CheckForMultiSelect(newNode))
        {
            DoMultiSelect(newNode);
            return;
        }
       
        if(HistoryData.MultiSelectIsActive)
        {
            HistoryData.AddData(newNode.SelectedNode);
            HistoryListManagement.ResetAndClearHistoryList(HistoryData, ClearAction.SkipOne);
        }
        
        if(newNode.SelectedNode.HasChildBranch.IsNull()) return;
        AddNewNodeToHistory(newNode);
    }

    private bool CheckForMultiSelect(ISelectedNode newNode)
    {
        bool MultiSelectAllowed(INode node) => node.MultiSelectSettings.AllowMultiSelect == IsActive.Yes;
        return _inputScheme.MultiSelectPressed() & MultiSelectAllowed(newNode.SelectedNode);
    }

    private void DoMultiSelect(ISelectedNode newNode)
    {
        HistoryData.AddMultiSelectData(newNode.SelectedNode);
        MultiSelectSystem.MultiSelectPressed(HistoryData);
    }

    private void AddNewNodeToHistory(ISelectedNode newNode)
    {
        HistoryData.AddData(newNode.SelectedNode);
        NewSelectionProcess.AddNewSelection(HistoryData);
    }

    private void CloseNodesAfterDisabledNode(IDisabledNode args)
    {
        if(!HistoryData.History.Contains(args.ThisNode)) return;
        
        HistoryData.AddStopPoint(args.ThisNode);
        HistoryListManagement.ResetAndClearHistoryList(HistoryData, ClearAction.StopAt);
    }

    private void BackOneLevel()
    {
        if (HistoryData.MultiSelectIsActive)
        {
            HistoryData.SetToThisTrunkWhenFinished(HistoryData.CurrentTrunk);
            ClearAllHistory();
        }
        else
        {
            MoveBackInHistory.BackOneLevelProcess(HistoryData);
        }
    }

    private void SwitchToGame(IInMenu args)
    {
        if (!args.InTheMenu && HistoryData.CanStart)
        {
            HistoryData.SetToThisTrunkWhenFinished(HistoryData.RootTrunk);
            ClearAllHistory();
        }    
    }

    public void SwitchGroupPressed()
    {
        HistoryData.SwitchPressed(true);
        HistoryData.SetToThisTrunkWhenFinished(HistoryData.CurrentTrunk);
        ClearAllHistory();
        HistoryData.SwitchPressed(false);
    }

    public void ExitPause()
    {
        HistoryData.SetToThisTrunkWhenFinished(HistoryData.CurrentTrunk);
        ClearAllHistory();
    }

    private void ClearAllHistory()
    {
        MultiSelectSystem.ClearMultiSelect(HistoryData);
        HistoryListManagement.ResetAndClearHistoryList(HistoryData, ClearAction.All);
    }
    
    public void CancelHasBeenPressed(EscapeKey cancelType, IBranch branchToCancel)
    {
        if(cancelType == EscapeKey.None) return;

        // if (HistoryData.GameIsPaused)
        // {
        //     if (HistoryData.NoHistory)
        //     {
        //         Debug.Log("No History");
        //         MoveToLastBranchInHistory();
        //     }
        // }
        //Handle Cancelling out of a child in pause
        //Handle coming out of pause. Need a setting to allow this or not
        
        if (HistoryData.GameIsPaused || HistoryData.NoPopUps)
        {
            switch (cancelType)
            {
                case EscapeKey.BackOneLevel:
                    BackOneLevel();
                    break;
                case EscapeKey.BackToHome:
                    MoveBackInHistory.BackToHomeProcess(HistoryData);
                    break;
            }
        }
        else
        {
            
            if(branchToCancel.IsNotNull())
            {
                PopUpController.CloseExactPopUp(branchToCancel, MoveToLastBranchInHistory);
            }
            else
            {
                PopUpController.RemoveNextPopUp(HistoryData, MoveToLastBranchInHistory);
            }
        }
    }

    //TODO Check that GameIsPaused is relivant here once redone
    public void MoveToLastBranchInHistory()
    {
        if (HistoryData.GameIsPaused)
        {
            HistoryData.ActiveBranch.OpenThisBranch();
            return;
        }
        
        if (!HistoryData.NoPopUps)
        {
            PopUpController.NextPopUp(HistoryData).OpenThisBranch();
            return;
        }
        
        if (HistoryData.NoHistory)
        {
            MoveBackInHistory.BackToHomeProcess(HistoryData);
        }
        else
        {
            HistoryData.LastSelected().HasChildBranch.OpenThisBranch();
        }
    }
    
    public void CheckListsAndRemove(IBranch branchToClose)
    {
        if (HistoryData.MultiSelectIsActive)
        {
            MultiSelectSystem.RemoveFromMultiSelectHistory(HistoryData);
        }
        HistoryData.AddStopPoint(branchToClose.LastSelected);
        HistoryListManagement.ResetAndClearHistoryList(HistoryData, ClearAction.StopAt);
        branchToClose.LastSelected.ExitNodeByType();
    }
}
