using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ptcent.Cloud.Drive.Application.Events;
using Ptcent.Cloud.Drive.Application.Plugins;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Domain.Event;

namespace Ptcent.Cloud.Drive.Infrastructure.Persistence.Interceptors
{
    public sealed class EntityChangeInterceptor : SaveChangesInterceptor
    {
        private readonly IMediator _mediator;
        private static readonly AsyncLocal<PluginContext> _ctx = new();

        // 字段过滤器
        private readonly Dictionary<string, string[]> _entityFilteredAttributes;

        public EntityChangeInterceptor(IMediator mediator, Dictionary<string, string[]>? entityFilteredAttributes = null)
        {
            _mediator = mediator;
            _entityFilteredAttributes = entityFilteredAttributes ?? new Dictionary<string, string[]>();
        }

        private static PluginContext GetContext()
        {
            if (_ctx.Value == null)
                _ctx.Value = new PluginContext();
            else
                _ctx.Value.IncreaseDepth();

            return _ctx.Value;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return result;

            var pluginContext = GetContext();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                    continue;

                var filteredAttrs = _entityFilteredAttributes.ContainsKey(entry.Entity.GetType().Name)
                    ? _entityFilteredAttributes[entry.Entity.GetType().Name]
                    : null;

                var evt = BuildEvent(entry, filteredAttrs);

                await _mediator.Publish(new EntityChangedNotification(evt, PluginStage.PreValidation, pluginContext), cancellationToken);
                await _mediator.Publish(new EntityChangedNotification(evt, PluginStage.PreOperation, pluginContext), cancellationToken);
            }

            return result;
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return result;

            var pluginContext = GetContext();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                    continue;

                var filteredAttrs = _entityFilteredAttributes.ContainsKey(entry.Entity.GetType().Name)
                    ? _entityFilteredAttributes[entry.Entity.GetType().Name]
                    : null;

                var evt = BuildEvent(entry, filteredAttrs);


                await _mediator.Publish(new EntityChangedNotification(evt, PluginStage.PostOperation, pluginContext), cancellationToken);
            }

            return result;
        }

        private static EntityChangedEvent BuildEvent(EntityEntry entry, string[]? filteredAttributes = null)
        {
            var changes = new Dictionary<string, (object, object)>();

            foreach (var p in entry.Properties)
            {
                bool include = false;

                if (entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                {
                    include = true; // 新增和删除都收集所有字段
                }
                else if (entry.State == EntityState.Modified)
                {
                    // 字段变化或者 OriginalValue 不可用（兼容缓存对象）
                    if (!Equals(p.OriginalValue, p.CurrentValue) || p.OriginalValue == null)
                        include = true;
                }

                if (include)
                {
                    // 过滤字段
                    if (filteredAttributes == null || filteredAttributes.Contains(p.Metadata.Name))
                    {
                        changes[p.Metadata.Name] = (p.OriginalValue, p.CurrentValue);
                    }
                }
            }

            // 获取主键
            var primaryKeyProps = entry.Metadata.FindPrimaryKey()?.Properties;
            if (primaryKeyProps == null || primaryKeyProps.Count == 0)
                throw new InvalidOperationException($"Entity {entry.Entity.GetType().Name} does not have a primary key.");

            var primaryKeyValue = primaryKeyProps.Count == 1
                ? entry.Property(primaryKeyProps[0].Name).CurrentValue
                : string.Join(",", primaryKeyProps.Select(p => entry.Property(p.Name).CurrentValue));

            return new EntityChangedEvent(
                entry.Entity.GetType().Name,
                primaryKeyValue!,
                entry.State switch
                {
                    EntityState.Added => ChangeType.Create,
                    EntityState.Modified => ChangeType.Update,
                    EntityState.Deleted => ChangeType.Delete,
                    _ => throw new NotSupportedException()
                },
                changes
            );
        }
    }

}
