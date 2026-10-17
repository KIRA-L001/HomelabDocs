using System.Linq;
using FastEndpoints;
using HomelabDocs.Business.Devices;
using HomelabDocs.Shared.Containers;

namespace HomelabDocs.Api.Endpoints.Devices;

public sealed class GetDeviceContainersResponse
{
    public IEnumerable<ContainerDto> RunningContainers { get; set; } = default!;
    public IEnumerable<ContainerDto> InProgressContainers { get; set; } = default!;
}

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

        var running = containers.Where(c =>
            c.Status.Equals("running", StringComparison.OrdinalIgnoreCase));

        var inProgress = containers.Where(c =>
            c.Status.Equals("created", StringComparison.OrdinalIgnoreCase) ||
            c.Status.Equals("restarting", StringComparison.OrdinalIgnoreCase) ||
            c.Status.Equals("paused", StringComparison.OrdinalIgnoreCase) ||
            c.Status.Equals("removing", StringComparison.OrdinalIgnoreCase));

        await Send.OkAsync(new GetDeviceContainersResponse
        {
            RunningContainers = running,
            InProgressContainers = inProgress
        }, ct);
    }
}
