# Lemon [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/)

[Yarhl](https://github.com/SceneGate/yarhl) plugin for Nintendo 3DS common
formats.

TODO BADGE

<!-- prettier-ignore -->
| Release | Package                                                           |
| ------- | ----------------------------------------------------------------- |
| Stable  | [![Nuget](https://img.shields.io/nuget/v/PleOps.Cake?label=nuget.org&logo=nuget)](https://www.nuget.org/packages/PleOps.Cake) |
| Preview | [Azure Artifacts](https://dev.azure.com/SceneGate/SceneGate/_packaging?_a=feed&feed=SceneGate-Preview) |

## Supported formats

_Encryption, decryption or signature validation not supported yet._

- **NCSD (CCI and CSU)**: unpack
- **CIA**: unpack
- **NCCH (CXI and CFA)**: unpack
- **ExeFS**: unpack and pack
- **RomFS**: unpack and pack
- **TMD**: deserialize

## Documentation

Feel free to ask any question in the
[project Discussion site!](https://github.com/SceneGate/Lemon/discussions)

Check our on-line [API documentation](https://scenegate.github.io/Lemon/).

## Install

Stable releases are available from nuget.org:

- Soon available.

The library targets and tests in the .NET 5.0 runtime.

Preview releases can be found in this
[Azure DevOps package repository](https://dev.azure.com/SceneGate/SceneGate/_packaging?_a=feed&feed=SceneGate-Preview).
To use a preview release, create a file `nuget.config` in the same directory of
your solution (.sln) file with the following content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="SceneGate-Preview" value="https://pkgs.dev.azure.com/SceneGate/SceneGate/_packaging/SceneGate-Preview/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

## Build

The project requires to build .NET 5.0 SDK, .NET Core 3.1 runtime and .NET
Framework 4.8 or latest Mono. If you open the project with VS Code and you did
install the
[VS Code Remote Containers](https://code.visualstudio.com/docs/remote/containers)
extension, you can have an already pre-configured development environment with
Docker or Podman.

To build, test and generate artifacts run:

```sh
# Only required the first time
dotnet tool restore

# Default target is Stage-Artifacts
dotnet cake
```

To just build and test quickly, run:

```sh
dotnet cake --target=BuildTest
```

## References

- [3D Brew](https://www.3dbrew.org/wiki/Main_Page)
