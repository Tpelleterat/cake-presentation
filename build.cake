///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var projectName = "UserManagement.sln";
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

Task("Build")
   .Does(() => {
      DotNetBuild(projectName, new DotNetBuildSettings{
         Configuration = configuration
      });
   });

Task("Test")
   .Does(() => {
      DotNetTest(projectName, new DotNetTestSettings{
         Configuration = configuration,
         NoBuild= true
      });
   });

Task("Default")
   .IsDependentOn("Build")
   .IsDependentOn("Test")
   .Does(() => {
      Information("Default Task ended");
   });
   ;

RunTarget(target);