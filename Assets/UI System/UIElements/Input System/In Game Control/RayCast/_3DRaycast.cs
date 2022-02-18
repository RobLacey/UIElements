﻿using UnityEngine;

namespace UIElements
{
    public interface I3DRaycast : IRaycast { }

    public class _3DRaycast : RaycastBase
    {
        public override void OnStart()
        {
            base.OnStart();
            _laserLength = _inputScheme.ReturnVirtualCursorSettings.RaycastLength;
        }

        private float _laserLength;

        protected override ICursorHandler RaycastToObj(Vector3 virtualCursorPos)
        {
            var mousePos = virtualCursorPos;
            mousePos.z = -10;
            var cursorPosition = _mainCamera.ScreenToWorldPoint(mousePos);
            var direction = (CameraPosition - cursorPosition).normalized;
            var hit = Physics.RaycastAll(CameraPosition, direction, _laserLength, _layerToHit);
            return hit.Length == 0  ? null : hit[0].collider.gameObject.GetComponent<ICursorHandler>();;
        }
    }
}