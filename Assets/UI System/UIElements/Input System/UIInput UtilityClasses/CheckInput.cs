﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

public static class CheckInput
{
    public static bool Pressed(string input)
    {
        if (input == string.Empty) return false;

        return Input.GetButtonDown(input);
    }

    public static bool Held(string input)
    {
        if (input == string.Empty) return false;

        return Input.GetButton(input);
    }

    public static float GetAxis(string input)
    {
        if (input != string.Empty)
        {
            return  Input.GetAxis(input);
        }
        return 0;
    }
    
    public static int GetAxisRaw(string input)
    {
        if (input != string.Empty)
        {
            return Mathf.RoundToInt(Input.GetAxisRaw(input));
        }
        return 0;
    }

    public static bool MouseButton(int button)
    {
        return Input.GetMouseButtonDown(button);
    }
    
    public static AxisEventData MenuNavCalc(int upDown, int leftRight)
    {
        AxisEventData eventData = new AxisEventData(EventSystem.current);
        
        if(upDown != 0)
        {
            eventData.moveDir = upDown == 1 ? MoveDirection.Up : MoveDirection.Down;
            return eventData;
        }

        eventData.moveDir = leftRight == 1 ? MoveDirection.Right : MoveDirection.Left;
        return eventData;

    }

}