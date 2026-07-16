using Infrastructure.Services;
using Infrastructure.States;
using Infrastructure.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Test.Infrastructure;

public sealed class GeoResourceOperationJobTests
{
    [Fact]
    public async Task Execute_allows_missing_optional_file_keys_for_url_refresh()
    {
        var service = Substitute.For<INodeGeoResourceService>();
        var logger = Substitute.For<ILogger<GeoResourceOperationJob>>();
        var job = new GeoResourceOperationJob(service, logger);
        var context = Substitute.For<IJobExecutionContext>();
        context.MergedJobDataMap.Returns(new JobDataMap
        {
            [GeoResourceOperationJob.GeoResourceIdKey] = 42L,
            [GeoResourceOperationJob.OperationKey] = GeoResourceOperation.Refresh.ToString()
        });
        context.CancellationToken.Returns(CancellationToken.None);

        await job.Execute(context);

        await service.Received(1).ExecuteQueuedOperationAsync(
            42L,
            GeoResourceOperation.Refresh,
            null,
            null,
            Arg.Any<CancellationToken>());
    }
}
