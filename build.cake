//
//  build.cake
//
//  Copyright (c) 2019 SceneGate
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

// NUnit tests
#tool nuget:?package=NUnit.ConsoleRunner&version=3.9.0

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

string netVersion = "472";
string netBinDir = $"bin/{configuration}/net{netVersion}";

string netcoreVersion = "2.2";
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
