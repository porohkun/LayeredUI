using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace LayeredUI
{
    public class LayerFactory : IFactory<string, ILayer>
    {
        readonly DiContainer _container;

        public LayerFactory(DiContainer container)
        {
            _container = container;
        }

        public ILayer Create(string layerName)
        {
            var prefab = Resources.Load($"Layers/{layerName}");
            return _container.InstantiatePrefabForComponent<ILayer>(prefab);
        }
    }
}
