using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace Rødliste
{
    class Program
    {
        static void Main(string[] args)
        {
            var temaer = ReadDefinitions();

            var connString = CreateConnectionstring(args[0]);

            foreach (var tema in temaer)
            foreach (var vurderingsenhet in tema.VurderingsEnheter)
            foreach (var regel in vurderingsenhet.Regler)
                Sql.Execute(regel, connString);

            var json = JsonConvert.SerializeObject(temaer);

            File.WriteAllText("temaer.json", json);
        }

        private static string CreateConnectionstring(string configFile)
        {
            dynamic config = JsonConvert.DeserializeObject(File.ReadAllText(configFile));
            return $"Host={config.host};Username={config.user};Password={config.pass};Database={config.db}";
        }

        private static IEnumerable<Tema> ReadDefinitions()
        {
            var temaer = new List<Tema>();

            dynamic json = JsonConvert.DeserializeObject<List<ExpandoObject>>(
                new WebClient().DownloadString("https://test.artsdatabanken.no/data/json/rv/rv.json"));

            foreach (var vurderingsenheter in json) temaer.Add(Tema.Get(vurderingsenheter));

            return temaer;
        }
    }
}