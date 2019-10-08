using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace LayeredUI
{
    public class LayersManager : MonoBehaviour
    {
        private LayerBase.Factory _layerFactory;

        private Dictionary<string, ILayer> _instances = new Dictionary<string, ILayer>();
        private List<ILayer> _stack = new List<ILayer>();

        [SerializeField]
        private bool _isStatic = false;
        [SerializeField]
        private Canvas _uiCanvas;
        [SerializeField]
        private RectTransform _container;
        [SerializeField]
        private CanvasGroup _fader;
        [SerializeField]
        private bool _showLayersInGui = false;
        [SerializeField]
        private bool _dontDestroyOnLoad = true;

        [Inject]
        private void Init(LayerBase.Factory layerFactory)
        {
            _layerFactory = layerFactory;

            if (_fader != null)
                _fader.alpha = 1f;
            if (_dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);
            if (_uiCanvas == null) _uiCanvas = GetComponent<Canvas>();
            if (_container == null) _container = _uiCanvas.transform as RectTransform;
        }

        #region Fading

        public void FadeIn(float fadeTime, Action afterFade = null)
        {
            StartCoroutine(FadeRoutine(1f, 0f, fadeTime, afterFade));
        }

        public void FadeOut(float fadeTime, Action afterFade = null)
        {
            StartCoroutine(FadeRoutine(0f, 1f, fadeTime, afterFade));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration, Action finalAction)
        {
            float time = Time.realtimeSinceStartup;
            if (_fader != null)
            {
                _fader.blocksRaycasts = true;
                _fader.alpha = from;
            }
            while (Time.realtimeSinceStartup < time + duration)
            {
                _fader.alpha = Mathf.Lerp(from, to, (Time.realtimeSinceStartup - time) / duration);
                yield return new WaitForEndOfFrame();
            }
            if (_fader != null)
            {
                _fader.alpha = to;
                _fader.blocksRaycasts = to != 0f;
            }
            finalAction?.Invoke();
        }

        #endregion

        #region Waiting


        public void Wait(float fadeTime, Action afterWait = null)
        {
            StartCoroutine(WaitRoutine(fadeTime, afterWait));
        }

        private IEnumerator WaitRoutine(float duration, Action finalAction)
        {
            float time = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < time + duration)
                yield return new WaitForEndOfFrame();
            finalAction?.Invoke();
        }

        #endregion

        public TLayer GetLayer<TLayer>() where TLayer : ILayer
        {
            var layerName = typeof(TLayer).Name;
            ILayer layer;
            if (_instances.TryGetValue(layerName, out layer))
                return (TLayer)layer;
            else
                return InstantiateFromPrefab<TLayer>();
        }

        public TLayer InstantiateFromPrefab<TLayer>() where TLayer : ILayer
        {
            var layerName = typeof(TLayer).Name;
            var instance = (TLayer)_layerFactory.Create(layerName);
            if (!instance.DestroyOnLoad)
                DontDestroyOnLoad(instance.gameObject);
            instance.name = layerName;
            var rt = instance.transform;
            rt.SetParent(_container);
            rt.localScale = Vector3.one;
            instance.transform.position = Vector3.zero;
            rt.anchoredPosition = Vector3.zero;
            rt.sizeDelta = Vector3.zero;

            instance.Enabled = false;
            if (_instances.ContainsKey(layerName))
                _instances.Remove(layerName);
            _instances.Add(layerName, instance);
            return instance;
        }

        public bool HaveLayer<TLayer>()
        {
            var layerName = typeof(TLayer).Name;
            return _instances.ContainsKey(layerName);
        }

        public void DestroyLayer(ILayer layer)
        {
            var layerName = layer.GetType().Name;
            if (_instances.ContainsKey(layerName))
            {
                Pop(layer);
                _instances.Remove(layerName);
            }
            GameObject.Destroy(layer.gameObject);
        }

        public ILayer TopLayer { get { if (_stack.Count > 0) return _stack.Last(); else return null; } }

        private void Push(ILayer layer, bool withSwitch)
        {
            Pop(layer);
            _stack.Add(layer);
            layer.transform.SetAsLastSibling();
            if (withSwitch)
                SwitchLayersActivity();
        }

        public void Push(ILayer layer)
        {
            Push(layer, true);
        }

        public TLayer Push<TLayer>() where TLayer : ILayer
        {
            var layer = GetLayer<TLayer>();
            Push(layer);
            return layer;
        }

        private ILayer Pop(bool withSwitch)
        {
            var count = _stack.Count;
            ILayer result = null;
            if (count > 1)
            {
                result = _stack[count - 1];
                result.Enabled = false;
                _stack.RemoveAt(count - 1);
            }
            if (withSwitch)
                SwitchLayersActivity();
            return result;
        }

        public ILayer Pop()
        {
            return Pop(true);
        }

        public void Hide(bool val)
        {
            gameObject.SetActive(!val);
        }

        public void Pop(ILayer layer)
        {
            if (_stack != null && _stack.Contains(layer))
            {
                layer.Enabled = false;
                _stack.Remove(layer);
                SwitchLayersActivity();
            }
        }

        public void Pop<TLayer>() where TLayer : ILayer
        {
            ILayer layerForRemove = null;
            if (_stack == null) return;
            foreach (var layer in _stack)
            {
                if (layer.GetType() == typeof(TLayer))
                {
                    layerForRemove = layer;
                    break;
                }
            }
            if (layerForRemove != null)
                Pop(layerForRemove);
        }

        public void PopTill(ILayer layer, bool destroy = false, bool include = false)
        {
            while (_stack.Count > 1)
            {
                var pLayer = Pop(false);
                if (pLayer != null)
                {
                    if (pLayer == layer)
                    {
                        if (!include)
                            Push(pLayer, false);
                        else
                            DestroyLayer(pLayer);
                        break;
                    }
                    else
                        DestroyLayer(pLayer);
                }
            }
            SwitchLayersActivity();
        }

        public void PopTill<TLayer>(bool destroy = false, bool include = false) where TLayer : ILayer
        {
            while (_stack.Count > 1)
            {
                var layer = Pop(false);
                if (layer != null)
                {
                    if (layer.GetType() == typeof(TLayer))
                    {
                        if (!include)
                            Push(layer, false);
                        else
                            DestroyLayer(layer);
                        break;
                    }
                    else
                        DestroyLayer(layer);
                }
            }
            SwitchLayersActivity();
        }

        public void Top(ILayer layer)
        {
            if (_stack.Contains(layer))
            {
                _stack.Remove(layer);
                Push(layer);
            }
        }

        public void Top<TLayer>() where TLayer : ILayer
        {
            ILayer layerForTop = null;
            foreach (var layer in _stack)
            {
                if (layer.GetType() == typeof(TLayer))
                {
                    layerForTop = layer;
                    break;
                }
            }
            if (layerForTop != null)
                Top(layerForTop);
        }


        public bool Contains<TLayer>() where TLayer : ILayer
        {
            foreach (var layer in _stack)
                if (layer.GetType() == typeof(TLayer))
                    return true;
            return false;
        }

        private void SwitchLayersActivity()
        {
            int hiding = -1;

            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                var layer = _stack[i];
                if (layer.IsTopLayer)
                    layer.Enabled = true;
                else
                    layer.Enabled = (int)layer.Visibility > hiding;
                hiding = Math.Max(hiding, (int)layer.Hiding);
            }
            if (TopLayer != null)
            {
                TopLayer.FloatUp();
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (_showLayersInGui)
                for (int i = 0; i < _stack.Count; i++)
                {
                    GUI.Label(new Rect(Screen.width - 150, Screen.height - 25 * (_stack.Count - i), 150, 25), _stack[i].GetType().Name);
                }
        }
#endif
    }
}