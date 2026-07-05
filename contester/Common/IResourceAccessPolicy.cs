namespace contester.Common;

public interface IResourceAccessPolicy<in TId, in TResource>
{
    Task<bool> CanReadAsync(Guid userId, TId resourceId, CancellationToken ct);
    Task<bool> CanReadAsync(Guid userId, TResource resource, CancellationToken ct);
    Task<bool> CanModifyAsync(Guid userId, TId resourceId, CancellationToken ct);
    Task<bool> CanModifyAsync(Guid userId, TResource resource, CancellationToken ct);
}
