using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTest : MonoBehaviour
{
    [SerializeField] Color colorA;
    [SerializeField] Color colorB;
    [SerializeField] Color result;


    public void RunColor()
    {
        RGBCombine(colorA,colorB);

        // Mixing the colors with equal weight
        Color mixedColor = Mix(colorA, colorB, 0.5, 0.5);



    }
    [ContextMenu("Update RBG")]
    private static void RGBCombine(Color colorA, Color colorB)
    {

    }

    public static Color Mix(Color color1, Color color2, double weight1, double weight2)
    {
        int mixedRed = (int)Math.Round(color1.r * weight1 + color2.r * weight2);
        int mixedGreen = (int)Math.Round(color1.g * weight1 + color2.g * weight2);
        int mixedBlue = (int)Math.Round(color1.b * weight1 + color2.b * weight2);

        // Ensure RGB values stay within the valid range (0-255)
        mixedRed = Math.Max(0, Math.Min(255, mixedRed));
        mixedGreen = Math.Max(0, Math.Min(255, mixedGreen));
        mixedBlue = Math.Max(0, Math.Min(255, mixedBlue));

        return new Color(mixedRed, mixedGreen, mixedBlue);
    }

}
