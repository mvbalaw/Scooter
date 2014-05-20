using System;
using System.Collections.Generic;

using NDesk.Options;

namespace Scooter
{
	[Serializable]
	public class Configuration
	{
		public Configuration()
		{
			BeforeFirstTestInSuiteAttributeName = "SuiteSetUpAttribute";
			AfterLastTestInSuiteAttributeName = "SuiteTearDownAttribute";
			BeforeFirstTestInFixtureAttributeName = "TestFixtureSetUpAttribute";
			AfterLastTestInFixtureAttributeName = "TestFixtureTearDownAttribute";
			BeforeEachTestAttributeName = "SetUpAttribute";
			AfterEachSuccessfulTestAttributeName = "TearDownAttribute";
			IgnoreTestAttributeName = "IgnoreAttribute";
			ExplicitTestAttributeName = "ExplicitAttribute";
			TestAttributeName = "TestAttribute";
			TestFixtureAttributeName = "TestFixtureAttribute";
			ExpectedExceptionAttributeName = "ExpectedExceptionAttribute";
		}

		public bool Parse(string[] args)
		{
			var p = new OptionSet
			        {
				        { "tfa=", "Test Fixture attribute name (default=TestFixtureAttribute)", v => TestFixtureAttributeName = v ?? "" },
				        { "btsa=", "Before first test in Test Suite attribute name (default=SuiteSetUpAttribute)", v => BeforeFirstTestInSuiteAttributeName = v ?? "" },
				        { "btfa=", "Before first test in Test Fixture attribute name (default=TestFixtureSetUpAttribute)", v => BeforeFirstTestInFixtureAttributeName = v ?? "" },
				        { "bta=", "Before each test attribute name (default=SetUpAttribute)", v => BeforeEachTestAttributeName = v ?? "" },
				        { "ta=", "Test attribute name (default=TestAttribute)", v => TestAttributeName = v ?? "" },
				        { "asta=", "After each successful test attribute name (default=TearDownAttribute)", v => AfterEachSuccessfulTestAttributeName = v ?? "" },
				        { "afta=", "After each failed test attribute name (default=TearDownAttribute)", v => AfterEachFailedTestAttributeName = v ?? "" },
				        { "atfa=", "After last test in Test Fixture attribute name (default=TestFixtureTearDownAttribute)", v => AfterLastTestInFixtureAttributeName = v ?? "" },
				        { "atsa=", "After last test in Test Suite attribute name (default=SuiteTearDownAttribute)", v => AfterLastTestInSuiteAttributeName = v ?? "" },
				        { "ita=", "Ignored test attribute name (default=IgnoreAttribute)", v => IgnoreTestAttributeName = v ?? "" },
				        { "eta=", "Explicit test attribute name (default=ExplicitAttribute)", v => ExplicitTestAttributeName = v ?? "" },
				        { "eea=", "Expected exception attribute name (default=ExpectedExceptionAttribute)", v => ExpectedExceptionAttributeName = v ?? "" },
				        { "shuffle", "Run the tests in random order", v => Shuffle = true },
				        { "parallel", "Run the tests in paralle", v => Parallel = true },
			        };

			try
			{
				Paths = p.Parse(args);
				return true;
			}
			catch (OptionException exception)
			{
				Console.Write("Error parsing arguments: ");
				Console.WriteLine(exception.Message);
				p.WriteOptionDescriptions(Console.Out);
				return false;
			}
		}

		public string AfterEachFailedTestAttributeName { get; set; }
		public string AfterEachSuccessfulTestAttributeName { get; private set; }
		public string AfterLastTestInFixtureAttributeName { get; private set; }
		public string AfterLastTestInSuiteAttributeName { get; private set; }
		public string BeforeEachTestAttributeName { get; private set; }
		public string BeforeFirstTestInFixtureAttributeName { get; private set; }
		public string BeforeFirstTestInSuiteAttributeName { get; private set; }
		public string ExpectedExceptionAttributeName { get; private set; }
		public string ExplicitTestAttributeName { get; private set; }
		public string IgnoreTestAttributeName { get; private set; }
		public bool Parallel { get; private set; }
		public List<string> Paths { get; private set; }
		public bool Shuffle { get; private set; }
		public string TestAttributeName { get; private set; }
		public string TestFixtureAttributeName { get; private set; }
	}
}