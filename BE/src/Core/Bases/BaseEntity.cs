using MassTransit;

namespace HuyHieuDang.Core.Bases;

public abstract class BaseEntity : BaseEntity<Guid>, IGuidIdentify
{
    protected BaseEntity() => Id = NewId.Next().ToGuid();
}

public abstract class BaseEntity<TId> : IEntity
{
    public TId Id { get; set; } = default!;

    public virtual DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public interface IIdentify<T>
{
    public T Id { get; set; }
}

public interface IGuidIdentify : IIdentify<Guid>
{
}