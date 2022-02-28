using System;
using System.Collections.Generic;
using EZ.Events;
using NaughtyAttributes;
using UIElements;
using UnityEngine;


public class UIData : MonoBehaviour, IMonoEnable, IEZEventUser
{
    [SerializeField] private UINode _lastHighlighted = default;
    [SerializeField] private GameObject _lastHighlightedGO = default;
    [SerializeField] private UINode _lastSelected = default;
    [SerializeField] private GameObject _lastSelectedGO = default;
    [SerializeField] private UIBranch _activeBranch = default;
    [SerializeField] [ReadOnly] private bool _onHomeScreen = true;
    [SerializeField] [ReadOnly] private bool _controllingWithKeys = default;
    [SerializeField] [ReadOnly] private bool _inMenu;
    [SerializeField] private List<UINode> _selectedNodes = default;
    [SerializeField] private List<GameObject> _selectedGOs = default;
    [SerializeField] private List<UIBranch> _activeResolvePopUps;
    [SerializeField] private List<UIBranch> _activeOptionalPopUps;


    //Main
    public void OnEnable() => ObserveEvents();

    private void OnDisable() => UnObserveEvents();

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IStoreNodeHistoryData>(ManageHistory);
        HistoryEvents.Do.Subscribe<IHighlightedNode>(SaveLastHighlighted);
        HistoryEvents.Do.Subscribe<ISelectedNode>(SaveLastSelected);
        HistoryEvents.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
        InputEvents.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        HistoryEvents.Do.Subscribe<IInMenu>(SaveInMenu);
        BranchEvent.Do.Subscribe<ICloseBranch>(CloseAndReset);
        PopUpEvents.Do.Subscribe<IAddResolvePopUp>(AddResolve);
        PopUpEvents.Do.Subscribe<IAddOptionalPopUp>(AddOptional);
        PopUpEvents.Do.Subscribe<IRemoveResolvePopUp>(RemoveResolve);
        PopUpEvents.Do.Subscribe<IRemoveOptionalPopUp>(RemoveOptional);
    }


    public void UnObserveEvents()
    {
        HistoryEvents.Do.Unsubscribe<IStoreNodeHistoryData>(ManageHistory);
        HistoryEvents.Do.Unsubscribe<IHighlightedNode>(SaveLastHighlighted);
        HistoryEvents.Do.Unsubscribe<ISelectedNode>(SaveLastSelected);
        HistoryEvents.Do.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        HistoryEvents.Do.Unsubscribe<IOnHomeScreen>(SaveOnHomeScreen);
        InputEvents.Do.Unsubscribe<IAllowKeys>(SaveAllowKeys);
        HistoryEvents.Do.Unsubscribe<IInMenu>(SaveInMenu);
        BranchEvent.Do.Unsubscribe<ICloseBranch>(CloseAndReset);
        PopUpEvents.Do.Unsubscribe<IAddResolvePopUp>(AddResolve);
        PopUpEvents.Do.Unsubscribe<IAddOptionalPopUp>(AddOptional);
        PopUpEvents.Do.Unsubscribe<IRemoveResolvePopUp>(RemoveResolve);
        PopUpEvents.Do.Unsubscribe<IRemoveOptionalPopUp>(RemoveOptional);

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
        _lastHighlighted = (UINode) args.Highlighted;
        if (_lastHighlighted.InGameObject.IsNotNull())
            _lastHighlightedGO = _lastHighlighted.InGameObject;
    }
    private void SaveLastSelected(ISelectedNode args)  
    {
        _lastSelected = (UINode) args.SelectedNode;
        if(args.SelectedNode.IsNull()) return;
        
        if (_lastSelected.InGameObject.IsNotNull())
            _lastSelectedGO = _lastSelected.InGameObject;
    }
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = (UIBranch) args.ActiveBranch;
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveAllowKeys(IAllowKeys args) => _controllingWithKeys = args.CanAllowKeys;
    private void SaveInMenu(IInMenu args) => _inMenu = args.InTheMenu;
    private void AddResolve(IAddResolvePopUp args) => _activeResolvePopUps.Add((UIBranch)args.ThisPopUp);
    private void AddOptional(IAddOptionalPopUp args) => _activeOptionalPopUps.Add((UIBranch)args.ThisPopUp);
    private void RemoveResolve(IRemoveResolvePopUp args) => _activeResolvePopUps.Remove((UIBranch)args.ThisPopUp);
    private void RemoveOptional(IRemoveOptionalPopUp args) => _activeOptionalPopUps.Remove((UIBranch)args.ThisPopUp);


    private void ManageHistory(IStoreNodeHistoryData args)
    {
        if (args.NodeToUpdate is null)
        {
            _selectedNodes.Clear();
            _selectedGOs.Clear();
            return;
        }
        if (_selectedNodes.Contains((UINode)args.NodeToUpdate))
        {
            _selectedNodes.Remove((UINode) args.NodeToUpdate);
            _selectedGOs.Remove(args.NodeToUpdate.InGameObject);
        }
        else
        {
            _selectedNodes.Add((UINode) args.NodeToUpdate);
            if(args.NodeToUpdate.InGameObject.IsNull())return;
            _selectedGOs.Add(args.NodeToUpdate.InGameObject);
        }
    }
}