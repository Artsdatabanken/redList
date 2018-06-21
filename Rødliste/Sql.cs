using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Npgsql;

namespace Rødliste
{
    internal class Sql
    {
        public string Select { get; set; }
        public List<string> From { get; set; }
        public List<string> Where { get; set; }
        internal static string ConnString  { get; set; }
        
        public static void Execute(Regel regel)
        {
            regel.Sql.Select = CreateSqlStringForRegel(regel.Sql);
            var trimChars = new [] {'{', '}'};

            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(regel.Sql.Select, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if(reader.HasRows) regel.Naturområder = new List<string>();
                    while (reader.Read())
                    {
                        var localid = reader.GetString(0);
                        localid = localid.Trim(trimChars);
                        regel.Naturområder.Add(localid);
                    }
                }
            }
        }

        private static string CreateConnectionstring(string configFile)
        {
            dynamic config = JsonConvert.DeserializeObject(File.ReadAllText(configFile));
            return $"Host={config.host};Username={config.user};Password={config.pass};Database={config.db}";
        }

        private static string CreateSqlStringForRegel(Sql regelSql)
        {
            var sql = "SELECT l_g.localid FROM " + string.Join(",", regelSql.From);

            sql += " WHERE " + string.Join(" AND ", regelSql.Where);

            return sql;
        }

        public static List<string> GetPredecessors(List<string> natursystem)
        {
            var select =
                $"SELECT ch.predecessor FROM data.codeshierarchy ch WHERE ch.successor = '{natursystem[0]}' AND (ch.predecessor like '%-E-%' OR ch.predecessor like '%-C-%')";
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(select, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine($"WARNING: No predescessors found for {natursystem[0]}");
                        return natursystem;
                    }
                    while (reader.Read())
                    {
                        natursystem.Add(reader.GetString(0));
                    }
                }
            }

            return natursystem;
        }

        public static void SetConnString(string configFile)
        {
            ConnString = CreateConnectionstring(configFile);

        }
    }
}