namespace contester.Features.Common.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException() : base("Entity not found") { }
    public EntityNotFoundException(Type entityType)
        : base($"Entity {entityType.Name} with the given Id could not be found") { }
    public EntityNotFoundException(Type entityType, Guid entityId)
        : base($"Entity {entityType.Name} with Id {entityId} could not be found") { }
    public EntityNotFoundException(Type entityType, Guid entityId1, Guid entityId2)
        : base($"Entity {entityType.Name} with Id ({entityId1},{entityId2}) could not be found") { }
    public EntityNotFoundException(Type entityType, int entityId)
        : base($"Entity {entityType.Name} with Id {entityId} could not be found") { }
}
