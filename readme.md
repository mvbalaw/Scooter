Scooter is a simple NUnit console replacement.

By default it is configured to be compatible with NUnit attribute names but the attribute names can be replaced via command line configuraton switches if your tests are marked with different attributed names.

Unique features:

	Scooter double checks all test failures/errors by running the test a second time and only counting a failure if the second attempt fails.
	
	You can attribute an alternative method (-afta=AfterEachFailedTestAttribute) to be called after each test failure. This can facilitate capturing screenshots, test failure notification, etc.
	
		[AfterEachFailedTest]
		public void After_each_failed_test(string testFixtureName, string testMethodName) {}

	Scooter can optionally run the tests in parallel (-parallel) and/or shuffled (-shuffle).

Usage:

- replace the call to NUnit console in your build script with one that calls Scooter.Console.exe instead.

all TestFixtures in your assemblies will be run unless marked with an Ignore or Explicit attribute.

License: MIT License.
