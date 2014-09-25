using System.Linq;
using System.Reflection;

namespace Scooter
{
	public static class MethodInfoExtensions
	{
		public static bool IsRunAfterEachTestMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.AfterEachTestAttributeName));
		}

		public static bool IsRunAfterFailedTestMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.AfterEachFailedTestAttributeName));
		}

		public static bool IsRunAfterLastTestInFixtureMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.AfterLastTestInFixtureAttributeName));
		}

		public static bool IsRunAfterLastTestInSuiteMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.AfterLastTestInSuiteAttributeName));
		}

		public static bool IsRunAfterSuccessfulTestMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.AfterEachSuccessfulTestAttributeName));
		}

		public static bool IsRunBeforeEachTestMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.BeforeEachTestAttributeName));
		}

		public static bool IsRunBeforeFirstTestInFixtureMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.BeforeFirstTestInFixtureAttributeName));
		}

		public static bool IsRunBeforeFirstTestInSuiteMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.BeforeFirstTestInSuiteAttributeName));
		}

		public static bool IsTestMethod(this MethodInfo methodInfo, Configuration configuration)
		{
			return methodInfo.GetCustomAttributes()
				.Any(x => x.GetType().Name.Equals(configuration.TestAttributeName));
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