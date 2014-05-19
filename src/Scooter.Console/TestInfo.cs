using System;
using System.Reflection;

namespace Scooter
{
	internal class TestInfo
	{
		public TestInfo(MethodInfo testMethod, bool isIgnore, Action<MethodInfo, bool> runTest, Configuration configuration)
		{
			_testMethod = testMethod;
			_runTest = runTest;
			IsIgnore = isIgnore || testMethod.IsToBeIgnored(configuration);
		}

		private readonly Action<MethodInfo, bool> _runTest;
		private readonly MethodInfo _testMethod;

		public void Execute()
		{
			try
			{
				_runTest(_testMethod, IsIgnore);
			}
			catch (Exception exception)
			{
				SetError(exception);
			}
		}

		public void SetError(Exception exception1)
		{
			IsError = true;
			Message = exception1.InnerException.Message;
			StackTrace = exception1.InnerException.ToString();
		}

		public bool IsError { get; private set; }
		public bool IsIgnore { get; private set; }
		public string Message { get; private set; }
		public string Name
		{
			get { return _testMethod.Name; }
		}
		public string StackTrace { get; private set; }
	}
}