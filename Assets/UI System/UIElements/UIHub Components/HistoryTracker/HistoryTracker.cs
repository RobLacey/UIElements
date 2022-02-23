using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UIElements.Input_System;
using UnityEngine;

public class HistoryTracker : IHistoryTrack, IEZEventUser, IReturnToHome, IStoreNodeHistoryData, 
                              IEZEventDispatcher, IReturnHomeGroupIndex, IServiceUser
{
    public HistoryTracker() => PopUpHistory = EZInject.Class.WithParams<IManagePopUpHistory>(this);

    //Variables
    private readonly List<INode> _history = new List<INode>();
    private INode _lastSelected;
    private IDataHub _myDataHub;
    private InputScheme _inputScheme;

    //Properties
    private IManagePopUpHistory PopUpHistory { get; }
    public INode NodeToUpdate { get; private set; }
    public bool NoHistory => _history.Count == 0;
    private bool IsPaused => _myDataHub.GamePaused;
    private bool CanStart => _myDataHub.SceneStarted;
    private bool OnHomeScreen => _myDataHub.OnHomeScreen;
    private IBranch ActiveBranch => _myDataHub.ActiveBranch;
    public INode TargetNode { get; set; }
    private SelectData SelectData { get; set; }
    public bool NodeNeededForMultiSelect(INode node)
    {
        return SelectData.MultiSelectIsActive & _history.Contains(node) & !_inputScheme.MultiSelectPressed();
    }

    //Events
    private Action<IReturnToHome> HasReturnedToHomeScreen { get; set; }
    private Action<IReturnHomeGroupIndex> ReturnHomeGroupBranch { get; set; }
    
    //TODO Remove Test Rig
    private Action<IStoreNodeHistoryData> DoAddANode { get; set; }
    
    public void UpdateHistoryData(INode node)
    {
        NodeToUpdate = node;
        DoAddANode?.Invoke(this);
    }
    
    //Main
    public void OnEnable()
    {
        Debug.Log("Upto : Finish changing over popUpManger to static the look at GOUI methods in here to see if needed");
        UseEZServiceLocator();
        AddService();
        FetchEvents();
        ObserveEvents();
        PopUpHistory.OnEnable();
        SelectData = new SelectData(this);
    }

    
    public void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
    }

    public void AddService() => EZService.Locator.AddNew<IHistoryTrack>(this);

    public void OnRemoveService() { }
   
    public void FetchEvents()
    {
        HasReturnedToHomeScreen = HistoryEvents.Do.Fetch<IReturnToHome>();
        ReturnHomeGroupBranch = HistoryEvents.Do.Fetch<IReturnHomeGroupIndex>();
        DoAddANode = HistoryEvents.Do.Fetch<IStoreNodeHistoryData>();
    }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<ISelectedNode>(SetSelected);
        HistoryEvents.Do.Subscribe<IDisabledNode>(CloseNodesAfterDisabledNode);
        HistoryEvents.Do.Subscribe<IInMenu>(SwitchToGame);
        InputEvents.Do.Subscribe<IHotKeyPressed>(SetFromHotkey);
        InputEvents.Do.Subscribe<ISwitchGroupPressed>(SwitchGroupPressed);
        CancelEvents.Do.Subscribe<ICancelPopUp>(CancelPopUpFromButton);
    }

    public void UnObserveEvents() { }

    public void GOUIBranchHasClosed(IBranch branchToClose, bool noGOUILeft)
    {
        if(!branchToClose.IsInGameBranch()) return;
        
        NoGOUIAndWasActiveBranch(noGOUILeft, ActiveBranch == branchToClose);
        
        if(!_history.Contains(branchToClose.LastSelected)) return;

        CheckListsAndRemove(branchToClose);
        
        if(_history.Count > 0)
            _history.Last().MyBranch.MoveToThisBranch();
    }

    private void NoGOUIAndWasActiveBranch(bool noGOUILeft, bool activeBranch)
    {
        if (!noGOUILeft || !activeBranch) return;
        ReturnToNextHomeGroup();
    }

    public void ReturnToNextHomeGroup()
    {
        ReturnHomeGroupBranch?.Invoke(this);
        TargetNode.MyBranch.MoveToThisBranch();
    }

    private void CheckListsAndRemove(IBranch branchToClose)
    {
        if (SelectData.MultiSelectIsActive)
        {
            SelectData.AddMultiSelectData(branchToClose.LastSelected, _history);
            MultiSelectSystem.RemoveFromMultiSelectHistory(SelectData);
            branchToClose.LastSelected.ExitNodeByType();
        }
        else
        {
            SelectData.AddData(null,branchToClose.LastSelected, _history, IsPaused);
            HistoryListManagement.ClearGOUIBranchFromHistory(SelectData);
        }
    }

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
           ClearAllHistory(newNode.SelectedNode);
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
        SelectData.AddMultiSelectData(newNode.SelectedNode, _history);
        MultiSelectSystem.MultiSelectPressed(SelectData);
        _lastSelected = newNode.SelectedNode;
    }

    private void AddNewNodeToHistory(ISelectedNode newNode)
    {
        SelectData.AddData(newNode.SelectedNode, _lastSelected, _history, IsPaused);
        _lastSelected = NewSelectionProcess.AddNewSelection(SelectData);
    }

    private void CloseNodesAfterDisabledNode(IDisabledNode args)
    {
        SelectData.AddData(args.ThisNode, _lastSelected, _history, IsPaused);
        HistoryListManagement.CloseToThisPointInHistory(SelectData);
    }
    
    public void BackOneLevel()
    {
        if (SelectData.MultiSelectIsActive)
        {
            _lastSelected = _history.First();
            ClearAllHistory();
        }
        else
        {
            if(_history.Count == 0) return;
            SelectData.AddMoveBackData(_history, ActiveBranch, OnHomeScreen);
            _lastSelected = MoveBackInHistory.BackOneLevelProcess(SelectData);
        }
    }

    private void SwitchToGame(IInMenu args)
    {
        if (!args.InTheMenu && CanStart)
            BackToHome();
    }

    public void BackToHome()
    {
        if(_history.Count == 0) return;

        SelectData.AddMoveBackData(_history, ActiveBranch, OnHomeScreen);
        _lastSelected = MoveBackInHistory.BackToHomeProcess(SelectData);
    }

    private void SwitchGroupPressed(ISwitchGroupPressed args)
    {
        if (!OnHomeScreen && ActiveBranch.IsInternalBranch())
        {
            BackOneLevel();
        }
        else
        {
            ClearAllHistory();
        }
    }

    private void ClearAllHistory(INode toSkip = null)
    {
        SelectData.AddData(toSkip, _lastSelected, _history, IsPaused);
        MultiSelectSystem.ClearMultiSelect(SelectData);
        HistoryListManagement.ResetAndClearHistoryList(SelectData, ClearAction.SkipOne);
    }
    
    private void SetFromHotkey(IHotKeyPressed args)
    {
        HotKeyReturnsToHomeScreen(args.MyBranch.ScreenType);
        ClearAllHistory();
        _lastSelected = args.ParentNode;
    }
    
    private void HotKeyReturnsToHomeScreen(ScreenType hotKeyScreenType)
    {
        if (hotKeyScreenType != ScreenType.FullScreen && !OnHomeScreen)
            BackToHomeScreen();
    }

    public void BackToHomeScreen() => HasReturnedToHomeScreen?.Invoke(this);

    public void MoveToLastBranchInHistory()
    {
        if (!_myDataHub.NoPopups && !IsPaused)
        {
            PopUpHistory.MoveToNextPopUp();
            return;
        }
        
        if (_history.Count == 0 || IsPaused)
        {
            BackToHomeScreen();
        }
        else
        {
            if(_history.Last().HasChildBranch.IsNotNull())
            {
                _history.Last().HasChildBranch.DoNotTween();
                _history.Last().HasChildBranch.MoveToThisBranch();
            }
            else
            {
                _history.Last().MyBranch.DoNotTween();
                _history.Last().MyBranch.MoveToThisBranch();
            }
        }
    }
    
    private void CancelPopUpFromButton(ICancelPopUp popUpToCancel) => PopUpHistory.HandlePopUps(popUpToCancel.MyBranch);

    public void CheckForPopUpsWhenCancelPressed(Action endOfCancelAction)
    {
        PopUpHistory.NoPopUpAction(endOfCancelAction)
                    .DoPopUpCheckAndHandle();
    }
}
