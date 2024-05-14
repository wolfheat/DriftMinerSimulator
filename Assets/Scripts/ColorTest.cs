using System;
using UnityEngine;
using UnityEngine.InputSystem;

    [ExecuteInEditMode]
public class ColorTest : MonoBehaviour
{

    [Header("Color A")]
    [Range(0, 1f)]
    [SerializeField] float A_Red;
    [Range(0, 1f)]
    [SerializeField] float A_Green;
    [Range(0, 1f)]
    [SerializeField] float A_Blue;
    [SerializeField] Color colorA;

    [Header("Color B")]
    [Range(0, 1f)]
    [SerializeField] float B_Red;
    [Range(0, 1f)]
    [SerializeField] float B_Green;
    [Range(0, 1f)]
    [SerializeField] float B_Blue;
    [SerializeField] Color colorB;

    [Header("CMYK Color Combine")]
    [SerializeField] Color result;

    [ContextMenu("Update RBG")]

    private void Update()
    {
        RunUpdate();
    }

    private void OnGUI()
    {
        RunUpdate();
    }

    public void RunUpdate()
    {
        colorA = new Color(A_Red, A_Green, A_Blue);
        colorB = new Color(B_Red, B_Green, B_Blue);

        if (colorA == colorB)
        {
            result = colorA;
            return;
        }

        result = MixAsCMYK(colorA, colorB);      
    }

    private Color Invert(Color c) => new Color(1 - c.r, 1 - c.g, 1 - c.b, 1);

    private static void RGBCombine(Color colorA, Color colorB)
    {
            
    }

    public static Color CMYKToRGB(Color c)
    {
        float red   = (1 - c.r) * (1 - c.a);
        float green = (1 - c.g) * (1 - c.a);
        float blue  = (1 - c.b) * (1 - c.a);

        return new Color(red, green, blue, (1 - c.a));
    }
    public static Color RGBToCMYK(Color c)
    {
        float cyan = 1f - c.r;
        float magenta = 1f - c.g;
        float yellow = 1f - c.b;

        float key = Mathf.Min(cyan, magenta, yellow);

        // Ensure RGB values stay within the valid range (0-255)
        cyan = key == 1 ? 0 : (cyan - key) / (1 - key);
        magenta = key == 1 ? 0 : (magenta - key) / (1 - key);
        yellow = key == 1 ? 0 : (yellow - key) / (1 - key);

        return new Color(cyan, magenta, yellow, key);
    }
    
    public static Color CMYKMix(Color color1, Color color2)
    {
        float max = Mathf.Max(color1.r + color2.r, color1.g + color2.g, color1.b + color2.b);
        if (max == 0) return new Color(0, 0, 0);

        float cyan = Mathf.Max((color1.r + color2.r)/ max, 1f);
        float magenta = Mathf.Max((color1.g + color2.g)/ max, 1f);
        float yellow = Mathf.Max((color1.b + color2.b)/max, 1f);
        float black = Mathf.Max((color1.a + color2.a) / max, 1f);
        /*
        float cyan = Mathf.Max(color1.r + color2.r,1f);
        float magenta = Mathf.Max(color1.g + color2.g, 1f);
        float yellow = Mathf.Max(color1.b + color2.b, 1f);
        */
        return new Color(1-cyan, 1-magenta, 1-yellow,1-black);
    }

    private Color MixAsCMYK(Color colorA, Color colorB) => MixB(RGBToCMYK(colorA), RGBToCMYK(colorB));

    public static Color MixB(Color color1, Color color2)
    {
        float mixedRed = Mathf.Min(color1.r + color2.r,1);
        float mixedGreen = Mathf.Min(color1.g + color2.g,1);
        float mixedBlue = Mathf.Min(color1.b + color2.b, 1);
        float black = Mathf.Min(color1.a + color2.a, 1);

        return new Color(1 - mixedRed,1- mixedGreen, 1 - mixedBlue,1-black);
    }
    public static Color MixC(Color color1, Color color2)
    {
        float max = Mathf.Max(color1.r+ color2.r,color1.g+color2.g,color1.b+color2.b);
        float mixedRed = (color1.r + color2.r)/max;
        float mixedGreen = (color1.g + color2.g)/max;
        float mixedBlue = (color1.b + color2.b)/max;

        return new Color(1 - mixedRed,1- mixedGreen, 1 - mixedBlue);
    }
    public static Color Mix(Color color1, Color color2)
    {
        float mixedRed = color1.r + color2.r;
        float mixedGreen = color1.g + color2.g;
        float mixedBlue = color1.b + color2.b;

        // Ensure RGB values stay within the valid range (0-255)
        mixedRed = Math.Max(1, mixedRed);
        mixedGreen = Math.Max(1, mixedGreen);
        mixedBlue = Math.Max(1,mixedBlue);

        return new Color(1 - mixedRed,1- mixedGreen, 1 - mixedBlue);
    }

}
