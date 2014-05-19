using System;
using System.Linq;

namespace Scooter
{
	public static class TypeExtensions
	{
		public static bool IsTestFixture(this Type type, Configuration configuration)
		{
			var isTextFixture = Attribute
				.GetCustomAttributes(type)
				.Any(x => x.GetType().Name.Equals(configuration.TestFixtureAttributeName));
			return isTextFixture;
		}

		public static bool IsToBeIgnored(this Type type, Configuration configuration)
		{
			var isTextFixture = Attribute
				.GetCustomAttributes(type)
				.Any(x =>
				{
					var name = x.GetType().Name;
					return name.Equals(configuration.IgnoreTestAttributeName) || name.Equals(configuration.ExplicitTestAttributeName);
				});
			return isTextFixture;
		}
	}
}