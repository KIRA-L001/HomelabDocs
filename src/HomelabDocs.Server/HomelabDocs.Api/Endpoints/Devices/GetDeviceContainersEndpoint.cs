using FastEndpoints;
using HomelabDocs.Business.Devices;
using HomelabDocs.Shared.Containers;

namespace HomelabDocs.Api.Endpoints.Devices;

public sealed class GetDeviceContainersEndpoint
    : Endpoint<GetDeviceContainersRequest, GetRunningContainersResponse>
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

        // Add "in-progress" status to containers
        var containersWithStatus = containers.Select(c => 
        {
            // If the container doesn't have a status, set it to "in-progress"
            // If it already has a status, keep it as is
            return c with { Status = string.IsNullOrEmpty(c.Status) ? "in-progress" : c.Status };
        }).ToList();

        await Send.OkAsync(
            new GetRunningContainersResponse
            {
                Containers = containersWithStatus
            },
            ct);
    }
}
