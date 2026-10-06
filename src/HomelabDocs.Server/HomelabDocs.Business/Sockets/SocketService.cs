using HomelabDocs.Domain.Sockets;
using HomelabDocs.Shared.Sockets;
using SocketEntity = HomelabDocs.Domain.Sockets.Socket;

namespace HomelabDocs.Business.Sockets;

public sealed class SocketService : ISocketService
{
    private readonly ISocketRepository _socketRepository;

    public SocketService(ISocketRepository socketRepository)
    {
        _socketRepository = socketRepository;
    }

    public async Task<IReadOnlyList<SocketResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var sockets = await _socketRepository.ListAsync(cancellationToken);
        return sockets.Select(Map).ToArray();
    }

    public async Task<SocketResponse?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var socket = await _socketRepository.GetByIdAsync(id, cancellationToken);
        return socket is null ? null : Map(socket);
    }

    public async Task<SocketMutationResult> CreateAsync(
        string name,
        string address,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(name, address);
        if (validationError is not null)
        {
            return SocketMutationResult.Invalid(validationError);
        }

        var trimmedName = name.Trim();
        var trimmedAddress = address.Trim();

        if (await _socketRepository.NameExistsAsync(trimmedName, excludeId: null, cancellationToken))
        {
            return SocketMutationResult.Conflict(
                $"A socket named '{trimmedName}' already exists.");
        }

        var socket = new SocketEntity
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Address = trimmedAddress,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _socketRepository.AddAsync(socket, cancellationToken);
        return SocketMutationResult.Success(Map(socket));
    }

    public async Task<SocketMutationResult> UpdateAsync(
        Guid id,
        string name,
        string address,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(name, address);
        if (validationError is not null)
        {
            return SocketMutationResult.Invalid(validationError);
        }

        var trimmedName = name.Trim();
        var trimmedAddress = address.Trim();

        var existing = await _socketRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return SocketMutationResult.NotFound();
        }

        if (await _socketRepository.NameExistsAsync(trimmedName, excludeId: id, cancellationToken))
        {
            return SocketMutationResult.Conflict(
                $"A socket named '{trimmedName}' already exists.");
        }

        existing.Name = trimmedName;
        existing.Address = trimmedAddress;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await _socketRepository.UpdateAsync(existing, cancellationToken);
        return SocketMutationResult.Success(Map(existing));
    }

    public async Task<SocketMutationResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var socket = await _socketRepository.GetByIdAsync(id, cancellationToken);
        if (socket is null)
        {
            return SocketMutationResult.NotFound();
        }

        await _socketRepository.RemoveAsync(socket, cancellationToken);
        return SocketMutationResult.Success();
    }

    private static string? Validate(string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Name is required.";
        if (string.IsNullOrWhiteSpace(address))
            return "Address is required.";
        return null;
    }

    private static SocketResponse Map(SocketEntity socket)
    {
        return new SocketResponse
        {
            Id = socket.Id,
            Name = socket.Name,
            Address = socket.Address,
            CreatedAt = socket.CreatedAt,
            Version = socket.Version,
            UpdateNotice = socket.UpdateNotice
        };
    }
}
