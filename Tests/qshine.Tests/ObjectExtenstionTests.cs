using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;
using qshine;
using System;
namespace qshine.Tests
{
	[TestClass()]
	public class ObjectExtenstionTests
	{
		[TestMethod()]
		public void CallStaticMethod_Return_Value()
		{
			//best case
			int result2;
			Assert.IsTrue(typeof(DateTime).TryCall(out result2, "DaysInMonth", 2012,1));
			Assert.AreEqual(31, result2);

			//Method name incorrect
			bool result1;
			Assert.IsFalse(typeof(string).TryCall(out result1, "IsNullOrEmptyX", null));

			//Argument type is not match
			Assert.IsFalse(typeof(DateTime).TryCall(out result2, "DaysInMonth", 2012,"1"));

			//Argument number is not match
			Assert.IsFalse(typeof(DateTime).TryCall(out result2, "DaysInMonth", 2012));
		}

		[TestMethod()]
		public void CallStaticMethod_Without_Return_Value()
		{
			//best case
			var result = typeof(TestClass1).TryCall("SetP", 1);
			Assert.IsTrue(result);
			Assert.AreEqual(1,TestClass1.GetP1());

			//best case
			result = typeof(TestClass1).TryCall("SetP", 2,"3");
			Assert.IsTrue(result);
			Assert.AreEqual(2,TestClass1.GetP1());
			Assert.AreEqual("3",TestClass1.GetP2());

			//Missing argument
			result = typeof(TestClass1).TryCall("SetP");
			Assert.IsFalse(result);

			//Too manay args
			result = typeof(TestClass1).TryCall("SetP", 2, "3", true);
			Assert.IsFalse(result);

			//wrong type of argument
			result = typeof(TestClass1).TryCall("SetP","1");
			Assert.IsFalse(result);

			//wrong method name
			result = typeof(TestClass1).TryCall("SetP1",1);
			Assert.IsFalse(result);
		}

		[TestMethod()]
		public void CallInstanceMethod_Return_Value_ExplicitlyTypes()
		{
			var text = "1234";
			bool result;

			//best case
			Assert.IsTrue(text.TryCall(out result, new [] { typeof(string)},"Equals", "1234"));
			Assert.IsTrue(result, "Best case 1");

			//best case
			Assert.IsTrue(text.TryCall(out result, new [] { typeof(string)},"Equals", "12343"));
			Assert.IsFalse(result, "Best case 2");

			//Method name incorrect
			Assert.IsFalse(text.TryCall(out result, new [] { typeof(string)},"EqualsX", "12343"), "Method name incorrect");

			//Argument type is not match
			Assert.IsFalse(text.TryCall(out result, new [] { typeof(int)}, "Contains", 12343), "Argument type is not match");

			//Argument number is not match
			Assert.IsFalse(text.TryCall(out result, new [] { typeof(string),typeof(string)},"Contains", "1234", "1234"), "Argument number is not match");
		}

		[TestMethod()]
		public void CallInstanceMethod_Return_Value()
		{
			var text = "1234";
			bool result;

			//best case
			Assert.IsTrue(text.TryCall(out result, "Equals", "1234"));
			Assert.IsTrue(result, "Best case 1");

			//best case
			Assert.IsTrue(text.TryCall(out result, "Equals", "12343"));
			Assert.IsFalse(result, "Best case 2");

			//Method name incorrect
			Assert.IsFalse(text.TryCall(out result, "EqualsX", "12343"), "Method name incorrect");

			//Argument type is not match
			Assert.IsFalse(text.TryCall(out result, "Contains", 12343), "Argument type is not match");

			//Argument number is not match
			Assert.IsFalse(text.TryCall(out result, "Contains", "1234", "1234"), "Argument number is not match");
		}

		[TestMethod()]
		public void CallInstanceMethod_No_Return_Value_ExplicitlyTypes_Static()
		{
			var instance = new TestClass1();
			//best case
			var result = instance.TryCall(new [] {typeof(int)},"SetP", 1);
			Assert.IsTrue(result,"Tries to call static method by method name and explicitly provide arguments types");
			Assert.AreEqual(1, TestClass1.GetP1());

			//best case
			result = instance.TryCall(new[] { typeof(int), typeof(string)}, "SetP", 2, "3");
			Assert.IsTrue(result,"Tries to call static method by method name and explicitly provide arguments types");
			Assert.AreEqual(2, TestClass1.GetP1());
			Assert.AreEqual("3", TestClass1.GetP2());

			//Missing argument
			result = instance.TryCall(null, "SetP");
			Assert.IsFalse(result,"Tries to call static method by method name, but missing argument");

			//Too many args
			result = instance.TryCall(new[] { typeof(int), typeof(string), typeof(bool)},"SetP", 2, "3", true);
			Assert.IsFalse(result,"Tries to call static method by method name, but too many args");

			//wrong type of argument
			result = instance.TryCall(new [] {typeof(string)},"SetP", "1");
			Assert.IsFalse(result,"Tries to call static method by method name, but wrong type of argument");

			//wrong method name
			result = instance.TryCall(new [] {typeof(int)},"SetP1",  1);
			Assert.IsFalse(result,"Tries to call static method by method name, but wrong method name");
		}

		[TestMethod()]
		public void CallInstanceMethod_No_Return_Value_Types_Static()
		{
			var instance = new TestClass1();
			//best case
			var result = instance.TryCall("SetP", 1);
			Assert.IsTrue(result, "Tries to call static method by method name");
			Assert.AreEqual(1, TestClass1.GetP1());

			//best case
			result = instance.TryCall("SetP", 2, "3");
			Assert.IsTrue(result, "Tries to call static method by method name");
			Assert.AreEqual(2, TestClass1.GetP1());
			Assert.AreEqual("3", TestClass1.GetP2());

			//Missing argument
			result = instance.TryCall("SetP");
			Assert.IsFalse(result, "Tries to call static method by method name, but missing argument");

			//Too many args
			result = instance.TryCall("SetP", 2, "3", true);
			Assert.IsFalse(result, "Tries to call static method by method name, but too many args");

			//wrong type of argument
			result = instance.TryCall("SetP", "1");
			Assert.IsFalse(result, "Tries to call static method by method name, but wrong type of argument");

			//wrong method name
			result = instance.TryCall("SetP1", 1);
			Assert.IsFalse(result, "Tries to call static method by method name, but wrong method name");
		}

		[TestMethod()]
		public void CallInstanceMethod_No_Return_ExplicitlyTypes()
		{
			var instance = new TestClass1();
			//best case
			var result = instance.TryCall(new[] { typeof(int) }, "SetPX", 1);
			Assert.IsTrue(result,"Tries to call instance method by method name and explicitly provide arguments types");
			Assert.AreEqual(1, instance.GetPX1());

			//best case
			result = instance.TryCall(new[] { typeof(int), typeof(string) }, "SetPX", 2, "3");
			Assert.IsTrue(result,"Tries to call instance method by method name and explicitly provide arguments types");
			Assert.AreEqual(2, instance.GetPX1());
			Assert.AreEqual("3", instance.GetPX2());

			//Missing argument
			result = instance.TryCall(null, "SetPX", null);
			Assert.IsFalse(result,"Tries to call method by method name, but missing argument");

			//Too manay args
			result = instance.TryCall(new[] { typeof(int), typeof(string), typeof(bool) }, "SetPX", 2, "3", true);
			Assert.IsFalse(result,"Tries to call method by method name, but too many args");

			//wrong type of argument
			result = instance.TryCall(new[] { typeof(string) },"SetPX", "1");
			Assert.IsFalse(result,"Tries to call method by method name, but wrong type of argument");

			//wrong method name
			result = instance.TryCall(new[] { typeof(int) },"SetPX1",  1);
			Assert.IsFalse(result,"Tries to call method by method name, but wrong method name");
		}
	
		[TestMethod()]
		public void CallInstanceMethod_No_Return_Types()
		{
			var instance = new TestClass1();
			//best case
			var result = instance.TryCall("SetPX", 1);
			Assert.IsTrue(result, "Tries to call instance method by method name");
			Assert.AreEqual(1, instance.GetPX1());

			//best case
			result = instance.TryCall("SetPX", 2, "3");
			Assert.IsTrue(result, "Tries to call instance method by method name");
			Assert.AreEqual(2, instance.GetPX1());
			Assert.AreEqual("3", instance.GetPX2());

			//Missing argument
			result = instance.TryCall("SetPX");
			Assert.IsFalse(result, "Tries to call method by method name, but missing argument");

			//Too manay args
			result = instance.TryCall("SetPX", 2, "3", true);
			Assert.IsFalse(result, "Tries to call method by method name, but too many args");

			//wrong type of argument
			result = instance.TryCall("SetPX", "1");
			Assert.IsFalse(result, "Tries to call method by method name, but wrong type of argument");

			//wrong method name
			result = instance.TryCall("SetPX1", 1);
			Assert.IsFalse(result, "Tries to call method by method name, but wrong method name");
		}

		[TestMethod()]
		public void CallInstanceGenericMethod_Return_Value()
		{
			int result;
			var instance = new TestClass1();
			//best case
			var hasMethod = instance.TryCall(
				out result, 
				new [] {
					typeof(string),
					typeof(string),
					typeof(double),
				},
				new [] {
					typeof(int),
					typeof(string),
					typeof(string),
					typeof(double),
				},
				"SetGeneric", 9,"1","2",12.5);
			
			Assert.IsTrue(hasMethod, "Tries to call instance generic method by method name");
			Assert.AreEqual("1,2,12.5", instance.GetPX2());

			//Missing argument
			hasMethod = instance.TryCall(
				out result,
				new[] {
								typeof(string),
								typeof(string),
								typeof(double),
				},
				new[] {
								typeof(string),
								typeof(string),
								typeof(double),
				},
				"SetGeneric", 9, "1", "2", 12.5);

			Assert.IsFalse(hasMethod, "Tries to call instance generic method by method name, but missing argument type");
		
			//Missing generic argument
			hasMethod = instance.TryCall(
				out result,
				new[] {
								typeof(string),
								typeof(double),
				},
				new[] {
								typeof(int),
								typeof(string),
								typeof(string),
								typeof(double),
				},
				"SetGeneric", 9, "1", "2", 12.5);
			Assert.IsFalse(hasMethod, "Tries to call instance generic method by method name, but missing generic argument type");

			//Too manay args
			hasMethod = instance.TryCall(
				out result,
				new [] {
					typeof(string),
					typeof(string),
					typeof(double),
				},
				new [] {
					typeof(int),
					typeof(string),
					typeof(string),
					typeof(double),
					typeof(int),
				},
				"SetGeneric", 9, "1", "2", 12.5,12);
			Assert.IsFalse(hasMethod, "Tries to call instance generic method by method name, but Too manay args");
		
			//wrong type argument
			hasMethod = instance.TryCall(
				out result,
				new[] {
								typeof(string),
								typeof(string),
								typeof(double),
				},
				new[] {
								typeof(int),
								typeof(string),
								typeof(string),
								typeof(string),
				},
				"SetGeneric", 9, "1", "2", "12.5");
			Assert.IsFalse(hasMethod, "Tries to call instance generic method by method name, but wrong type argument");

			//wrong method name
			hasMethod = instance.TryCall(
				out result,
				new[] {
								typeof(string),
								typeof(string),
								typeof(double),
				},
				new[] {
								typeof(int),
								typeof(string),
								typeof(string),
								typeof(double),
				},
				"SetGeneric1", 9, "1", "2", 12.5);
			Assert.IsFalse(hasMethod, "Tries to call instance generic method by method name, but wrong method name");
		
		}
	}

	public class TestClass1
	{
		static int _p1;
		static string _p2;
		int _pX1;
		string _pX2;

		public static void SetP(int p1)
		{
			_p1 = p1;
		}
		public static void SetP(int p1, string p2)
		{
			_p1 = p1;
			_p2 = p2;
		}
		public static int GetP1()
		{
			return _p1;
		}
		public static string GetP2()
		{
			return _p2;
		}

		public void SetPX(int p1)
		{
			_pX1 = p1;
		}
		public void SetPX(int p1, string p2)
		{
			_pX1 = p1;
			_pX2 = p2;
		}

		public int SetGeneric<T1, T2, T3>(int p0, T1 p1, T2 p2, T3 p3)
		{
			_pX1 = p0;
			_pX2 = p1.ToString() + "," + p2.ToString() + "," + p3.ToString();
			return p0;
		}

		public int SetGeneric(int p0)
		{
			_pX2 = "Just give second without name.";
			return p0;
		}

		public int GetPX1() { return _pX1; }
		public string GetPX2() { return _pX2; }
	}
}
