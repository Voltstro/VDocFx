namespace VDocFx.Api.Dotnet;

internal class DotnetApiConfig
{
    public string Dest { get; init; } = "api";

    public string[] Assemblies { get; init; } = Array.Empty<string>();
}
