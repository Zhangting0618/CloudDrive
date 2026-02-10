using MediatR;
using Ptcent.Cloud.Drive.Application.Events;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Attributes;
using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.PluginHandlers
{
    /// <summary>
    /// 只在特定字段 Update 时触发
    /// </summary>
    [PluginTrigger(ChangeType = EntityChangeType.Update, Attributes = new[] { "LeafName", "Extension" })]
    [PluginExecution(PluginExecutionMode.Async)]
    [PluginPriority(10)]
    public class FileBankChangedPlugin
        : INotificationHandler<EntityChangedNotification>
    {
        public async Task Handle(EntityChangedNotification n, CancellationToken ct)
        {
            if (n.Stage != PluginStage.PostOperation)
                return;

            if (n.Event.EntityName != "FileEntity")
                return;

            Console.WriteLine($"🔥 Async Plugin Start {DateTime.Now:HH:mm:ss.fff}");

            // ⏱ 模拟超慢操作（比如发消息 / 调外部系统）
            await Task.Delay(5000, ct);

            Console.WriteLine($"✅ Async Plugin End   {DateTime.Now:HH:mm:ss.fff}");
        }
    }

}
