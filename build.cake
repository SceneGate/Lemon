// Copyright (c) 2019 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// NUnit tests
#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var tests = Argument("tests", string.Empty);
var warningsAsError = Argument("warnaserror", true);
var warnAsErrorOption = warningsAsError
    ? MSBuildTreatAllWarningsAs.Error
    : MSBuildTreatAllWarningsAs.Default;

string solutionPath = "src/Lemon.sln";

string netstandardVersion = "2.0";
string netstandardBinDir = $"bin/{configuration}/netstandard{netstandardVersion}";

string netVersion = "48";
string netBinDir = $"bin/{configuration}/net{netVersion}";

string netcoreVersion = "3.1";
string netcoreBinDir = $"bin/{configuration}/netcoreapp{netcoreVersion}";

Task("Clean")
    .Does(() =>
{
    DotNetCoreClean(solutionPath, new DotNetCoreCleanSettings {
        Configuration = "Debug",
        Verbosity = DotNetCoreVerbosity.Minimal,
    });
    DotNetCoreClean(solutionPath, new DotNetCoreCleanSettings {
        Configuration = "Release",
        Verbosity = DotNetCoreVerbosity.Minimal,
    });
});

Task("Build")
    .Does(() =>
{
    DotNetCoreBuild(solutionPath, new DotNetCoreBuildSettings {
        Configuration = configuration,
        Verbosity = DotNetCoreVerbosity.Minimal,
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .TreatAllWarningsAs(warnAsErrorOption),
    });
});

Task("Run-IntegrationTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    // NUnit3 to test libraries with .NET Framework / Mono
    var settings = new NUnit3Settings();
    if (tests != string.Empty) {
        settings.Test = tests;
    }

    var testAssemblies = new List<FilePath> {
        $"src/Lemon.IntegrationTests/{netBinDir}/Lemon.IntegrationTests.dll"
    };
    NUnit3(testAssemblies, settings);

    // .NET Core test library
    var netcoreSettings = new DotNetCoreTestSettings {
        NoBuild = true,
        Framework = $"netcoreapp{netcoreVersion}"
    };

    if (tests != string.Empty) {
        netcoreSettings.Filter = $"FullyQualifiedName~{tests}";
    }

    DotNetCoreTest(
        $"src/Lemon.IntegrationTests/Lemon.IntegrationTests.csproj",
        netcoreSettings);
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Run-IntegrationTests");

Task("Travis")
    .IsDependentOn("Default");

RunTarget(target);


public void ReportWarning(string msg)
{
    if (warningsAsError) {
        throw new Exception(msg);
    } else {
        Warning(msg);
    }
}
