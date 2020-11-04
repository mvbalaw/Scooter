using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Scooter
{
	internal class TestFixtureInfo
	{
		public TestFixtureInfo(Type @class, MethodInfo testMethod, Configuration configuration)
			: this(@class, configuration)
		{
			Tests = new[] { new TestInfo(testMethod, IsIgnore, RunTest, configuration) };
		}

		public TestFixtureInfo(Type @class, Configuration configuration)
		{
			_class = @class;
			_configuration = configuration;
			var methods = @class.GetMethods(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
			BeforeEachTest = GetSetupMethod(methods, x => x.IsRunBeforeEachTestMethod(configuration), "BeforeEachTest");
			AfterEachTest = GetSetupMethod(methods, x => x.IsRunAfterEachTestMethod(configuration), "AfterEachTest");
			AfterEachSuccessfulTest = GetSetupMethod(methods, x => x.IsRunAfterSuccessfulTestMethod(configuration), "AfterEachSuccessfulTest");
			AfterEachFailedTest = GetSetupMethod(methods, x => x.IsRunAfterFailedTestMethod(configuration) && x.GetParameters().Length == 2 && x.GetParameters().All(y => y.ParameterType == typeof(string)), "AfterEachFailedTest");
			BeforeFirstTest = GetSetupMethod(methods, x => x.IsRunBeforeFirstTestInFixtureMethod(configuration), "BeforeFirstTest");
			AfterLastTest = GetSetupMethod(methods, x => x.IsRunAfterLastTestInFixtureMethod(configuration), "AfterLastTest");
			BeforeFirstFixture = GetSetupMethod(methods, x => x.IsRunBeforeFirstTestInSuiteMethod(configuration), "BeforeFirstFixture");
			AfterLastFixture = GetSetupMethod(methods, x => x.IsRunAfterLastTestInSuiteMethod(configuration), "AfterLastFixture");
			IsIgnore = @class.IsToBeIgnored(configuration);
			Tests = methods
				.Where(x => x.IsTestMethod(_configuration))
				.Select(x => new TestInfo(x, IsIgnore, RunTest, configuration))
				.ToArray();
		}

		private readonly Type _class;
		private readonly Configuration _configuration;
		private object _instance;
		private readonly IList<string> _testsExecuted = new List<string>();
		private readonly IList<string> _testsVisited = new List<string>();

		private bool ExceptionIsExpected(MemberInfo test, Exception exception)
		{
			var customAttributes = test.CustomAttributes.ToArray();
			var expectedException = customAttributes.FirstOrDefault(x => x.AttributeType.Name.Equals(_configuration.ExpectedExceptionAttributeName));
			if (expectedException == null)
			{
				return false;
			}
			if (expectedException.NamedArguments == null)
			{
				return false;
			}

			foreach (var argument in expectedException.NamedArguments)
			{
				switch (argument.MemberName)
				{
					case "ExpectedMessage":
						if (argument.TypedValue.Value.ToString() != exception.Message)
						{
							return false;
						}
						break;
					case "ExpectedException":
						if ((Type)argument.TypedValue.Value != exception.GetType())
						{
							return false;
						}
						break;
					default:
						throw new NotImplementedException("don't know how to handle ExpectedException." + argument.MemberName + " == " + argument.TypedValue);
				}
			}
			return true;
		}

		public object GetOrCreateInstance()
		{
			return _instance ?? (_instance = Activator.CreateInstance(_class));
		}

		private MethodInfo GetSetupMethod(IEnumerable<MethodInfo> methods, Func<MethodInfo, bool> func, string descriptionForError)
		{
			var matches = methods.Where(func).ToList();
			if (!matches.Any())
			{
				return null;
			}
			if (matches.Count == 1)
			{
				return matches.Single();
			}
			throw new Exception("Test fixture " + _class.FullName + " has multiple methods marked: " + descriptionForError);
		}

		private void Log(string description, MethodInfo method)
		{
			if (method != null && _configuration.Verbose)
			{
				Console.WriteLine("-> " + description + ": " + Name + "." + method.Name);
			}
		}

		private void RunAfterEachFailedTest(string testName)
		{
			if (AfterEachFailedTest != null)
			{
				Log("RunAfterEachFailedTest", AfterEachFailedTest);
				TryInvoke(AfterEachFailedTest, new object[] { Name, testName });
			}
		}

		private void RunAfterEachSuccessfulTest()
		{
			Log("RunAfterEachSuccessfulTest", AfterEachSuccessfulTest);
			TryInvoke(AfterEachSuccessfulTest);
		}

		private void RunAfterEachTest()
		{
			Log("RunAfterEachTest", AfterEachTest);
			TryInvoke(AfterEachTest);
		}

		public void RunAfterLastFixture()
		{
			RunTest(AfterLastFixture, false);
		}

		private void RunAfterLastTest()
		{
			Log("RunAfterLastTest", AfterLastTest);
			TryInvoke(AfterLastTest);
		}

		private void RunBeforeEachTest()
		{
			Log("RunBeforeEachTest", BeforeEachTest);
			TryInvoke(BeforeEachTest);
		}

		public void RunBeforeFirstFixture()
		{
			Log("RunBeforeFirstFixture", BeforeFirstFixture);
			RunTest(BeforeFirstFixture, false);
		}

		private void RunBeforeFirstTest(bool ignore)
		{
			if (ignore)
			{
				return;
			}
			Log("RunBeforeFirstTest", BeforeFirstTest);
			TryInvoke(BeforeFirstTest);
		}

		private void RunTest(MethodInfo test, bool ignore)
		{
			if (_testsVisited.Count == 0)
			{
				RunBeforeFirstTest(ignore);
			}
			try
			{
				RunTestWithDoubleCheck(test, ignore);
			}
			finally
			{
				_testsVisited.Add(test.Name);
				if (!ignore)
				{
					_testsExecuted.Add(test.Name);
				}
				if (_testsVisited.Count == Tests.Count())
				{
					RunAfterLastTest();
					_instance = null;
				}
			}
		}

		private void RunTestWithDoubleCheck(MethodInfo test, bool ignore)
		{
			if (ignore)
			{
				Console.Write('N');
				return;
			}
			RunBeforeEachTest();
			var instance = GetOrCreateInstance();

			try
			{
				Log("RunTest", test);
				test.Invoke(instance, null);
				Console.Write('.');
				RunAfterEachSuccessfulTest();
				RunAfterEachTest();
			}
			catch (Exception exception)
			{
				if (ExceptionIsExpected(test, exception.InnerException))
				{
					Console.Write('.');
					RunAfterEachSuccessfulTest();
					RunAfterEachTest();
					return;
				}
				RunAfterEachFailedTest(test.Name);
				RunAfterEachTest();

				if (exception.InnerException != null)
					Console.WriteLine("double checking " + Name + "." + test.Name + " due to: " +
					                  exception.InnerException.Message);
				RunBeforeEachTest();
				try
				{
					Log("RunTest", test);
					test.Invoke(instance, null);
					Console.Write('.');
					RunAfterEachSuccessfulTest();
					RunAfterEachTest();
				}
				catch
				{
					Console.Write('F');
					RunAfterEachFailedTest(test.Name);
					RunAfterEachTest();
					throw;
				}
			}
		}

		private void TryInvoke(MethodInfo method, object[] parameters = null)
		{
			if (method == null)
			{
				return;
			}

			try
			{
				var instance = GetOrCreateInstance();
				method.Invoke(instance, parameters);
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine("Caught exception running " + Name + "." + method.Name);
				Console.Error.WriteLine(exception);
			}
		}

		private MethodInfo AfterEachFailedTest { get; }
		public MethodInfo AfterEachSuccessfulTest { get; }
		private MethodInfo AfterEachTest { get; }
		public MethodInfo AfterLastFixture { get; }
		private MethodInfo AfterLastTest { get; }
		private MethodInfo BeforeEachTest { get; }
		public MethodInfo BeforeFirstFixture { get; }
		private MethodInfo BeforeFirstTest { get; }
		public bool IsIgnore { get; }

		public string Name => _class.FullName;
		public IEnumerable<TestInfo> Tests { get; }
	}
}