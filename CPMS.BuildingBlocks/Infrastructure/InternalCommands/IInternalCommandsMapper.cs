namespace CPMS.BuildingBlocks.Infrastructure.InternalCommands;

public interface IInternalCommandsMapper
{
    string GetName(Type type);

    Type GetType(string name);
}