using Controllers;
using Databases;
using Databases.Impls;
using Extensions;
using ObjectPooling.Objects;
using ObjectPooling.Pools;
using ObjectPooling.Pools.Impls;
using UnityEngine;
using UnityEngine.UI;
using Views;
using Zenject;

namespace Installers
{
    [CreateAssetMenu(menuName = "Installers/MainPrefabInstaller", fileName = "MainPrefabInstaller")]
    public class MainPrefabInstaller : ScriptableObjectInstaller
    {
        [Header("Canvas")] 
        [SerializeField] private Canvas _canvas;
        
        [Header("Ui Views")]
        [SerializeField] private InputView _inputView;
        
        [Header("Databases")]
        [SerializeField] private GameBoardSettingsDatabase _gameBoardSettingsDatabase;

        
        [Header("Behaviours")] 
        [SerializeField] private Cell _cell;
        [SerializeField] private SnakeSegment _snakeSegment;
        [SerializeField] private Obstacle _obstacle;
        [SerializeField] private Fruit _fruit;
        
        public override void InstallBindings()
        {
            BindUiViews();
            BindObjectPools();
            BindDatabases();
        }

        private void BindUiViews()
        {
            Container.Bind<CanvasScaler>().FromComponentInNewPrefab(_canvas).AsSingle();
            var parent = Container.Resolve<CanvasScaler>().transform;
            
            Container.BindView<InputController, InputView>(_inputView, parent);
        }

        private void BindObjectPools()
        {
            BindPool<Cell, CellPool, ICellPool>(_cell, 484);
            BindPool<SnakeSegment, SnakeSegmentPool, ISnakeSegmentPool>(_snakeSegment, 10);
            BindPool<Obstacle, ObstaclePool, IObstaclePool>(_obstacle, 89);
            BindPool<Fruit, FruitPool, IFruitPool>(_fruit, 8);
        }

        private void BindDatabases()
        {            
            Container.Bind<IGameBoardSettingsDatabase>().FromInstance(_gameBoardSettingsDatabase).AsSingle();
        }

        private void BindPool<TItemContract, TPoolConcrete, TPoolContract>(TItemContract prefab, int size)
            where TItemContract : MonoBehaviour
            where TPoolConcrete : TPoolContract, IMemoryPool
            where TPoolContract : IMemoryPool
        {
            var poolContainerName = "[Pool] " + prefab;
            Container.BindMemoryPoolCustomInterface<TItemContract, TPoolConcrete, TPoolContract>()
                .WithInitialSize(size)
                .FromComponentInNewPrefab(prefab)
#if UNITY_EDITOR
                .UnderTransformGroup(poolContainerName)
#endif
                .AsCached()
                .OnInstantiated((_, item) => (item as MonoBehaviour)?.gameObject.SetActive(false));
        }
    }
}