using System.Collections.Generic;
using EZ.Service;
using UnityEngine.EventSystems;

namespace UIElements
{

    public interface IInteractWithUi : IMonoEnable
    {
        bool OverNothingUI();
        void ClearLastHit();
        void CheckIfCursorOverUI(IVirtualCursor virtualCursor);
    }

    public class VcInteractionUI : IInteractWithUi, IServiceUser
    {
        //Variables
        private VirtualCursorSettings _settings;
        private UINode _lastHit;
        private IDataHub _myDataHub;
        readonly PointerEventData _pointerEventData = new PointerEventData(EventSystem.current) { };
        private bool _overNothingUI;
        
        //Properties
        private bool OnlyHitInGameObjects => _settings.OnlyHitInGameUi == IsActive.Yes;

        //Main
        public void OnEnable() => UseEZServiceLocator();

        public void UseEZServiceLocator()
        {
            _myDataHub = EZService.Locator.Get<IDataHub>(this);
            _settings = EZService.Locator.Get<InputScheme>(this).ReturnVirtualCursorSettings;
        }

        public void ClearLastHit() => _lastHit = null;

        public void CheckIfCursorOverUI(IVirtualCursor virtualCursor)
        {
            if(!_myDataHub.SceneStarted || OnlyHitInGameObjects) return;
            
            _pointerEventData.position = virtualCursor.CursorRect.position;
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_pointerEventData, raycastResults);

            if (raycastResults.Count > 0)
            {
                foreach (var result in raycastResults)
                {
                    var foundNode = (UINode)result.gameObject.GetComponentInParent<INode>();
                    
                    if (CheckIfOverNothing(foundNode)) return;

                    if (_lastHit == foundNode) return;

                    _lastHit = foundNode;
                    foundNode.OnPointerEnter(null);
                    return;
                }

            }
            RemoveHit();
        }

        private bool CheckIfOverNothing(UINode overNode)
        {
            if (overNode.IsNull())
            {
                _overNothingUI = true;
                if (_lastHit.IsNotNull())
                    RemoveHit();
                return true;
            }
            _overNothingUI = false;
            return false;
        }

        private void RemoveHit()
        {
            if (_lastHit == null) return;
            
            _lastHit.OnPointerExit(null);
            ClearLastHit();
        }
        
        public bool OverNothingUI()
        {
            if (!_overNothingUI) return false;

            _overNothingUI = false;
            return true;
        }
    }
}
    
