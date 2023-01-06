using System;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UIElements.Input_System;
using UnityEngine;

public class HistoryTracker : IHistoryTrack, IEZEventUser, IEZEventDispatcher, IServiceUser, ICancelPause
{
    //Variables
    private InputScheme _inputScheme;

    //Properties
    private HistoryData HistoryData { get; } = new HistoryData();
    public bool NodeNeededForMultiSelect(INode node) => HistoryData.MultiSelectIsActive & 
                                                        HistoryData.History.Contains(node) & 
                                                        !_inputScheme.MultiSelectPressed();
    //Events
     private Action<ICancelPause> CancelPause { get; set; }
     
    //Main
    public void OnEnable()
    {
        FetchEvents();
        UseEZServiceLocator();
        AddService();
        ObserveEvents();
    }

    public void FetchEvents() => CancelPause = InputEvents.Do.Fetch<ICancelPause>();

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
        ClearTrunk();
        HistoryData.SwitchPressed(false);
    }

    public void ExitPause()
    {
        HistoryData.SetToThisTrunkWhenFinished(HistoryData.CurrentTrunk);
        ClearAllHistory();
    }

    private void ClearTrunk()
    {
        MultiSelectSystem.ClearMultiSelect(HistoryData);
        MoveBackInHistory.BackToThisTrunkProcess(HistoryData);
    }

    private void ClearAllHistory()
    {
        MultiSelectSystem.ClearMultiSelect(HistoryData);
        HistoryListManagement.ResetAndClearHistoryList(HistoryData, ClearAction.All);
    }
    
    public void CancelHasBeenPressed(EscapeKey cancelType, IBranch branchToCancel)
    {
        if(cancelType == EscapeKey.None) return;
        if (CheckIfCanExitPauseScreen()) return;
        
        if (HistoryData.GameIsPaused || HistoryData.NoPopUps)
        {
            if(ClearMultiSelect() || HistoryData.NoHistory) return;
            BackInHistory(cancelType);
        }
        else
        {
            PopUpController.HandlePopUps(HistoryData, branchToCancel, MoveToLastBranchInHistory);
        }
    }

    private bool CheckIfCanExitPauseScreen()
    {
        if (HistoryData.GameIsPaused & HistoryData.NoHistory)
        {
            CancelPause?.Invoke(this);
            return true;
        }
        return false;
    }

    private void BackInHistory(EscapeKey cancelType)
    {
        switch (cancelType)
        {
            case EscapeKey.BackOneLevel:
                MoveBackInHistory.BackOneLevelProcess(HistoryData);
                break;
            case EscapeKey.BackToRootTrunk:
                MoveBackInHistory.BackToRootProcess(HistoryData);
                break;
            case EscapeKey.BackToCurrentTrunk:
                MoveBackInHistory.BackToThisTrunkProcess(HistoryData);
                break;
        }
    }

    private bool ClearMultiSelect()
    {
        if (!HistoryData.MultiSelectIsActive) return false;
        
        HistoryData.SetToThisTrunkWhenFinished(HistoryData.CurrentTrunk);
        ClearAllHistory();
        return true;
    }

    public void MoveToLastBranchInHistory() => ReturnNextBranch().OpenThisBranch();

    public IBranch ReturnNextBranch()
    {
        if (HistoryData.GameIsPaused)
            return HistoryData.ActiveBranch;
        
        if (!HistoryData.NoPopUps)
        {
            return PopUpController.NextPopUp(HistoryData);
        }
        
        return HistoryData.NoHistory ? HistoryData.RootTrunk.ActiveBranch : HistoryData.ActiveBranch;
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

    public void OpenCurrentSwitchHistory()
    {
        
    }

    public void CloseCurrentSwitchHistory()
    {
        
    }

}
