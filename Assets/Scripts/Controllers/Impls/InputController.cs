using Core.Abstracts;
using Databases;
using Enums;
using Services;
using Services.Impls;
using UniRx;
using UniRx.Triggers;
using Views;
using Zenject;

namespace Controllers.Impls
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
            View.LeftButton.OnPointerDownAsObservable().Subscribe(_ => _gameBoardService.RotateSnake(ERotationSide.Left)).AddTo(View);
            View.RightButton.OnPointerDownAsObservable().Subscribe(_ => _gameBoardService.RotateSnake(ERotationSide.Right)).AddTo(View);
            View.StartButton.OnPointerDownAsObservable().Subscribe(_ => _gameBoardService.Start()).AddTo(View);
        }
    }
}