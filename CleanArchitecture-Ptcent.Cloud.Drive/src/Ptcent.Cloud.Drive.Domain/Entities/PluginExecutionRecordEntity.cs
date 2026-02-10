using Ptcent.Cloud.Drive.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Ptcent.Cloud.Drive.Domain.Entities
{
    public class PluginExecutionRecordEntity
    {
        [Key]
        public long Id { get; set; }

        public string ExecutionKey { get; set; } = null!;

        public PluginExecutionStatus Status { get; set; }

        public int RetryCount { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
