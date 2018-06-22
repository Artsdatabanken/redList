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
                    switch (reader.GetDataTypeName(0))
                    {
                        case "text":
                            while (reader.Read()) yield return reader.GetString(0);
                            break;
                        default:
                            while (reader.Read()) yield return reader.GetInt64(0).ToString();
                            break;
                    }
                }
            }
        }

        private static void Insert(string queryString)
        {
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = queryString;
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public static void GetNaturområder(Regel regel, string vurderingsenhetRødlistekategori)
        {
            CreateSqlStringForRegel(regel.Sql);

            var naturområder = new List<string>();

            Select(regel.Sql.QueryString).ToList()
                .ForEach(n => naturområder.Add(n.Trim(TrimChars)));

            regel.Naturområder = naturområder.Count > 0 ? naturområder : null;

            regel.Naturområder?.ForEach(localid => InsertCodes(localid, "RL_" + vurderingsenhetRødlistekategori));
        }

        private static void InsertCodes(string localid, string vurderingsenhetRødlistekategori)
        {
            // TODO: Make this more elegant
            var codesId = Select($"SELECT id as codes_id FROM data.codes where code = '{vurderingsenhetRødlistekategori}'").First();
            var geometryId = Select($"SELECT geometry_id FROM data.localid_geometry where localid = '{{{localid}}}'").First();

            if (Select(
                    $"SELECT geometry_id FROM data.codes_geometry c_g WHERE codes_id = {codesId} AND geometry_id = {geometryId}")
                .Any()) return;

            var insertString = $"INSERT INTO data.codes_geometry (codes_id, geometry_id, code) values ({codesId},{geometryId},'{vurderingsenhetRødlistekategori}')";
            
            Insert(insertString);
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