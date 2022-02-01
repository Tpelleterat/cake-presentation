#tool "dotnet:?package=GitVersion.Tool&version=5.8.1"
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.8.0
#addin nuget:?package=Cake.Sonar&version=1.1.29
#addin nuget:?package=Cake.Docker&version=1.1.0

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS / VARIABLES
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var sonarLogin = Argument("sonarlogin", string.Empty);
var sonarPassword = EnvironmentVariable("SONAR_PASSWORD");
var disableSonar = HasArgument("disableSonar");
var solutionName = "UserManagement.sln";
var version = string.Empty;
var dockerImageName = "usermanagementapi";
var dockerRegistry = Argument("dockerregistry", "cakebuildregistry.azurecr.io");

///////////////////////////////////////////////////////////////////////////////
// METHODS
///////////////////////////////////////////////////////////////////////////////

private string BuildImageTag(){
   return $"{dockerRegistry}/{dockerImageName}:{version}";
}

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

Task("Version")
   .Does(() => {
      version = GitVersion().SemVer;

      Information(version);
   });

Task("Build name")
   .Does(() => {
      if(BuildSystem.IsRunningOnAzurePipelines)
      {
         BuildSystem.AzurePipelines.Commands.UpdateBuildNumber($"User Management version {version}");
      }
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
            Tag = new string[]{BuildImageTag()},
            Rm = true
        };

      DockerBuild(settings, ".");
   });

Task("Docker push")
   .WithCriteria(!BuildSystem.IsLocalBuild)
   .Does(() => {

      DockerPush(BuildImageTag());

   });

Task("SonarBegin")
   .ContinueOnError()
   .WithCriteria(() => !BuildSystem.IsLocalBuild && !disableSonar)
   .Does(() => {
      Information($"Sonar login {sonarLogin}, password {sonarPassword}");

      SonarBegin(new SonarBeginSettings{
         Key = "MyProject",
         Url = "sonarcube.contoso.local",
         Login = sonarLogin,
         Password = sonarPassword,
         Verbose = true
         });
      });

Task("SonarEnd")
   .ContinueOnError()
   .WithCriteria(() => !BuildSystem.IsLocalBuild && !disableSonar)
   .Does(() => {
      SonarEnd(new SonarEndSettings{
         Login = sonarLogin,
         Password = sonarPassword
      });
   });

Task("Default")
   .IsDependentOn("Version")
   .IsDependentOn("Build name")
   .IsDependentOn("SonarBegin")
   .IsDependentOn("Restore")
   .IsDependentOn("Build")
   .IsDependentOn("Test")
   .IsDependentOn("SonarEnd")
   .IsDependentOn("Publish")
   .IsDependentOn("Docker build")
   .IsDependentOn("Docker push")
   .Does(() => {
      Information("Default Task ended");
   })
   ;

RunTarget(target);