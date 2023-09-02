using Controllers;
using Controllers.Impls;
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
        [SerializeField] private LevelResultView _levelResultView;
        
        [Header("Databases")]
        [SerializeField] private GameBoardSettingsDatabase _gameBoardSettingsDatabase;
        [SerializeField] private SnakeSettingsDatabase _snakeSettingsDatabase;
        [SerializeField] private LevelResultDatabase _levelResultDatabase;

        
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
            Container.BindView<LevelResultController, LevelResultView>(_levelResultView, parent);
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
            Container.Bind<ISnakeSettingsDatabase>().FromInstance(_snakeSettingsDatabase).AsSingle();
            Container.Bind<ILevelResultDatabase>().FromInstance(_levelResultDatabase).AsSingle();
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