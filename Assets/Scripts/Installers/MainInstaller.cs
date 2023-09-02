using Services.Impls;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class MainInstaller : MonoInstaller
    {
        [SerializeField] private Transform _gameBoardParent;
        
        public override void InstallBindings()
        {
            BindServices();
        }

        private void BindServices()
        {
            Container.BindInstance(_gameBoardParent).WhenInjectedInto<GameBoardService>();
            Container.BindInterfacesTo<GameBoardService>().AsSingle();
        }
    }
}