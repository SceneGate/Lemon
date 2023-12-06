# Lemon

<!-- markdownlint-disable MD033 -->
<p align="center">
  <a href="https://www.nuget.org/packages/SceneGate.Lemon">
    <img alt="Stable version" src="https://img.shields.io/nuget/v/SceneGate.Lemon?label=nuget.org&logo=nuget" />
  </a>
  &nbsp;
  <a href="https://dev.azure.com/SceneGate/SceneGate/_packaging?_a=feed&feed=SceneGate-Preview">
    <img alt="GitHub commits since latest release (by SemVer)" src="https://img.shields.io/github/commits-since/SceneGate/Lemon/latest?sort=semver" />
  </a>
  &nbsp;
  <a href="https://github.com/SceneGate/Lemon/workflows/Build%20and%20release">
    <img alt="Build and release" src="https://github.com/SceneGate/Lemon/workflows/Build%20and%20release/badge.svg" />
  </a>
  &nbsp;
  <a href="https://choosealicense.com/licenses/mit/">
    <img alt="MIT License" src="https://img.shields.io/badge/license-MIT-blue.svg?style=flat" />
  </a>
  &nbsp;
</p>

_Lemon_ is a library part of the [_SceneGate_](https://github.com/SceneGate)
framework that provides support for **3DS file formats.**

## Supported formats

_Encryption, decryption or signature validation not supported yet._

- **NCSD (CCI and CSU)**: unpack
- **CIA**: unpack and pack
- **NCCH (CXI and CFA)**: unpack and pack
- **ExeFS**: unpack and pack
- **RomFS**: unpack and pack
- **TMD**: deserialize and serialize

## Usage

The project provides the following .NET libraries (NuGet packages in nuget.org).
The libraries works on supported versions of .NET: 6.0 and 8.0.

- [![SceneGate.Lemon](https://img.shields.io/nuget/v/SceneGate.Lemon?label=SceneGate.Lemon&logo=nuget)](https://www.nuget.org/packages/SceneGate.Lemon):
  support for 3DS formats

Preview releases can be found in this
[Azure DevOps package repository](https://dev.azure.com/SceneGate/SceneGate/_packaging?_a=feed&feed=SceneGate-Preview).
To use a preview release, create a file `nuget.config` in the same directory of
your solution file (.sln) with the following content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear/>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="SceneGate-Preview" value="https://pkgs.dev.azure.com/SceneGate/SceneGate/_packaging/SceneGate-Preview/nuget/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="SceneGate-Preview">
      <package pattern="Yarhl*" />
      <package pattern="Texim*" />
      <package pattern="SceneGate*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

## Documentation

Documentation is not yet available but it will be published in the
[project website](https://scenegate.github.io/Lemon).

Don't hesitate to ask questions in the
[project Discussion site!](https://github.com/SceneGate/Ekona/discussions)

## Build

The project requires to build .NET 8.0 SDK.

To build, test and generate artifacts run:

```sh
# Build and run tests
dotnet run --project build/orchestrator

# (Optional) Create bundles (nuget, zips, docs)
dotnet run --project build/orchestrator -- --target=Bundle
```

Some test binary resources are pushed via [Git LFS](https://git-lfs.com/). Make
sure to clone these files as well, otherwise the tests would fail. On Linux you
may need to install it and re-pull, for instance for Ubuntu run:

```sh
sudo apt install git-lfs
git lfs pull
```

To build the documentation only, run:

```sh
dotnet docfx docs/docfx.json --serve
```

## References

- [3D Brew](https://www.3dbrew.org/wiki/Main_Page)
