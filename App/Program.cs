using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace App
{
    public class Connection
    {
        public string ConnectionString { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }


    internal class Program
    {
        private const string configFile = "config.json";

        private static void Main()
        {
            if (File.Exists(configFile))
            {
                var connections = JsonConvert.DeserializeObject<List<Connection>>(File.ReadAllText(configFile));
                var sqlFiles = Directory.GetFiles(".", "*.sql").Select(f=>new FileInfo(f)).OrderBy(f=>f.Name);

                foreach (var sqlFile in sqlFiles)
                {
                    string sql = File.ReadAllText(sqlFile.FullName);
                    Console.WriteLine(sqlFile.Name + ":");
                    foreach (var connection in connections)
                    {
                        using (var sqlConnection = new SqlConnection(connection.ConnectionString))
                        {
                            Console.Write(" - " + sqlConnection.Database);
                            try
                            {
                                var variableSQL = replaceVariables(sql, connection.Parameters);
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
                                string fileName = Path.GetFileNameWithoutExtension(sqlFile.Name) + "_" +
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
                    JsonConvert.SerializeObject(new List<Connection>
                        {
                            new Connection
                            {
                                ConnectionString = "ConnectionString1",
                                Parameters = new Dictionary<string, string>
                                {
                                    {"Parameter1","Value1"},
                                    {"Parameter2","Value2"}
                                }
                            },
                            new Connection
                            {
                                ConnectionString = "ConnectionString2"
                            }
                    }));
                Console.WriteLine("You need to put in your connection strings in '{0}'", configFile);
            }

            Console.ReadKey();
        }
        private const string variableTagRegex = @"(\[)(.+?)(\])";
        private static string replaceVariables(string originalString, Dictionary<string, string> variables)
        {
            if (variables == null)
                return originalString;

            var dict = new Dictionary<string, string>(variables, StringComparer.InvariantCultureIgnoreCase);
            return Regex.Replace(originalString, variableTagRegex, match =>
            {
                string key = match.Groups[2].Value.ToLower();

                return dict.ContainsKey(key) ? dict[key] : match.Value;
            });
        }
    }
}