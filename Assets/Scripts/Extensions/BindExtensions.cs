using Core.Interfaces;
using UnityEngine;
using Zenject;

namespace Extensions
{
    public static class BindExtensions
    {
        public static void BindView<T, TU>(this DiContainer container, Object viewPrefab, Transform parent)
            where TU : IView
            where T : IController
        {
            container.BindInterfacesAndSelfTo<T>().AsSingle();
            var bindInterfacesAndSelfTo = container.BindInterfacesAndSelfTo<TU>();
            var fromComponentInNewPrefab = bindInterfacesAndSelfTo
                .FromComponentInNewPrefab(viewPrefab);
            var underTransform = fromComponentInNewPrefab
                .UnderTransform(parent);
            underTransform.AsSingle();
        }
    }
}