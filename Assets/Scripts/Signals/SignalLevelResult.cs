using Enums;

namespace Signals
{
    public class SignalLevelResult
    {
        public ELevelResultType LevelResultType { get; }

        public SignalLevelResult(ELevelResultType levelResultType)
        {
            LevelResultType = levelResultType;
        }
    }
}