
using System;
using EZ.Events;
using EZ.Service;
using UIElements;

public interface IMenuAndGameSwitching : IEZEventUser, IMonoEnable, IMonoStart, IMonoDisable { }


public class MenuAndGameSwitching : IMenuAndGameSwitching, IInMenu, IEZEventDispatcher, IServiceUser
{
    //Variables
    private bool _wasInGame;
    private InputScheme _inputScheme;
    private IDataHub _myDataHub;

    //Events
    private Action<IInMenu> IsInMenu { get; set; }

    //Properties
    public bool InTheMenu { get; set; } = true;
    private InMenuOrGame StartWhere { get; set; }

    private void SaveNoPopUps(INoPopUps args)
    {
        //TODO Make Sure this all works still as withe the whole class
        if (!InTheMenu && !_myDataHub.NoPopups) _wasInGame = true;
         PopUpEventHandler();
    }

    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        ObserveEvents();
    }
    
    public void UseEZServiceLocator()
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    public void FetchEvents() => IsInMenu = HistoryEvents.Do.Fetch<IInMenu>();

    public void ObserveEvents()
    {
        InputEvents.Do.Subscribe<IMenuGameSwitchingPressed>(CheckForActivation);
        HistoryEvents.Do.Subscribe<IGameIsPaused>(WhenTheGameIsPaused);
        HistoryEvents.Do.Subscribe<IOnStart>(StartUp);
        PopUpEvents.Do.Subscribe<INoPopUps>(SaveNoPopUps);
    }

    public void OnDisable() => UnObserveEvents();

    public void UnObserveEvents()
    {
        InputEvents.Do.Unsubscribe<IMenuGameSwitchingPressed>(CheckForActivation);
        HistoryEvents.Do.Unsubscribe<IGameIsPaused>(WhenTheGameIsPaused);
        HistoryEvents.Do.Unsubscribe<IOnStart>(StartUp);
        PopUpEvents.Do.Unsubscribe<INoPopUps>(SaveNoPopUps);
    }

    public void OnStart()
    {
        if (_inputScheme.InGameMenuSystem == InGameSystem.On)
            StartWhere = _inputScheme.WhereToStartGame;
    }

    private void CheckForActivation(IMenuGameSwitchingPressed arg)
    {
        if (!_myDataHub.NoPopups) return;
        SwitchBetweenGameAndMenu();
    }
    
    private void PopUpEventHandler()
    {
        if (!_myDataHub.NoPopups && !InTheMenu)
        {
            SwitchBetweenGameAndMenu();
        }
        
        if (_myDataHub.NoPopups && _wasInGame)
        {
            _wasInGame = false;
            SwitchBetweenGameAndMenu();
        }
    }

    private void StartUp(IOnStart onStart)
    {
        InTheMenu = true;
        if (StartWhere == InMenuOrGame.InGameControl)
        {
            SwitchBetweenGameAndMenu();
        }
        else
        {
            BroadcastState();
        }
    }

    private void SwitchBetweenGameAndMenu()
    {
        if (InTheMenu)
        {
            SwitchToGame();
        }
        else
        {
            SwitchToMenu();
        }
    }


    private void SwitchToGame()
    {
        InTheMenu = false;
        BroadcastState();
    }

    private void SwitchToMenu()
    {
        InTheMenu = true;
        BroadcastState();
    }

    private void BroadcastState() => IsInMenu?.Invoke(this);

    private void WhenTheGameIsPaused(IGameIsPaused args)
    {
        if (args.IsPaused && !InTheMenu)
        {
            SwitchBetweenGameAndMenu();
            _wasInGame = true;
            return;
        }

        if (!args.IsPaused && _wasInGame)
        {
            SwitchBetweenGameAndMenu();
            _wasInGame = false;
        }
    }
}
