using Core.Abstracts;
using Services;
using UniRx;
using UniRx.Triggers;
using Views;
using Zenject;

namespace Controllers
{
    public class InputController : Controller<InputView>, IInitializable
    {
        private readonly IGameBoardService _gameBoardService;

        public InputController
        (
            IGameBoardService gameBoardService
        )
        {
            _gameBoardService = gameBoardService;
        }
        
        public void Initialize()
        {
            View.LeftButton.OnPointerDownAsObservable().Subscribe(_ => _gameBoardService.MoveLeft()).AddTo(View);
            View.RightButton.OnPointerDownAsObservable().Subscribe(_ => _gameBoardService.MoveRight()).AddTo(View);
            View.StartButton.OnPointerDownAsObservable().Subscribe(_ => _gameBoardService.Start()).AddTo(View);
        }
    }
}