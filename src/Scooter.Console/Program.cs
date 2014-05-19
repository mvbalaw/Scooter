using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace Scooter
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			var configuration = new Configuration();
			if (!configuration.Parse(args))
			{
				return -1;
			}

			return RunTests(args, configuration);
		}

		private static int RunTests(string[] args, Configuration configuration)
		{
			var fullPaths = configuration.Paths.Select(Path.GetFullPath);
			var configurationSettings = args.Where(x => !configuration.Paths.Contains(x)).ToArray();
			foreach (var assemblyPath in fullPaths)
			{
				try
				{
					var domainSetup = new AppDomainSetup
					                  {
						                  ApplicationBase = Path.GetDirectoryName(assemblyPath),
					                  };
					if (File.Exists(assemblyPath + ".config"))
					{
						domainSetup.ConfigurationFile = assemblyPath + ".config";
					}

					var evidence = new Evidence();
					evidence.AddHostEvidence(new Zone(SecurityZone.MyComputer));
					var appDomain = AppDomain.CreateDomain("Runner", evidence, domainSetup, new PermissionSet(PermissionState.Unrestricted));

					// Instantiate the Service type in the remote AppDomain and get a handle.
					var testRunner = (ITestRunner)appDomain.CreateInstanceFromAndUnwrap(typeof(TestRunner).Assembly.Location, typeof(TestRunner).FullName);

					var testsFailed = testRunner.ExecuteAssemblyTests(assemblyPath, configurationSettings);
					if (testsFailed)
					{
						return -1;
					}
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception);
					return -1;
				}
			}
			return 0;
		}
	}

}