
using System.Collections.Generic;
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

public interface ISetUpStartBranches
{
    List<IBranch> GroupsBranches { get; }
} 

public interface IClearScreen  { }

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



