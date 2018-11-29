using System;
using qshine.Configuration;
using qshine.database.common;
using qshine.database.common.language;
using qshine.database.idm;
//using qshine.LogInterceptor;

namespace qshine.database
{
	public class program
	{
		public static void Main()
		{
			ApplicationEnvironment.Build("app.config");

            var builder = new DatabaseBuilder("testDatabase");
            builder.Build();

        }
	}
}
