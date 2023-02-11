# VDocFx

Voltstro's fork of DocFx (v3).

## Getting Started

- Install .NET 7
- Install latest `vdocfx` release:
```
dotnet tool update -g vdocfx --version "1.0.0-*" --add-source https://pkgs.dev.azure.com/Voltstro/VDocFx/_packaging/VDocFx/nuget/v3/index.json
```

## Why

In November of 2022, Microsoft stopped developing v3 publicly, and started more heavily focusing on v2 for the public to use. That would normally be fine, but all of our docs already used v3, and [VoltProjects](https://github.com/Voltstro/VoltProjects) was designed around using v3. So we forked it and created VDocFx.

There are three major down-sides to docfx v3.

- Lack of tight integration with API doc generation
- Lots of Microsoft Learn septic code
- Usage of libraries that are not open-source, that come from Microsoft's Azure DevOps feed.

This fork fixes them (mostly, v2's API doc generation is arguably still better!).

## License

Licensed under the [MIT](LICENSE) License.
