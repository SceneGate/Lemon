#load "nuget:?package=PleOps.Cake&version=0.3.1"

Task("Define-Project")
    .Description("Fill specific project information")
    .Does<BuildInfo>(info =>
{
    info.CoverageTarget = 90;
    info.AddLibraryProjects("Lemon");
    info.AddTestProjects("Lemon.IntegrationTests");

    // No need to set if you want to use nuget.org
    info.PreviewNuGetFeed = "https://pkgs.dev.azure.com/SceneGate/SceneGate/_packaging/SceneGate-Preview/nuget/v3/index.json";
});

Task("Default")
    .IsDependentOn("Stage-Artifacts");

string target = Argument("target", "Default");
RunTarget(target);
