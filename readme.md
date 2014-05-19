Scooter is a simple NUnit console replacement.

By default it is configured to be compatible with NUnit attribute names but the attribute names can be replaced with command line configuraton switches if your tests are marked with a different attributed names.

Scooter can optionally run the tests in parallel (-parallel) and/or shuffled (-shuffle).

Scooter double checks all test failures and exceptions by running the test a second time and only reporting a failure if the second attempt fails.

Usage:

- replace the call to NUnit console in your build script with one that calls Scooter.exe instead.

all TestFixtures in your assemblies will be run unless marked with an Ignore or Explicit attribute default.

License: MIT License.
