
using System;

[Obsolete("MovedToToggleGroup", true)]
public interface  IToggleNotLinked : INodeBase { }

[Obsolete("MovedToToggleGroup", true)]
public class ToggleNotLinked : NodeBase, IToggleNotLinked
{
    public ToggleNotLinked(INode node) : base(node)
    {
        _dontAddToHistoryTracking = true;
    }
}
