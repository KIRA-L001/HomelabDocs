using FastEndpoints;
using HomelabDocs.Business.Devices;
using HomelabDocs.Shared.Containers;
using System.Linq;

namespace HomelabDocs.Api.Endpoints.Devices;

public sealed class GetDeviceContainersEndpoint
    : Endpoint<GetDeviceContainersRequest, GetDeviceContainersResponse>
{
    private readonly IDeviceQueryService _deviceQueryService;

    public GetDeviceContainersEndpoint(IDeviceQueryService deviceQueryService)
    {
        _deviceQueryService = deviceQueryService;
    }

    public override void Configure()
    {
        Get("/api/devices/{Name}/containers");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetDeviceContainersRequest req,
        CancellationToken ct)
    {
        var containers = await _deviceQueryService.GetContainersAsync(req.Name, ct);
        if (containers is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var running = containers.Where(c => c.Status == ContainerStatus.Running).ToList();
        var inProgress = containers.Where(c => c.Status == ContainerStatus.InProgress).ToList();

        await Send.OkAsync(
            new GetDeviceContainersResponse
            {
                RunningContainers = running,
                InProgressContainers = inProgress
            },
            ct);
    }
}

public class GetDeviceContainersResponse
{
    public List<ContainerDto> RunningContainers { get; set; } = new();
    public List<ContainerDto> InProgressContainers { get; set; } = new();
}
