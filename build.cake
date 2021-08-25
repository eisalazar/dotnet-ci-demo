///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var solutionDir =".";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(ctx =>
{
   // Executed BEFORE the first task.
    Information($"Running target [{target}] in configuration [{configuration}]");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

Task("Restore").Does(()=> {
    var settings = new DotNetCoreRestoreSettings()
    {
        Sources = new[] {"http://baget.apps.andreani.com.ar/v3/index.json", "https://www.nuget.org/api/v2/"}
    };
    DotNetCoreRestore(solutionDir, settings);
});

Task("Build").IsDependentOn("Restore")
  .Does(() => {
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    };

    DotNetCoreBuild(solutionDir, settings);
  });

Task("Test")
  .Does(() => {
    var settings = new DotNetCoreToolSettings()
        {
            ArgumentCustomization = args => 
                args.Append("test\\bin\\Debug\\netcoreapp3.1\\ConsoleDemo.Test.dll")
                .Append("--target").AppendQuoted("dotnet")
                .Append("--targetargs").AppendQuoted("test test --no-build --logger trx;LogFileName=unit_tests.xml")
                .Append("--format opencover")
        };  

    DotNetCoreTool("coverlet", settings);

  });
  

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

RunTarget(target);