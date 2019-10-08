using System;
using UnityEngine;

namespace LayeredUI
{
    public interface ILayer
    {
        bool Enabled { get; set; }
        bool IsTopLayer { get; }
        LayerVisibilityType Visibility { get; }
        LayerHidingType Hiding { get; }
        bool DestroyOnLoad { get; }

        event Action FloatedUp;
        void FloatUp();

        GameObject gameObject { get; }
        string name { get; set; }
        RectTransform transform { get; }
    }
}
