﻿using System;
using EZ.Service;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIElements
{
    
    public interface ICanvasOrderData
    {
        int ReturnPresetCanvasOrder(ICanvasOrderCalculator canvasOrderCalculator);
        int ReturnToolTipCanvasOrder();
        int ReturnVirtualCursorCanvasOrder();
        int ReturnOffScreenMarkerCanvasOrder();
        int ReturnControlBarCanvasOrder();
    }

    [Serializable]
    public class CanvasOrderData : ICanvasOrderData, IMonoEnable, IIsAService
    {
        [InfoBox("Adjust settings to needs but try and organise from high to low in this order")]
        [SerializeField] private int _virtualCursor = 30;
        [SerializeField] private int _toolTipBase = 25;
        [SerializeField] private int _pauseMenu = 20;
        [SerializeField] private int _resolvePopUp = 20;
        [SerializeField] private int _optionalPopUp = 15;
        [SerializeField] private int _timedPopUp = 15;
        [SerializeField] private int _controlBar = 12;
        [SerializeField] private int _offScreenMarker = 10;
        [SerializeField] private int _inGameObject = -3;

        //Properties & Getters / Setters
        public int ReturnToolTipCanvasOrder() => _toolTipBase;
        public int ReturnVirtualCursorCanvasOrder() => _virtualCursor;
        public int ReturnOffScreenMarkerCanvasOrder() => _offScreenMarker;
        public int ReturnControlBarCanvasOrder() => _controlBar;
        
        //Main
        public void OnEnable() => AddService();

        public void AddService() => EZService.Locator.AddNew<ICanvasOrderData>(this);

        public void OnRemoveService() { }

        public int ReturnPresetCanvasOrder(ICanvasOrderCalculator calculator)
        {
            switch (calculator.GetBranchType)
            {
                case BranchType.ResolvePopUp:
                    return _resolvePopUp;
                case BranchType.OptionalPopUp:
                    return _optionalPopUp;
                case BranchType.TimedPopUp:
                    return _timedPopUp;
                case BranchType.PauseMenu:
                    return _pauseMenu;
                case BranchType.Standard:
                    return SetStandardCanvasOrder(calculator);
                case BranchType.InternalObsolete:
                    return SetStandardCanvasOrder(calculator);
                case BranchType.InGameObject:
                    return _inGameObject;
                case BranchType.ControlBar:
                    return SetStandardCanvasOrder(calculator);
                default:
                    return 0;
            }
        }

        public int SetStandardCanvasOrder(ICanvasOrderCalculator calculator)
        {
            switch (calculator.GetOrderInCanvas)
            {
                case OrderInCanvas.InFront:
                    return 2;
                case OrderInCanvas.Behind:
                    return -1;
                case OrderInCanvas.Manual:
                    return calculator.GetManualCanvasOrder;
                case OrderInCanvas.Default:
                    break;
            }

            return 0;
        }
    }
}