using System.Collections.Generic;

namespace Redlist
{
    internal class Regel
    {
        public List<string> Beskrivelsesvariabler;
        public string Natursystem;
        public Sql Sql { get; set; }
        public List<string> Naturområder { get; set; }

        public static Regel Get(dynamic regel)
        {
            var natursystem = new List<string> {"NA_" + regel.Natursystem};

            if (regel.Natursystem.Contains('-')) Sql.GetPredecessors(natursystem);

            var sql = new Sql
            {
                From = new List<string>
                {
                    "data.localid_geometry l_g",
                    "data.codes_geometry na"
                },
                Where = new List<string>
                {
                    $"na.code IN ('{string.Join("','", natursystem)}')",
                    "l_g.geometry_id = na.geometry_id"
                }
            };

            if (!((IDictionary<string, object>) regel).ContainsKey("BeskrivelsesVariabler"))
                return new Regel
                {
                    Natursystem = regel.Natursystem,
                    Beskrivelsesvariabler = null,
                    Sql = sql
                };

            return new Regel
            {
                Natursystem = regel.Natursystem,
                Beskrivelsesvariabler = GetBeskrivelsesVariabler(regel, sql),
                Sql = sql
            };
        }

        private static List<string> GetBeskrivelsesVariabler(dynamic regel, Sql sql)
        {
            var beskrivelsesVariabler = new List<string>();

            for (var i = 0; i < regel.BeskrivelsesVariabler.Count; i++)
            {
                beskrivelsesVariabler.Add(regel.BeskrivelsesVariabler[i]);

                var prefix = char.IsNumber(regel.BeskrivelsesVariabler[i][0]) ? "BS_" : "MI_";

                sql.From.Add($"data.codes_geometry c_g{i}");
                sql.Where.Add($"c_g{i}.code = '{prefix}{regel.BeskrivelsesVariabler[i]}'");
                sql.Where.Add($"c_g{i}.geometry_id = na.geometry_id");
            }

            return beskrivelsesVariabler;
        }
    }
}