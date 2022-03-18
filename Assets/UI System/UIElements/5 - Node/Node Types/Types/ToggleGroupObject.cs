using System.Collections.Generic;
using EZ.Service;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ToggleGroup", menuName = "UIElements Schemes / New Toggle Group", order = 0)]
    public class ToggleGroupObject : ScriptableObject, IServiceUser
    {
        [SerializeField] private IsActive _overrideTrunkSwitcher = IsActive.No;
        [SerializeField] private IsActive _activeToggleOnSwitch = IsActive.No;

        //Variables
         private ISwitchTrunkGroup _parentTrunkSwitcher;
        private bool _started;
        private IDataHub _myDataHub;
        private List<Toggle> toggleGroup = new List<Toggle>();

        //Properties
        private ToggleSwitcher ToggleSwitcher { get; set; }
        private bool CanUseToggleSwitcher => _overrideTrunkSwitcher == IsActive.Yes;
        public INode StartingNode { get; private set; }
        public bool ActivateToggleOnSwitch => _activeToggleOnSwitch == IsActive.Yes;
        
        //Main
        public void OnAwake()
        {
            if(_started) return;

            _started = true;
            toggleGroup = new List<Toggle>();
            StartingNode = null;
            
            if (CanUseToggleSwitcher)
            {
                ToggleSwitcher = new ToggleSwitcher(this);
                UseEZServiceLocator();
            }
        }

        public void AddToGroupAndIsStartingPoint(Toggle toggle)
        {
            toggleGroup.Add(toggle);
            CheckForStartPosition(toggle);
        }
        
        public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

        public void OnStart()
        {
            if (!CanUseToggleSwitcher) return;
            
            ToggleSwitcher.SetSwitchGroup(toggleGroup);
       }

        private void OnDisable() => _started = false;

        private void CheckForStartPosition(Toggle toggle)
        {
            if (toggle.StartAsSelected || StartingNode.IsNull())
            {
                StartingNode = toggle.MyNode;
            }
        }
        
        /// <summary>
        /// Method can be used to activate a Tab switcher when moving onto it in a Trunk that isn't only made up of Tabs or
        /// has multiple tab groups
        /// </summary>
        public void ActivateTabSwitcher()
        {
            if(CanUseToggleSwitcher && _myDataHub.CurrentSwitcher != ToggleSwitcher)
            {
                _myDataHub.CurrentSwitcher = ToggleSwitcher;
            }
        }

        public void SetNewSwitcherIndex(Toggle toggle)
        {
            if(CanUseToggleSwitcher)
                ToggleSwitcher.SetNewIndex(toggle);
        }
        
        public void TurnOffOtherTogglesInGroup(Toggle activeToggle)
        {
            foreach (var toggleGroupMember in toggleGroup)
            {
                if(toggleGroupMember == activeToggle) continue;
                toggleGroupMember.SetNodeAsNotSelected_NoEffects();
            }
        }

    }
