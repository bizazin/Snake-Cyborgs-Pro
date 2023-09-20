using Enums;

namespace Databases
{
    public interface ILevelResultDatabase
    {
        string GetLevelResult(ELevelResultType levelResultTypeType);
    }
}