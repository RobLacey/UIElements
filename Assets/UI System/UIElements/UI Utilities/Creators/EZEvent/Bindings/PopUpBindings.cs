using EZ.Events;

public class PopUpBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //PopUps
        AutoRemove().CreateEvent<IAddOptionalPopUp>();
        AutoRemove().CreateEvent<IAddResolvePopUp>();
        //AutoRemove().CreateEvent<INoPopUps>();
        AutoRemove().CreateEvent<IRemoveResolvePopUp>();
        AutoRemove().CreateEvent<IRemoveOptionalPopUp>();
    }
}

//public interface INoPopUps { }

public interface IAddOptionalPopUp // This one is test
{
    IBranch ThisPopUp { get; }
}

public interface IAddResolvePopUp // This one is test
{
    IBranch ThisPopUp { get; }
}

public interface IRemoveOptionalPopUp // This one is test
{
    IBranch ThisPopUp { get; }
}

public interface IRemoveResolvePopUp // This one is test
{
    IBranch ThisPopUp { get; }
}
