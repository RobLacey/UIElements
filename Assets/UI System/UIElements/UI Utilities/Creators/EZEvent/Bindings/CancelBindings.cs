using EZ.Events;

public class CancelBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //Cancel
        AutoRemove().CreateEvent<ICancelButtonPressed>();
       // AutoRemove().CreateEvent<ICancelHoverOver>();
    }
}

public interface ICancelButtonPressed
{
    EscapeKey EscapeKeyType { get; }
    IBranch BranchToCancel { get; }
} 

// public interface ICancelHoverOver
// {
//     EscapeKey EscapeKeyType { get; }
// }


