using UnityEngine;

namespace UIElements
{
    public interface IRaycast
    {
        bool DoRaycast(Vector3 virtualCursorPos);
        void WhenInMenu();
        void ExitLastObject();
    }
}