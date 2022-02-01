///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionName = "UserManagement.sln";
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
   .IsDependentOn("Restore")
   .IsDependentOn("Build")
   .IsDependentOn("Test")
   .IsDependentOn("Publish")
;

Task("Restore")
   .Does(() => {
      Information("Restore!");

      DotNetRestore(solutionName);
   });

Task("Build")
   .Does(() => {
      Information("Build!");

      DotNetBuild(".", new DotNetBuildSettings{
         Configuration = configuration
      });
   });

Task("Test")
   .Does(() => {
      Information("Test!");

      DotNetTest(".", new DotNetTestSettings{
         Configuration = configuration,
         NoBuild= true
      });
   });

Task("Publish")
   .Does(() => {
      Information("Publish!");

      DotNetPublish(solutionName, new DotNetPublishSettings{
         Configuration = configuration,
         OutputDirectory = "build/publish",
      });
   });


RunTarget(target);