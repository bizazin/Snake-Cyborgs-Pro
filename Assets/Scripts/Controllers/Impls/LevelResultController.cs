using Core.Abstracts;
using Databases;
using Enums;
using Views;
using Zenject;

namespace Controllers.Impls
{
    public class LevelResultController : Controller<LevelResultView>, IInitializable, ILevelResultController
    {
        private readonly ILevelResultDatabase _levelResultDatabase;


        public LevelResultController        
        (
            ILevelResultDatabase levelResultDatabase
        )
        {
            _levelResultDatabase = levelResultDatabase;
        }
        
        public void Initialize()
        {
        }

        public void SetLevelResult(ELevelResultType levelResultType)
        {
            View.HeaderText.text = _levelResultDatabase.GetLevelResult(levelResultType);
        }
    }
}