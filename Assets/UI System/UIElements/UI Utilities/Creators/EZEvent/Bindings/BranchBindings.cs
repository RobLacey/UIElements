
using EZ.Events;
using UIElements;
using UnityEngine;

public class BranchBindings : EZEventBindingsBase
{
    protected override void EventsToBind()
    {
        //Branch
        AutoRemove().CreateEvent<IClearScreen>();
        AutoRemove().CreateEvent<ISetUpStartBranches>();
        AutoRemove().CreateEvent<IEndTween>();
        AutoRemove().CreateEvent<ICloseBranch>();
    }
}

public interface ISetUpStartBranches  { } 

public interface IClearScreen 
{
    IBranch IgnoreThisBranch { get; }
}

public interface IEndTween
{
    RectTransform EndTweenRect { get; }
    TweenScheme Scheme { get; }
}

public interface ICloseBranch
{
    IBranch TargetBranch { get; }
    GOUIModule ReturnGOUIModule { get; }
}



