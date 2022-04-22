using EZ.Events;
using UIElements;

public class InputBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //Input
        AutoRemove().CreateEvent<IPausePressed>();
        AutoRemove().CreateEvent<ICancelPause>();
        AutoRemove().CreateEvent<IMenuGameSwitchingPressed>();
        AutoRemove().CreateEvent<IStandardCancel>();

        //ChangeControl
        AutoRemove().CreateEvent<IAllowKeys>();
        AutoRemove().CreateEvent<IChangeControlsPressed>();
   }
}

public interface IPausePressed
{
    bool ClearScreen { get; }
} // This one is test

public interface ICancelPause { }

public interface IMenuGameSwitchingPressed { }

public interface IAllowKeys // This one is test
{
    bool CanAllowKeys { get; }
}

public interface IChangeControlsPressed { }

public interface IStandardCancel // This one is test
{
    EscapeKey EscapeKeySettings { get; }
}





