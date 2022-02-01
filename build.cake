#addin nuget:?package=Cake.Docker&version=1.1.0
#tool "dotnet:?package=GitVersion.Tool&version=5.8.1"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionName = "UserManagement.sln";
var version = string.Empty;
var dockerImageName = "usermanagementapi";
var dockerRegistry = Argument("dockerregistry", "cakebuildregistry.azurecr.io");
///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
   .IsDependentOn("Version")
   .IsDependentOn("Restore")
   .IsDependentOn("Build")
   .IsDependentOn("Test")
   .IsDependentOn("Publish")
   .IsDependentOn("Docker build")
;

Task("Version")
   .Does(() => {
      version = GitVersion().SemVer;

      Information(version);
   });

Task("Restore")
   .Does(() => {
      DotNetRestore(solutionName);
   });

Task("Build")
   .Does(() => {
      DotNetBuild(".", new DotNetBuildSettings{
         Configuration = configuration

      });
   });

Task("Test")
   .Does(() => {
      DotNetTest(".", new DotNetTestSettings{
         Configuration = configuration,
         NoBuild= true
      });
   });

Task("Publish")
   .Does(() => {
      DotNetPublish(solutionName, new DotNetPublishSettings{
         Configuration = configuration,
         OutputDirectory = "build/publish",
      });
   });

Task("Docker build")
   .Does(() => {

      var settings = new DockerImageBuildSettings{
            Tag = new string[]{$"{dockerRegistry}/{dockerImageName}:{version}"},
            Rm = true
        };

      DockerBuild(settings, ".");
   });

RunTarget(target);