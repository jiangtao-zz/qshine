using System;

namespace sqliteSample
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			var db = new DatabaseScripts();
			db.CreateDatabase("ss");
		}
	}
}
