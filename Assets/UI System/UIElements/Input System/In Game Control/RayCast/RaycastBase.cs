using EZ.Service;
using UnityEngine;

namespace UIElements
{
    public abstract class RaycastBase : I2DRaycast, I3DRaycast
    {
        protected RaycastBase(IVcSettings settings) => _settings = settings.VCSettings;

        //Variables
        private ICursorHandler _lastGameObject;
        protected readonly Camera _mainCamera = Camera.main;
        private readonly VirtualCursorSettings _settings;
        
        //Properties
        protected Vector3 CameraPosition => _mainCamera.transform.position;
        protected float LaserLength => _settings.RaycastLength;
        protected LayerMask LayerToHit => _settings.LayerToHit;

        //Main
        public void WhenInMenu()
        {
            if(_lastGameObject.IsNull()) return;
            _lastGameObject = null;
        }

        public bool DoRaycast(Vector3 virtualCursorPos)
        {
            OverGameObj(RaycastToObj(virtualCursorPos));
            return _lastGameObject.IsNotNull();
        }

        protected abstract ICursorHandler RaycastToObj(Vector3 virtualCursorPos);

        private void OverGameObj(ICursorHandler hit)
        {
            if (IfNoGOUIHit(hit)) return;
            
            if (_lastGameObject.IsNotNull())
            {
                if (_lastGameObject == hit) return;
                _lastGameObject.VirtualCursorExit();
            }

            _lastGameObject = hit;
            hit.VirtualCursorEnter();
        }

        private bool IfNoGOUIHit(ICursorHandler hit)
        {
            if (!hit.IsNull()) return false;
            ExitLastObject();
            return true;
        }

        public void ExitLastObject()
        {
            if (_lastGameObject.IsNull()) return;
            
            _lastGameObject.VirtualCursorExit();
            _lastGameObject = null;
        }
    }
}