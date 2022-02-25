using EZ.Events;

public class CancelBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //Cancel
        AutoRemove().CreateEvent<ICancelButtonActivated>();
        AutoRemove().CreateEvent<ICancelHoverOver>();
    }
}

public interface ICancelButtonActivated
{
    EscapeKey EscapeKeyType { get; }
} 

public interface ICancelHoverOver
{
    EscapeKey EscapeKeyType { get; }
}


