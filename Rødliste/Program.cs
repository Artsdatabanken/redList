using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace Rødliste
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteJson(GetRedlist(args));
        }

        private static List<Tema> GetRedlist(IReadOnlyList<string> args)
        {
            var definitions = ReadDefinitions();

            var redList = definitions.ToList();
            foreach (var tema in redList)
                foreach (var vurderingsenhet in tema.VurderingsEnheter)
                    foreach (var regel in vurderingsenhet.Regler)
                        Sql.Execute(regel, args[0]);

            return redList;
        }

        private static void WriteJson(List<Tema> redList)
        {
            CleanJson(redList);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented
            };


            var json = JsonConvert.SerializeObject(redList, settings);

            File.WriteAllText("redList.json", json);
        }

        private static void CleanJson(List<Tema> redList)
        {
            foreach (var tema in redList)
            {
                foreach (var vurderingsenhet in tema.VurderingsEnheter)
                {
                    foreach (var regel in vurderingsenhet.Regler)
                    {
                        regel.Sql = null;
                    }
                    vurderingsenhet.Regler.RemoveAll(r => r.Naturområder == null);
                }

                tema.VurderingsEnheter.RemoveAll(v => v.Regler.Count == 0);
            }

            redList.RemoveAll(t => t.VurderingsEnheter.Count == 0);
        }

        private static IEnumerable<Tema> ReadDefinitions()
        {
            var definitions = new List<Tema>();

            dynamic json = JsonConvert.DeserializeObject<List<ExpandoObject>>(
                new WebClient().DownloadString("https://test.artsdatabanken.no/data/json/rv/rv.json"));

            foreach (var vurderingsenheter in json) definitions.Add(Tema.Get(vurderingsenheter));

            return definitions;
        }
    }
}