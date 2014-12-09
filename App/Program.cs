using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace App
{
    public class Config
    {
        public List<string> ConnectionStrings { get; set; }
    }

    internal class Program
    {
        private const string configFile = "config.json";

        private static void Main()
        {
            if (File.Exists(configFile))
            {
                var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile));
                string[] sqlFiles = Directory.GetFiles(".", "*.sql");

                foreach (string sqlFile in sqlFiles)
                {
                    var sqlFileInfo = new FileInfo(sqlFile);
                    string sql = File.ReadAllText(sqlFile);
                    Console.WriteLine(sqlFileInfo.Name + ":");
                    foreach (string connectionString in config.ConnectionStrings)
                    {
                        using (var sqlConnection = new SqlConnection(connectionString))
                        {
                            Console.Write(" - " + sqlConnection.Database);
                            try
                            {
                                using (var sqlCommand = new SqlCommand(sql, sqlConnection))
                                {
                                    var stopwatch = new Stopwatch();
                                    stopwatch.Start();
                                    sqlConnection.Open();
                                    sqlCommand.ExecuteNonQuery();
                                    sqlConnection.Close();
                                    stopwatch.Stop();
                                    Console.WriteLine(": {0}", stopwatch.Elapsed);
                                }
                            }
                            catch (Exception ex)
                            {
                                string fileName = Path.GetFileNameWithoutExtension(sqlFileInfo.Name) + "_" +
                                                  sqlConnection.Database + "_" + DateTime.Now.ToString("yyyyMMdd") +
                                                  "_Error.txt";
                                File.WriteAllText(fileName, ex.ToString());
                                Console.WriteLine(": Error");
                            }
                        }
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.WriteLine("Done. Press any key to exit.");
            }
            else
            {
                File.WriteAllText(configFile,
                    JsonConvert.SerializeObject(new Config
                    {
                        ConnectionStrings = new List<string> {"ConnectionString1", "ConnectionString2"}
                    }));
                Console.WriteLine("You need to put in your connection strings in '{0}'", configFile);
            }

            Console.ReadKey();
        }
    }
}