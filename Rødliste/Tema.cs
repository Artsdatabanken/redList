using System.Collections.Generic;

namespace Rødliste
{
    internal class Tema
    {
        public string Navn;
        public List<Vurderingsenhet> VurderingsEnheter;

        public static Tema Get(dynamic vurderingsenheter)
        {
            var tema = new Tema
            {
                Navn = vurderingsenheter.Tema,
                VurderingsEnheter = new List<Vurderingsenhet>()
            };

            foreach (var vurderingsenhet in vurderingsenheter.Vurderingsenheter)
                tema.VurderingsEnheter.Add(Vurderingsenhet.Get(vurderingsenhet));

            return tema;
        }
    }
}