using Services.Impls;
using Signals;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class MainInstaller : MonoInstaller
    {
        [SerializeField] private Transform _gameBoardParent;
        
        public override void InstallBindings()
        {
            BindSignals();
            BindServices();
        }

        private void BindSignals()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<SignalLevelResult>();
        }

        private void BindServices()
        {
            Container.BindInterfacesTo<FruitService>().AsSingle();
            Container.BindInterfacesTo<SnakeService>().AsSingle();
            
            Container.BindInstance(_gameBoardParent).WhenInjectedInto<GameBoardService>();
            Container.BindInterfacesTo<GameBoardService>().AsSingle();
        }
    }
}