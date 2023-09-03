using Core.Abstracts;
using Enums;
using Services;
using UniRx;
using UniRx.Triggers;
using Views;
using Zenject;

namespace Controllers
{
    public class InputController : Controller<InputView>, IInitializable
    {
        private readonly ISnakeService _snakeService;

        public InputController
        (
            ISnakeService snakeService
        )
        {
            _snakeService = snakeService;
        }
        
        public void Initialize()
        {
            View.LeftButton.OnPointerDownAsObservable().Subscribe(_ => _snakeService.RotateSnake(ERotationSide.Left)).AddTo(View);
            View.RightButton.OnPointerDownAsObservable().Subscribe(_ => _snakeService.RotateSnake(ERotationSide.Right)).AddTo(View);
        }
    }
}