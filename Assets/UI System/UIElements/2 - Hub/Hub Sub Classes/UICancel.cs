using System;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

interface ICancel
{
    void OnEnable();
}

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel : ICancel, IServiceUser, IEZEventUser, IMonoEnable
{
    //Variables
    private IHistoryTrack _uiHistoryTrack;
    //private InputScheme _inputScheme;
    private IDataHub _myDataHub;


    //Properties 7 Getters / Setters
   // private bool GameIsPaused => _myDataHub.GamePaused;
    private bool ActiveResolvePopUps => !_myDataHub.NoResolvePopUp;
    private EscapeKey GlobalEscapeSetting => _myDataHub.GlobalEscapeSetting;

    public void OnEnable()
    {
        UseEZServiceLocator();
        ObserveEvents();
    }

    public void ObserveEvents()
    {
        InputEvents.Do.Subscribe<ICancelPressed>(CancelPressed);
        CancelEvents.Do.Subscribe<ICancelActivated>(CancelOrBackButtonPressed);
    }

    public void UnObserveEvents() { }

    public void UseEZServiceLocator()
    {
       // _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _uiHistoryTrack = EZService.Locator.Get<IHistoryTrack>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    private void CancelPressed(ICancelPressed args)
    {
        if(ActiveResolvePopUps & !_myDataHub.GamePaused) return;
        
        _uiHistoryTrack.CancelHasBeenPressed(ProcessCancelType(args.EscapeKeySettings), null);
    }

    private void CancelOrBackButtonPressed(ICancelActivated args)
    {
        _uiHistoryTrack.CancelHasBeenPressed(ProcessCancelType(args.EscapeKeyType), args.BranchToCancel);
    }

    private EscapeKey ProcessCancelType(EscapeKey escapeKey)
    {
        return escapeKey == EscapeKey.GlobalSetting ? GlobalEscapeSetting : escapeKey;
    }
}
