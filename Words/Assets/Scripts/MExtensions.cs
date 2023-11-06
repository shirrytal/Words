using System;
using UnityEngine;


 namespace Ext
{
    public static class MExtensions
    {
        public static Color Normalize(this Color color)
        {
            if (color.r > 1)
                color.r /= 255.0f;
            if (color.g > 1)
                color.g /= 255.0f;
            if (color.b > 1)
                color.b /= 255.0f;
            return color;
        }
    }
}


