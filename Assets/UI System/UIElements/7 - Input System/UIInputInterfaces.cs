using EZ.Inject;
using UnityEngine;

public interface IInput : IParameters
{
    bool StartInGame();
}

public interface IVirtualCursorSettings : IInput
{
    Transform GetParentTransform { get; }
}
