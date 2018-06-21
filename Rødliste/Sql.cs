using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Npgsql;

namespace Rødliste
{
    internal class Sql
    {
        public string Select { get; set; }
        public List<string> From { get; set; }
        public List<string> Where { get; set; }
        internal static string ConnString { get; set; }

        private static IEnumerable<string> Execute(string select)
        {
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(select, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) yield return reader.GetString(0);
                }
            }
        }

        private static string CreateSqlStringForRegel(Sql regelSql)
        {
            var sql = "SELECT l_g.localid FROM " + string.Join(",", regelSql.From);

            sql += " WHERE " + string.Join(" AND ", regelSql.Where);

            return sql;
        }

        public static void GetNaturområder(Regel regel)
        {
            regel.Sql.Select = CreateSqlStringForRegel(regel.Sql);

            var naturområder = Execute(regel.Sql.Select).ToList();

            var trimChars = new[] {'{', '}'};
            for (var index = 0; index < naturområder.Count; index++)
                naturområder[index] = naturområder[index].Trim(trimChars);

            regel.Naturområder = naturområder.Count > 0 ? naturområder : null;
        }

        public static List<string> GetPredecessors(List<string> natursystem)
        {
            var select =
                $"SELECT ch.predecessor FROM data.codeshierarchy ch WHERE ch.successor = '{natursystem[0]}' AND (ch.predecessor like '%-E-%' OR ch.predecessor like '%-C-%')";

            natursystem.AddRange(Execute(select).ToList());

            if (natursystem.Count == 1) Console.WriteLine($"WARNING: No predescessors found for {natursystem[0]}");

            return natursystem;
        }

        public static void SetConnString(string configFile)
        {
            ConnString = CreateConnectionstring(configFile);
        }

        private static string CreateConnectionstring(string configFile)
        {
            dynamic config = JsonConvert.DeserializeObject(File.ReadAllText(configFile));
            return $"Host={config.host};Username={config.user};Password={config.pass};Database={config.db}";
        }
    }
}