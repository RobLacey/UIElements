using EZ.Events;

public class CancelBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //Cancel
        AutoRemove().CreateEvent<ICancelActivated>();
       // AutoRemove().CreateEvent<ICancelHoverOver>();
    }
}

public interface ICancelActivated
{
    EscapeKey EscapeKeyType { get; }
    IBranch BranchToCancel { get; }
} 

// public interface ICancelHoverOver
// {
//     EscapeKey EscapeKeyType { get; }
// }


