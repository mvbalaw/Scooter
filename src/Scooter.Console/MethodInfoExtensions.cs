using System.Linq;
using System.Reflection;

namespace Scooter
{
	public static class MethodInfoExtensions
	{
		public static bool IsFixtureSetupMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.BeforeFirstTestInFixtureAttributeName));
		}

		public static bool IsFixtureTeardownMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.AfterLastTestInFixtureAttributeName));
		}

		public static bool IsSuiteSetupMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.BeforeFirstTestInSuiteAttributeName));
		}

		public static bool IsSuiteTeardownMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.AfterLastTestInSuiteAttributeName));
		}

		public static bool IsTestMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.TestAttributeName));
		}

		public static bool IsTestSetupMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.BeforeEachTestAttributeName));
		}

		public static bool IsTestTeardownMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.AfterEachTestAttributeName));
		}

		public static bool IsToBeIgnored(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x =>
				{
					var name = x.GetType().Name;
					return name.Equals(configuration.IgnoreTestAttributeName) || name.Equals(configuration.ExplicitTestAttributeName);
				});
		}
	}
}