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

    [Property]
    public Property ShortenId_is_stable_prefix_of_non_whitespace_id()
    {
        return Prop.ForAll(
            NonWhitespaceString().ToArbitrary(),
            static containerId =>
            {
                var shortened = ContainerMapper.ShortenId(containerId);
                var expected = containerId.Length <= ShortIdLength
                    ? containerId
                    : containerId[..ShortIdLength];

                return shortened == expected
                       && containerId.StartsWith(shortened, StringComparison.Ordinal);
            });
    }

    [Property]
    public Property ResolveName_uses_first_usable_name_without_leading_slashes()
    {
        Gen<NameResolutionSample> samples =
            from usableName in NonWhitespaceString()
            from prefixCount in Gen.Choose(0, 5)
            from suffixCount in Gen.Choose(0, 5)
            from suffix in Gen.ListOf(ArbMap.Default.GeneratorFor<string>(), suffixCount)
            select new NameResolutionSample(usableName, prefixCount, suffix);

        return Prop.ForAll(
            samples.ToArbitrary(),
            static sample =>
            {
