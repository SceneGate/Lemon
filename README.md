# Lemon [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/) ![Build and release](https://github.com/SceneGate/Lemon/workflows/Build%20and%20release/badge.svg)

[Yarhl](https://github.com/SceneGate/yarhl) plugin for Nintendo 3DS common
formats.

The library supports .NET 6.0 and above on Linux, Window and MacOS.

<!-- prettier-ignore -->
| Release | Package                                                           |
| ------- | ----------------------------------------------------------------- |
| Stable  | [![Nuget](https://img.shields.io/nuget/v/SceneGate.Lemon?label=nuget.org&logo=nuget)](https://www.nuget.org/packages/SceneGate.Lemon) |
| Preview | [Azure Artifacts](https://dev.azure.com/SceneGate/SceneGate/_packaging?_a=feed&feed=SceneGate-Preview) |

## Supported formats

_Encryption, decryption or signature validation not supported yet._

- **NCSD (CCI and CSU)**: unpack
- **CIA**: unpack and pack
- **NCCH (CXI and CFA)**: unpack and pack
- **ExeFS**: unpack and pack
- **RomFS**: unpack and pack
- **TMD**: deserialize

## Documentation

Feel free to ask any question in the
[project Discussion site!](https://github.com/SceneGate/Lemon/discussions)

Check our on-line [API documentation](https://scenegate.github.io/Lemon/).

## Build

The project requires to build .NET 6.0 SDK and .NET Framework 4.8 or latest
Mono. If you open the project with VS Code and you did install the
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
