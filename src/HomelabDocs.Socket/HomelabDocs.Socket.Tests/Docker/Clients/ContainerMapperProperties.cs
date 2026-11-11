```csharp
using Docker.DotNet.Models;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using HomelabDocs.Socket.Api.Docker.Clients;

namespace HomelabDocs.Socket.Tests.Docker.Clients;

public sealed class ContainerMapperProperties
{
    private const string ComposeProjectLabel = "com.docker.compose.project";
    private const int ShortIdLength = 12;

    [Property]
    public bool ShortenId_never_exceeds_short_id_length(string? containerId)
    {
        var shortened = ContainerMapper.ShortenId(containerId!);
        return shortened.Length <= ShortIdLength;
    }

    [Property]
    public Property ShortenId_whitespace_or_empty_returns_empty()
    {
        return Prop.ForAll(
            WhitespaceOrEmptyStringGen().ToArbitrary(),
            static input => ContainerMapper.ShortenId(input) == string.Empty);
    }