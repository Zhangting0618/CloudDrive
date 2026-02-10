using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Web.Options;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    public static class SnowIdExtensions
    {
        public static IServiceCollection AddSnowId(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection("SnowId");

            var options = new SnowIdOptions
            {
                Method = section.GetValue<short>("Method"),
                BaseTime = DateTime.Parse(section["BaseTime"]!),
                WorkerId = section.GetValue<ushort>("WorkerId"),
                WorkerIdBitLength = (byte)section.GetValue<int>("WorkerIdBitLength"),
                SeqBitLength = (byte)section.GetValue<int>("SeqBitLength"),
                MaxSeqNumber = section.GetValue<int>("MaxSeqNumber"),
                MinSeqNumber = section.GetValue<ushort>("MinSeqNumber"),
                TopOverCostCount = section.GetValue<int>("TopOverCostCount"),
                DataCenterId = section.GetValue<ushort>("DataCenterId"),
                DataCenterIdBitLength = (byte)section.GetValue<ushort>("DataCenterIdBitLength"),
                TimestampType = (byte)section.GetValue<ushort>("TimestampType")
            };

            services.AddIdGenerator(options);

            return services;
        }
    }

}
