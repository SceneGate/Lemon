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

// Gendarme: decompress zip
#addin nuget:?package=Cake.Compression&loaddependencies=true&version=0.2.1

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var warningsAsError = Argument("warnaserror", false);

string netstandardVersion = "2.0";
string netstandardBinDir = $"bin/{configuration}/netstandard{netstandardVersion}";

Task("Clean")
    .Does(() =>
{
    MSBuild("src/Lemon.sln", configurator => configurator
        .WithTarget("Clean")
        .SetVerbosity(Verbosity.Minimal)
        .SetConfiguration(configuration));
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var msbuildConfig = new MSBuildSettings {
        Verbosity = Verbosity.Minimal,
        Configuration = configuration,
        Restore = true,
        MaxCpuCount = 0,  // Auto build parallel mode
        WarningsAsError = warningsAsError,
    };
    MSBuild("src/Lemon.sln", msbuildConfig);
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);


public void ReportWarning(string msg)
{
    if (warningsAsError) {
        throw new Exception(msg);
    } else {
        Warning(msg);
    }
}
