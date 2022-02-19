using System;
using UnityEngine;

public static class CheckInput
{
    public static bool Pressed(string input)
    {
        return input != string.Empty && Input.GetButtonDown(input);
    }

    public static bool Held(string input)
    {
        return input != string.Empty && Input.GetButton(input);
    }

    public static float GetAxis(string input)
    {
        if (input != string.Empty)
        {
            return  Input.GetAxis(input);
        }
        return 0;
    }

    public static bool MouseButton(int button)
    {
        return Input.GetMouseButtonDown(button);
    }
}