using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using Object = UnityEngine.Object;

namespace UIElements
{
    public interface IGOUISwitcher : IMonoEnable, IMonoStart, IMonoDisable, ISwitch { }
    
    public class GOUISwitcher : IGOUISwitcher, IEZEventUser, IServiceUser
    {
        //Properties & Getters / Setters
        private bool OneOrMorePlayers => _playerObjects.Count > 0;
        public bool HasOnlyOneMember => _playerObjects.Count == 1;


        //Variables
        private int _index = 0;
        private List<IGOUIModule> _playerObjects;
        private IHistoryTrack _historyTrack;
        private IDataHub _myDataHub;
        
        //Main
        public void OnEnable()
        {
            UseEZServiceLocator();
            ObserveEvents();
        }
        
        public void OnDisable() => UnObserveEvents();

        public void UseEZServiceLocator()
        {
            _historyTrack = EZService.Locator.Get<IHistoryTrack>(this);
            _myDataHub = EZService.Locator.Get<IDataHub>(this);
        }

        public void ObserveEvents()
        {
            GOUIEvents.Do.Subscribe<IStartGOUIBranch>(AddGOUIToSwitchList);
            BranchEvent.Do.Subscribe<ICloseBranch>(RemovePlayerObject);
        }

        public void UnObserveEvents()
        {
            GOUIEvents.Do.Unsubscribe<IStartGOUIBranch>(AddGOUIToSwitchList);
            BranchEvent.Do.Unsubscribe<ICloseBranch>(RemovePlayerObject);
        }

        public void OnStart() => FindActiveGameObjectsOnStart();

        private void FindActiveGameObjectsOnStart()
        {
            _playerObjects = new List<IGOUIModule>();
            var _allObjects = new List<IGOUIModule>(Object.FindObjectsOfType<GOUIModule>().ToList());

            foreach (var playerObject in _allObjects.Where(playerObject => playerObject.GOUIModuleCanBeUsed))
            {
                _playerObjects.Add(playerObject);
            }
        }

        public void DoSwitch(SwitchInputType switchInputType)
        {
            if(!OneOrMorePlayers) return;
            
            switch (switchInputType)
            {
                case SwitchInputType.Positive:
                    DoSwitchProcess(x => _index.PositiveIterate(x));
                    break;
                case SwitchInputType.Negative:
                    DoSwitchProcess(x => _index.NegativeIterate(x));
                    break;
                case SwitchInputType.Activate:
                    _playerObjects[_index].SwitchEnter();
                    break;
            }
        }

        private void DoSwitchProcess(Func<int, int> switchAction)
        {
            _playerObjects[_index].SwitchExit();
            _index = switchAction(_playerObjects.Count);
            _playerObjects[_index].SwitchEnter();
        }

        private void AddGOUIToSwitchList(IStartGOUIBranch args)
        {
            if(!_myDataHub.SceneStarted) return;
            
            if (!_playerObjects.Contains(args.ReturnGOUIModule))
            {
                _playerObjects.Add(args.ReturnGOUIModule);
            }
        }

        private void RemovePlayerObject(ICloseBranch args)
        {
            if (!_playerObjects.Contains(args.ReturnGOUIModule)) return;
            
            _index = 0;
            _playerObjects.Remove(args.ReturnGOUIModule);
            MoveToNextObjectOrBranch(args.TargetBranch);
        }

        private void MoveToNextObjectOrBranch(IBranch branchToClose)
        {
            if (_playerObjects.Count > 0)
            {
                if (_myDataHub.ActiveBranch == branchToClose || _myDataHub.ActiveBranch.MyParentBranch == branchToClose)
                {
                    _playerObjects[_index].SwitchEnter();
                }
                //TODO Check This functionality with Trunks and if needed. In fact double check this entire thing
                _historyTrack.CheckListsAndRemove(branchToClose);
            }
            else
            {
                _historyTrack.MoveToLastBranchInHistory();
            }
        }
    }
}