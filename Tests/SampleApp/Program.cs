using System;
using qshine;
using qshine.Configuration;
using System.Data.SqlClient;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var con = "Data Source=localhost;Initial Catalog=sampledb;Integrated Security=True;Connect Timeout=30;";
            using (var conn = new SqlConnection(con))
            {
                SqlCommand command = new SqlCommand("SELECT GETDATE()", conn);
                command.Connection.Open();
                var x = command.ExecuteScalar();
                Console.WriteLine(x);
            }

            ApplicationEnvironment.Build();

            var count = ApplicationEnvironment.Default.EnvironmentConfigure.Environments.Count;
            Console.WriteLine($"Environments={count}");
            Console.ReadKey();

        }
    }
}
