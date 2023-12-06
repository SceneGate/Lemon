# Lemon [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/)

_Lemon_ is a library part of the [_SceneGate_](https://github.com/SceneGate)
framework that provides support for **3DS file formats.**

## Supported formats

_Encryption, decryption or signature validation not supported yet._

- **NCSD (CCI and CSU)**: unpack
- **CIA**: unpack and pack
- **NCCH (CXI and CFA)**: unpack and pack
- **ExeFS**: unpack and pack
- **RomFS**: unpack and pack
- **TMD**: deserialize

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

## References

- [3D Brew](https://www.3dbrew.org/wiki/Main_Page)
