using Newtonsoft.Json.Linq;
using VDocFx.Api;
using VDocFx.Api.Dotnet;

namespace Microsoft.Docs.Build.metadata;

internal class ApiMetadataProvider
{
    private readonly Config _config;
    private readonly Input _input;
    private readonly Output _output;
    private readonly ErrorBuilder _errors;

    public ApiMetadataProvider(Config config, Input input, Output output, ErrorBuilder errors)
    {
        _config = config;
        _input = input;
        _output = output;
        _errors = errors;
    }

    public void Build()
    {
        IApiBuilder? apiBuilder = null;
        JToken? apiConfig = null;
        if (_config.Dotnet != null)
        {
            apiBuilder = new DotnetApiBuilder();
            apiConfig = _config.Dotnet;
        }

        if (apiBuilder == null)
        {
            _errors.Add(new Error(ErrorLevel.Error, "config-load-error", $"No API builders were found!"));
            return;
        }

        var mainPath = _input.GetMainPath();
        apiBuilder.Build(apiConfig!, _errors, mainPath, mainPath);
    }
}
