using System.Collections.Generic;
using System.Numerics;

namespace Rødliste
{
    internal class Regel
    {
        public List<string> Beskrivelsesvariabler;
        public string Natursystem;
        public Sql Sql { get; set; }
        public List<BigInteger> Naturområder { get; set; }

        public static Regel Get(dynamic regel)
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