using NUnit.Framework;
using qshine.Configuration;
using qshine;
using System;
namespace qshine.UnitTests
{
	[TestFixture()]
	public class InterceptorTests
	{
		[Test()]
		public void ExecuteMethod_Success()
		{
			var interceptor = new Interceptor();
			interceptor.OnEnter += (sender, e) =>
			{
				var args = e;
				Assert.IsNotNull(args);
				Assert.AreEqual(2, args.Args.Count);
				Assert.AreEqual(1, args.Args[0]);
				Assert.AreEqual("2", args.Args[1]);
				Assert.AreEqual("InterceptorMethodTest", args.MethodName);
			};
			interceptor.OnSuccess += (sender, e) =>
			{
				var args = e;
				Assert.IsNotNull(args);
				Assert.AreEqual(2, args.Args.Count);
				Assert.AreEqual(1, args.Args[0]);
				Assert.AreEqual("2", args.Args[1]);
				Assert.AreEqual("InterceptorMethodTest", args.MethodName);
				Assert.AreEqual("2_1", args.Result);
			};	
			interceptor.OnExit += (sender, e) =>
			{
				var args = e;
				Assert.IsNotNull(args);
				Assert.AreEqual(2, args.Args.Count);
				Assert.AreEqual(1, args.Args[0]);
				Assert.AreEqual("2", args.Args[1]);
				Assert.AreEqual("InterceptorMethodTest", args.MethodName);
				Assert.AreEqual("2_1", args.Result);
			};

			var result = InterceptorMethodTest(1, "2", interceptor);
			Assert.AreEqual("2_1", result);
		}

		[Test()]
		public void ExecuteMethod_Exception()
		{
			var interceptor = new Interceptor();
			interceptor.OnEnter += (sender, e) =>
			{
				var args = e;
				Assert.IsNotNull(args);
				Assert.AreEqual(3, args.Args.Count);
				Assert.AreEqual(1, args.Args[0]);
				Assert.AreEqual(0, args.Args[1]);
				Assert.AreEqual("3", args.Args[2]);
				Assert.AreEqual("InterceptorMethodTest1", args.MethodName);
			};
			interceptor.OnExit += (sender, e) =>
			{
				var args = e;
				Assert.IsNotNull(args);
				Assert.AreEqual(3, args.Args.Count);
				Assert.AreEqual(1, args.Args[0]);
				Assert.AreEqual(0, args.Args[1]);
				Assert.AreEqual("3", args.Args[2]);
				Assert.AreEqual("InterceptorMethodTest1", args.MethodName);
			};

			interceptor.OnException += ExceptionHandler;
			var result = InterceptorMethodTest1(1,0, "3", interceptor);
			interceptor.OnException -= ExceptionHandler;
			Assert.AreEqual(default(string), result);

			interceptor.OnException += ExceptionThrowHandler;

			try
			{
				InterceptorMethodTest1(1, 0, "3", interceptor);
				Assert.Fail("It should throw exception");
			}
			catch (DivideByZeroException)
			{
			}
		}

		[Test()]
		public void ExecuteMethod_ManyHandlers()
		{
			var interceptor = new Interceptor();
			interceptor.OnEnter += (sender, e) =>
			{
				var args = e as InterceptorEventArgs;
				Assert.IsNotNull(args);
				Assert.AreEqual(3, args.Args.Count);
				Assert.AreEqual(32, args.Args[0]);
				Assert.AreEqual(2, args.Args[1]);
				Assert.AreEqual("3", args.Args[2]);
				Assert.AreEqual("InterceptorMethodTest1", args.MethodName);
			};
			interceptor.OnEnter += (sender, e) =>
			{
				var args = e as InterceptorEventArgs;
				Assert.IsNotNull(args);
				Assert.AreEqual(3, args.Args.Count);
				Assert.AreEqual(32, args.Args[0]);
				Assert.AreEqual(2, args.Args[1]);
				Assert.AreEqual("3", args.Args[2]);
				Assert.AreEqual("InterceptorMethodTest1", args.MethodName);

			};

			var result = InterceptorMethodTest1(32, 2, "3", interceptor);
			Assert.AreEqual("3_16", result);

		}

		public string InterceptorMethodTest(int p1,string p2, Interceptor interceptor)
		{
			return interceptor.JoinPoint<string>(() =>
			 {
				 int ip1 = p1;
				 return p2 + "_" + p1;
			 },
			 this,"InterceptorMethodTest", p1, p2);
		}

		public string InterceptorMethodTest1(int p1, int p2, string p3, Interceptor interceptor)
		{
			return interceptor.JoinPoint<string>(() =>
			 {
				 return p3 + "_" + p1/p2;
			 },
             this, "InterceptorMethodTest1", p1, p2,p3);
		}

		void ExceptionHandler(object sender, InterceptorEventArgs e)
		{
			var args = e;
			Assert.IsNotNull(args);
			Assert.AreEqual(3, args.Args.Count);
			Assert.AreEqual(1, args.Args[0]);
			Assert.AreEqual(0, args.Args[1]);
			Assert.AreEqual("3", args.Args[2]);
			Assert.AreEqual("InterceptorMethodTest1", args.MethodName);
			Assert.IsTrue(args.Exception is System.Exception);
			args.StopExecution = true;
		}

		void ExceptionThrowHandler(object sender, EventArgs e)
		{
			var args = e as InterceptorEventArgs;
			Assert.IsNotNull(args);
			Assert.AreEqual(3, args.Args.Count);
			Assert.AreEqual(1, args.Args[0]);
			Assert.AreEqual(0, args.Args[1]);
			Assert.AreEqual("3", args.Args[2]);
			Assert.AreEqual("InterceptorMethodTest1", args.MethodName);
			Assert.IsTrue(args.Exception is System.Exception);
			Assert.IsTrue(sender is InterceptorTests);
			args.StopExecution = false;
		}
	}
}
