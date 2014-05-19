using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Scooter
{
	public interface ITestRunner
	{
		bool ExecuteAssemblyTests(string assemblyPath, string[] configurationSettings);
	}

	public class TestRunner : MarshalByRefObject, ITestRunner
	{
		private Configuration _configuration;

		public bool ExecuteAssemblyTests(string assemblyPath, string[] configurationSettings)
		{
			SetConfiguration(assemblyPath, configurationSettings);

			var assembly = Assembly.LoadFrom(assemblyPath);
			var testClasses = assembly
				.GetExportedTypes()
				.Where(x => x.IsClass)
				.Where(x => x.IsTestFixture(_configuration))
				.ToArray();

			TestFixtureInfo[] fixtures;
			if (_configuration.Parallel || _configuration.Shuffle)
			{
				// test fixture per test
				fixtures = testClasses
					.SelectMany(fixture => fixture.GetMethods()
						.Where(y => y.IsTestMethod(_configuration))
						.Select(testMethod => new TestFixtureInfo(fixture, testMethod, _configuration)))
					.ToArray();
			}
			else
			{
				fixtures = testClasses
					.Select(fixture => new TestFixtureInfo(fixture, _configuration))
					.ToArray();
			}

			VerifySuiteSetup(fixtures);
			VerifySuiteTeardown(fixtures);
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			RunSuiteSetup(fixtures);

			try
			{
				var tests = fixtures
					.Where(x => !x.IsIgnore)
					.SelectMany(x => x.Tests)
					.Where(x => !x.IsIgnore)
					.ToArray();
				RunAllTests(_configuration.Shuffle ? tests.Shuffle() : tests);
			}
			finally
			{
				try
				{
					RunSuiteTeardown(fixtures);
				}
				finally
				{
					stopwatch.Stop();
					Report(fixtures, stopwatch);
				}
			}
			return fixtures.SelectMany(x => x.Tests).Any(x => x.IsError);
		}

		private static void Report(TestFixtureInfo[] testResults, Stopwatch stopwatch)
		{
			Console.WriteLine();
			var errors = testResults.SelectMany(x => x.Tests).Where(x => x.IsError).ToArray();
			foreach (var item in errors)
			{
				Console.WriteLine("------------------------------------------------");
				Console.WriteLine(item.Name + " failed");
				Console.WriteLine(item.Message);
				Console.WriteLine(item.StackTrace);
			}

			Console.WriteLine("=================================================");
			var totalIgnored = testResults.SelectMany(x => x.Tests).Count(x => x.IsIgnore);
			var totalErrors = errors.Length;
			var totalRun = testResults.SelectMany(x => x.Tests).Count() - totalIgnored;

			Console.WriteLine(
				"Tests run: {0}, Errors: {1}, Failures: {2}, Inconclusive: {3}, Time: {4} seconds",
				totalRun,
				totalErrors,
				totalErrors,
				totalIgnored,
				stopwatch.Elapsed.TotalSeconds);

			Console.WriteLine("  Not run: {0}, Invalid: {1}, Ignored: {2}, Skipped: {3}",
				totalIgnored,
				errors.Count(),
				totalIgnored,
				totalIgnored);
			Console.WriteLine("=================================================");
		}

		private void RunAllTests(IEnumerable<TestInfo> tests)
		{
			if (_configuration.Parallel)
			{
				Parallel.ForEach(tests, test => test.Execute());
			}
			else
			{
				foreach (var test in tests)
				{
					test.Execute();
				}
			}
		}

		private static void RunSuiteSetup(IEnumerable<TestFixtureInfo> fixtures)
		{
			var setup = fixtures.SingleOrDefault(x => x.BeforeFirstFixture != null);
			if (setup == null)
			{
				return;
			}

			setup.RunBeforeFirstFixture();
		}

		private static void RunSuiteTeardown(IEnumerable<TestFixtureInfo> fixtures)
		{
			var setup = fixtures.SingleOrDefault(x => x.AfterLastFixture != null);
			if (setup == null)
			{
				return;
			}

			setup.RunAfterLastFixture();
		}

		private void SetConfiguration(string assemblyPath, string[] configurationSettings)
		{
			_configuration = new Configuration();
			_configuration.Parse(configurationSettings);

			Console.Write("Executing tests");
			if (_configuration.Parallel)
			{
				Console.Write(" parallel");
			}
			if (_configuration.Shuffle)
			{
				Console.Write(" shuffled");
			}
			Console.WriteLine(" in " + assemblyPath);
		}

		private static void VerifySuiteSetup(IEnumerable<TestFixtureInfo> fixtures)
		{
			var setups = fixtures.Where(x => x.BeforeFirstFixture != null).ToArray();
			if (!setups.Any())
			{
				return;
			}

			if (setups.Length > 1)
			{
				Console.Error.WriteLine("Found multiple suite setups");
				foreach (var fixture in setups)
				{
					Console.Error.Write(fixture.Name);
					Console.Error.Write('.');
					Console.Error.WriteLine(fixture.BeforeFirstFixture.Name);
				}
				throw new Exception("Found multiple suite setups");
			}
		}

		private static void VerifySuiteTeardown(IEnumerable<TestFixtureInfo> fixtures)
		{
			var setups = fixtures.Where(x => x.AfterLastFixture != null).ToArray();
			if (!setups.Any())
			{
				return;
			}

			if (setups.Length > 1)
			{
				Console.Error.WriteLine("Found multiple suite teardowns");
				foreach (var fixture in setups)
				{
					Console.Error.Write(fixture.Name);
					Console.Error.Write('.');
					Console.Error.WriteLine(fixture.AfterLastFixture.Name);
				}
				throw new Exception("Found multiple suite teardowns");
			}
		}
	}
}