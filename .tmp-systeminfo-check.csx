using XRayne.Infrastructure.Services;

var service = new WindowsSystemInfoService();
var cpu = await service.GetCpuInfoAsync();
foreach (var core in cpu.Cores)
{
    Console.WriteLine($"{core.Index}: {core.UsagePercent?.ToString() ?? "null"}");
}
