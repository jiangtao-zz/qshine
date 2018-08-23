using System;
using System.Data;
using System.Data.Common;
namespace qshine
{
	public class DbSession
	{
		public IDbConnection Connection { get; set;}
		public IDbTransaction Transaction { get; set;}
	}
}
