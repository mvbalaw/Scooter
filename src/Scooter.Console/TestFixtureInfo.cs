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
			BeforeEachTest = GetSetupMethod(methods, x => x.IsTestSetupMethod(configuration), "BeforeEachTest");
			AfterEachTest = GetSetupMethod(methods, x => x.IsTestTeardownMethod(configuration), "AfterEachTest");
			BeforeFirstTest = GetSetupMethod(methods, x => x.IsFixtureSetupMethod(configuration), "BeforeFirstTest");
			AfterLastTest = GetSetupMethod(methods, x => x.IsFixtureTeardownMethod(configuration), "AfterLastTest");
			BeforeFirstFixture = GetSetupMethod(methods, x => x.IsSuiteSetupMethod(configuration), "BeforeFirstFixture");
			AfterLastFixture = GetSetupMethod(methods, x => x.IsSuiteTeardownMethod(configuration), "AfterLastFixture");
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

		private void RunAfterEachTest(bool ignore)
		{
			if (ignore)
			{
				return;
			}
			TryInvoke(AfterEachTest);
		}

		public void RunAfterLastFixture()
		{
			RunTest(AfterLastFixture, false);
		}

		private void RunAfterLastTest()
		{
			TryInvoke(AfterLastTest);
		}

		private void RunBeforeEachTest(bool ignore)
		{
			if (ignore)
			{
				return;
			}
			TryInvoke(BeforeEachTest);
		}

		public void RunBeforeFirstFixture()
		{
			RunTest(BeforeFirstFixture, false);
		}

		private void RunBeforeFirstTest(bool ignore)
		{
			if (ignore)
			{
				return;
			}
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

		private void RunTestWithDoubleCheck(MethodBase test, bool ignore)
		{
			if (ignore)
			{
				Console.Write('N');
				return;
			}
			RunBeforeEachTest(false);
			var instance = GetOrCreateInstance();

			try
			{
				test.Invoke(instance, null);
				Console.Write('.');
			}
			catch (Exception exception)
			{
				if (ExceptionIsExpected(test, exception.InnerException))
				{
					Console.Write('.');
					return;
				}
				RunAfterEachTest(false);

				Console.WriteLine("double checking " + Name + "." + test.Name + " due to: " + exception.InnerException.Message);
				RunBeforeEachTest(false);
				try
				{
					test.Invoke(instance, null);
					Console.Write('.');
				}
				catch
				{
					Console.Write('F');
					throw;
				}
			}
			finally
			{
				RunAfterEachTest(false);
			}
		}

		private void TryInvoke(MethodInfo method)
		{
			if (method == null)
			{
				return;
			}

			try
			{
				var instance = GetOrCreateInstance();
				method.Invoke(instance, null);
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine("Caught exception running " + Name + "." + method.Name);
				Console.Error.WriteLine(exception);
			}
		}

		public MethodInfo AfterEachTest { get; private set; }
		public MethodInfo AfterLastFixture { get; private set; }
		public MethodInfo AfterLastTest { get; private set; }
		public MethodInfo BeforeEachTest { get; private set; }
		public MethodInfo BeforeFirstFixture { get; private set; }
		public MethodInfo BeforeFirstTest { get; private set; }
		public bool IsIgnore { get; private set; }

		public string Name
		{
			get { return _class.FullName; }
		}
		public IEnumerable<TestInfo> Tests { get; private set; }
	}
}