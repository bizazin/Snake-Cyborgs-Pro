using System;
using Core.Abstracts;
using Databases;
using Enums;
using Services;
using Signals;
using UniRx;
using UniRx.Triggers;
using Views;
using Zenject;

namespace Controllers
{
    public class LevelResultController : Controller<LevelResultView>, IInitializable, IDisposable
    {
        private readonly ILevelResultDatabase _levelResultDatabase;
        private readonly IGameBoardService _gameBoardService;
        private readonly SignalBus _signalBus;
        private readonly CompositeDisposable _disposable = new();

        public LevelResultController        
        (
            ILevelResultDatabase levelResultDatabase,
            IGameBoardService gameBoardService,
            SignalBus signalBus
        )
        {
            _levelResultDatabase = levelResultDatabase;
            _gameBoardService = gameBoardService;
            _signalBus = signalBus;
        }
        
        public void Initialize()
        {
            _signalBus.GetStream<SignalLevelResult>().Subscribe(s => SetLevelResult(s.LevelResultType))
                .AddTo(_disposable);
            View.SetRestartActive(false);

            View.StartButton.OnPointerDownAsObservable().Subscribe(_ =>
            {
                _gameBoardService.Start();
                View.SetRestartActive(true);
                View.gameObject.SetActive(false);
            }).AddTo(View);
            View.RestartButton.OnPointerDownAsObservable().Subscribe(_ =>
            {
                View.gameObject.SetActive(false);
                _gameBoardService.Restart();
            }).AddTo(View);
        }

        public void Dispose() => _disposable?.Dispose();

        private void SetLevelResult(ELevelResultType levelResultType)
        {
            View.gameObject.SetActive(true);
            View.HeaderText.text = _levelResultDatabase.GetLevelResult(levelResultType);
        }
    }
}