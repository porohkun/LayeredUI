# Layered UI #

## Using example: ##

### LayersManager prefab ###

- GameObject with
  Canvas
  LayersManager
  ZenjectBinding
  - Child GameObject 'Container'
  - Child GameObject 'Fader' with
    Image
    CavasGroup

### Any zenject's MonoInstaller ###

```csharp
    public class SomeInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindFactory<string, ILayer, LayerBase.Factory>().FromFactory<LayerFactory>();
        }
    }
```

### Every UI layer ###

Simply inherit your layer script from LayerBase and create prefab with it and put it to Assets/Resources/Layers/{YourLayerScriptName}.prefab

### Using ###

```csharp
    public class StartLayer : LayerBase
    {
        private void Start()
        {
            _manager.Push<LoadingLayer>();

            _manager.FadeIn(0.3f, () =>
                _manager.Wait(0.6f, () =>
                    _manager.FadeOut(0.3f, () =>
                    {
                        _manager.Push<GameScreenLayer>();
                        _manager.FadeIn(0.3f);
                    })));
        }
    }
```