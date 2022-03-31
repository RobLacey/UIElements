using System;
using System.Collections.Generic;
using EZ.Events;
using EZ.Service;
using NaughtyAttributes;
using UIElements;
using UnityEngine;

[Obsolete("Moved to DataHub", true)]
public class UIData : MonoBehaviour, IMonoEnable, IEZEventUser, IServiceUser
{
    [SerializeField] private Node _lastHighlighted = default;
    [SerializeField] private GameObject _lastHighlightedGO = default;
    [SerializeField] private Node _lastSelected = default;
    [SerializeField] private GameObject _lastSelectedGO = default;
    [SerializeField] private Branch _activeBranch = default;
    [SerializeField] private Trunk _currentTrunk = default;
    [SerializeField] [ReadOnly] private bool _onHomeScreen = true;
    [SerializeField] [ReadOnly] private bool _controllingWithKeys = default;
    [SerializeField] [ReadOnly] private bool _inMenu;
    [SerializeField] [ReadOnly] private bool _gameIsPaused;
    [SerializeField] private List<Trunk> _activeTrunks = default;
    [SerializeField] private List<Node> _selectedNodes = default;
    [SerializeField] private List<GameObject> _selectedGOs = default;
    [SerializeField] private List<Branch> _activeResolvePopUps;
    [SerializeField] private List<Branch> _activeOptionalPopUps;


    private IDataHub _myDataHub;

    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        ObserveEvents();
    }
    private void OnDisable() => UnObserveEvents();
    
    public void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }


    public void ObserveEvents()
    {
        // HistoryEvents.Do.Subscribe<IStoreNodeHistoryData>(ManageHistory);
        // HistoryEvents.Do.Subscribe<IHighlightedNode>(SaveLastHighlighted);
        // HistoryEvents.Do.Subscribe<ISelectedNode>(SaveLastSelected);
        // //HistoryEvents.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        // HistoryEvents.Do.Subscribe<IIsAtRootTrunk>(SaveOnHomeScreen);
        // HistoryEvents.Do.Subscribe<IAddTrunk>(AddTrunk);
        // HistoryEvents.Do.Subscribe<IRemoveTrunk>(RemoveTrunk);
        // InputEvents.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        // HistoryEvents.Do.Subscribe<IInMenu>(SaveInMenu);
        // InputEvents.Do.Subscribe<IPausePressed>(GameIsPaused);
        // BranchEvent.Do.Subscribe<ICloseBranch>(CloseAndReset);
        // PopUpEvents.Do.Subscribe<IAddResolvePopUp>(AddResolve);
        // PopUpEvents.Do.Subscribe<IAddOptionalPopUp>(AddOptional);
        // PopUpEvents.Do.Subscribe<IRemoveResolvePopUp>(RemoveResolve);
        // PopUpEvents.Do.Subscribe<IRemoveOptionalPopUp>(RemoveOptional);
    }

    public void UnObserveEvents()
    {
       //  HistoryEvents.Do.Unsubscribe<IStoreNodeHistoryData>(ManageHistory);
       //  HistoryEvents.Do.Unsubscribe<IHighlightedNode>(SaveLastHighlighted);
       //  HistoryEvents.Do.Unsubscribe<ISelectedNode>(SaveLastSelected);
       // // HistoryEvents.Do.Unsubscribe<IActiveBranch>(SaveActiveBranch);
       //  HistoryEvents.Do.Unsubscribe<IIsAtRootTrunk>(SaveOnHomeScreen);
       //  HistoryEvents.Do.Unsubscribe<IAddTrunk>(AddTrunk);
       //  HistoryEvents.Do.Unsubscribe<IRemoveTrunk>(RemoveTrunk);
       //  InputEvents.Do.Unsubscribe<IAllowKeys>(SaveAllowKeys);
       //  HistoryEvents.Do.Unsubscribe<IInMenu>(SaveInMenu);
       //  InputEvents.Do.Unsubscribe<IPausePressed>(GameIsPaused);
       //  BranchEvent.Do.Unsubscribe<ICloseBranch>(CloseAndReset);
       //  PopUpEvents.Do.Unsubscribe<IAddResolvePopUp>(AddResolve);
       //  PopUpEvents.Do.Unsubscribe<IAddOptionalPopUp>(AddOptional);
       //  PopUpEvents.Do.Unsubscribe<IRemoveResolvePopUp>(RemoveResolve);
       //  PopUpEvents.Do.Unsubscribe<IRemoveOptionalPopUp>(RemoveOptional);

    }

    private void CloseAndReset(ICloseBranch args)
    {
        if (_lastHighlightedGO.IsEqualTo(args.TargetBranch.LastHighlighted.InGameObject))
            _lastHighlightedGO = null;
        if (_lastSelectedGO.IsEqualTo(args.TargetBranch.LastSelected.InGameObject))
            _lastSelectedGO = null;
    }

    private void SaveLastHighlighted(IHighlightedNode args)
    {
        _lastHighlighted = (Node) args.Highlighted;
        if (_lastHighlighted.InGameObject.IsNotNull())
            _lastHighlightedGO = _lastHighlighted.InGameObject;
    }
    private void SaveLastSelected(ISelectedNode args)  
    {
        _lastSelected = (Node) args.SelectedNode;
        if(args.SelectedNode.IsNull()) return;
        
        if (_lastSelected.InGameObject.IsNotNull())
            _lastSelectedGO = _lastSelected.InGameObject;
    }

    //private void SaveActiveBranch(IActiveBranch args) => _activeBranch = (Branch)_myDataHub.ActiveBranch;
    private void SaveOnHomeScreen(IIsAtRootTrunk args) => _onHomeScreen = _myDataHub.IsAtRoot;
    private void SaveAllowKeys(IAllowKeys args) => _controllingWithKeys = args.CanAllowKeys;
    private void SaveInMenu(IInMenu args) => _inMenu = args.InTheMenu;
    private void AddResolve(IAddResolvePopUp args) => _activeResolvePopUps.Add((Branch)args.ThisPopUp);
    private void AddOptional(IAddOptionalPopUp args) => _activeOptionalPopUps.Add((Branch)args.ThisPopUp);
    private void RemoveResolve(IRemoveResolvePopUp args) => _activeResolvePopUps.Remove((Branch)args.ThisPopUp);
    private void RemoveOptional(IRemoveOptionalPopUp args) => _activeOptionalPopUps.Remove((Branch)args.ThisPopUp);
    private void GameIsPaused(IPausePressed args) => _gameIsPaused = _myDataHub.GamePaused;

    // private void AddTrunk(IAddTrunk trunkData)
    // {
    //     _currentTrunk = trunkData.ThisTrunk;
    //     if(_activeTrunks.Contains(trunkData.ThisTrunk)) return;
    //     _activeTrunks.Add(trunkData.ThisTrunk);
    // }
    //
    // private void RemoveTrunk(IRemoveTrunk trunkData)
    // {
    //     if (!_activeTrunks.Contains(trunkData.ThisTrunk)) return;
    //     _activeTrunks.Remove(trunkData.ThisTrunk);
    // }


    // private void ManageHistory(IStoreNodeHistoryData args)
    // {
    //     if (_selectedNodes.Contains(args.NodeToUpdate))
    //     {
    //         _selectedNodes.Remove(args.NodeToUpdate);
    //         _selectedGOs.Remove(args.NodeToUpdate.InGameObject);
    //     }
    //     else
    //     {
    //         _selectedNodes.Add(args.NodeToUpdate);
    //         if(args.NodeToUpdate.InGameObject.IsNull())return;
    //         _selectedGOs.Add(args.NodeToUpdate.InGameObject);
    //     }
    // }

}