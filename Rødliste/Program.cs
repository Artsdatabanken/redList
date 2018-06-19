using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Numerics;
using Newtonsoft.Json;
using Npgsql;

namespace Rødliste
{
    class Program
    {
        static void Main(string[] args)
        {
            var temaer = ReadDefinitions();

            var connString = args[0]; //"Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase";

            foreach (var tema in temaer)
            foreach (var vurderingsenhet in tema.VurderingsEnheter)
            foreach (var regel in vurderingsenhet.Regler)
                ExecuteSql(regel, connString);

            var json = JsonConvert.SerializeObject(temaer);

            File.WriteAllText("temaer.json", json);
        }

        private static void ExecuteSql(Regel regel, string connString)
        {
            var sql = CreateSqlStringForRegel(regel.Sql);

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Retrieve all rows
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    regel.Naturområder = new List<BigInteger>();
                    while (reader.Read())
                    {
                        var id = reader.GetInt64(0);
                        regel.Naturområder.Add(id);
                    }
                }
                //// Insert some data
                //using (var cmd = new NpgsqlCommand())
                //{
                //    cmd.Connection = conn;
                //    cmd.CommandText = "INSERT INTO data (some_field) VALUES (@p)";
                //    cmd.Parameters.AddWithValue("p", "Hello world");
                //    cmd.ExecuteNonQuery();
                //}

            }
        }

        private static string CreateSqlStringForRegel(Sql regelSql)
        {
            var sql = "SELECT na.geometry_id FROM " + string.Join(",", regelSql.From).TrimEnd(',');

            sql += " WHERE " + string.Join(" AND ", regelSql.Where).TrimEnd(',');

            return sql;
        }

        private static IEnumerable<Tema> ReadDefinitions()
        {
            var temaer = new List<Tema>();

            dynamic json = JsonConvert.DeserializeObject<List<ExpandoObject>>(
                new WebClient().DownloadString("https://test.artsdatabanken.no/data/json/rv/rv.json"));

            foreach (var vurderingsenheter in json) temaer.Add(GetTema(vurderingsenheter));

            return temaer;
        }

        private static Tema GetTema(dynamic vurderingsenheter)
        {
            var tema = new Tema
            {
                Navn = vurderingsenheter.Tema,
                VurderingsEnheter = new List<Vurderingsenhet>()
            };

            foreach (var vurderingsenhet in vurderingsenheter.Vurderingsenheter)
                tema.VurderingsEnheter.Add(GetVurderingsEnhet(vurderingsenhet));

            return tema;
        }

        private static Vurderingsenhet GetVurderingsEnhet(dynamic vurderingsenhet)
        {
            var vurderingsEnhet = new Vurderingsenhet
            {
                Navn = vurderingsenhet.Navn,
                Rødlistekategori = vurderingsenhet.Rødlistekategori,
                Regler = new List<Regel>()
            };

            foreach (var regel in vurderingsenhet.Regler) vurderingsEnhet.Regler.Add(GetRegel(regel));

            return vurderingsEnhet;
        }

        private static Regel GetRegel(dynamic regel)
        {
            var sql = new Sql
            {
                From = new List<string>
                {
                    "data.codes_geometry na"
                },
                Where = new List<string>
                {
                    "na.code = 'NA_" + regel.Natursystem + "'"
                }
            };

            if (!((IDictionary<string, object>) regel).ContainsKey("BeskrivelsesVariabler"))
                return new Regel
                {
                    Natursystem = regel.Natursystem,
                    Beskrivelsesvariabler = null,
                    Sql = sql
                };

            List<string> beskrivelsesVariabler = GetBeskrivelsesVariabler(regel, sql);

            return new Regel
            {
                Natursystem = regel.Natursystem,
                Beskrivelsesvariabler = beskrivelsesVariabler,
                Sql = sql
            };
        }

        private static List<string> GetBeskrivelsesVariabler(dynamic regel, Sql sql)
        {
            var beskrivelsesVariabler = new List<string>();

            for (var i = 0; i < regel.BeskrivelsesVariabler.Count; i++)
            {
                beskrivelsesVariabler.Add(regel.BeskrivelsesVariabler[i]);
                sql.From.Add("data.codes_geometry c_g" + i);
                sql.Where.Add("c_g" + i + ".code LIKE '%_" + regel.BeskrivelsesVariabler[i] + "'");
                sql.Where.Add("c_g" + i + ".geometry_id = na.geometry_id");
            }

            return beskrivelsesVariabler;
        }
    }
}