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
        public string QueryString { get; set; }
        public List<string> From { get; set; }
        public List<string> Where { get; set; }
        internal static string ConnString { get; set; }
        internal static readonly char[] TrimChars = {'{', '}'};

        private static IEnumerable<string> Select(string queryString)
        {
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(queryString, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) yield return reader.GetString(0);
                }
            }
        }

        public static void GetNaturområder(Regel regel)
        {
            CreateSqlStringForRegel(regel.Sql);

            var naturområder = Select(regel.Sql.QueryString).ToList();

            for (var index = 0; index < naturområder.Count; index++)
                naturområder[index] = naturområder[index].Trim(TrimChars);

            regel.Naturområder = naturområder.Count > 0 ? naturområder : null;
        }

        private static void CreateSqlStringForRegel(Sql regelSql)
        {
            regelSql.QueryString = "SELECT l_g.localid FROM " + string.Join(",", regelSql.From);

            regelSql.QueryString += " WHERE " + string.Join(" AND ", regelSql.Where);
        }

        public static void GetPredecessors(List<string> natursystem)
        {
            var select =
                $"SELECT ch.predecessor FROM data.codeshierarchy ch WHERE ch.successor = '{natursystem[0]}' AND (ch.predecessor like '%-E-%' OR ch.predecessor like '%-C-%')";

            natursystem.AddRange(Select(select).ToList());

            if (natursystem.Count == 1) Console.WriteLine($"WARNING: No predecessor(s) found for {natursystem[0]}");
        }

        public static void SetConnString(string configFile)
        {
            dynamic config = JsonConvert.DeserializeObject(File.ReadAllText(configFile));

            ConnString = $"Host={config.host};Username={config.user};Password={config.pass};Database={config.db}";
        }
    }
}