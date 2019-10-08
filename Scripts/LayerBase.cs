using System;
using UnityEngine;
using Zenject;

namespace LayeredUI
{
    public abstract class LayerBase : MonoBehaviour, ILayer
    {
        protected LayersManager _manager;

        public virtual bool Enabled
        {
            get => gameObject.activeSelf;
            set
            {
                if (gameObject.activeSelf != value)
                    gameObject.SetActive(value);
            }
        }
        public virtual bool IsTopLayer => this == _manager.TopLayer;
        public virtual LayerVisibilityType Visibility => LayerVisibilityType.Default;
        public virtual LayerHidingType Hiding => LayerHidingType.Default;
        public virtual bool DestroyOnLoad => false;

        public event Action FloatedUp;

        public new RectTransform transform => base.transform as RectTransform;

        [Inject]
        private void Init(LayersManager manager)
        {
            _manager = manager;
        }

        public virtual void OnQuitClick()
        {
            if (IsTopLayer)
                _manager.Pop();
            else
                _manager.Pop(this);
        }

        public void FloatUp()
        {
            if (FloatedUp != null)
                FloatedUp();
            OnFloatUp();
        }

        protected virtual void OnFloatUp()
        {

        }

        public class Factory : PlaceholderFactory<string, ILayer> { }
    }
}