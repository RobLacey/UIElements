using EZ.Events;
using EZ.Service;
using UIElements;

interface ICancel: IMonoEnable, IMonoDisable { }

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel : ICancel, IServiceUser, IEZEventUser
{
    //Variables
    private IHistoryTrack _uiHistoryTrack;
    private IDataHub _myDataHub;

    //Properties 7 Getters / Setters
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
    
    public void OnDisable() => UnObserveEvents();

    public void UnObserveEvents()
    {
        InputEvents.Do.Unsubscribe<ICancelPressed>(CancelPressed);
        CancelEvents.Do.Unsubscribe<ICancelActivated>(CancelOrBackButtonPressed);
    }

    public void UseEZServiceLocator()
    {
        _uiHistoryTrack = EZService.Locator.Get<IHistoryTrack>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    private void CancelPressed(ICancelPressed args) 
        => _uiHistoryTrack.CancelHasBeenPressed(ProcessCancelType(args.EscapeKeySettings), null);

    private void CancelOrBackButtonPressed(ICancelActivated args) 
        => _uiHistoryTrack.CancelHasBeenPressed(ProcessCancelType(args.EscapeKeyType), args.BranchToCancel);

    private EscapeKey ProcessCancelType(EscapeKey escapeKey) 
        => escapeKey == EscapeKey.GlobalSetting ? GlobalEscapeSetting : escapeKey;
}
