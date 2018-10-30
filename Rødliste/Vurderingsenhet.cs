using System.Collections.Generic;

namespace Redlist
{
    internal class Vurderingsenhet
    {
        public string Navn;
        public List<Regel> Regler;
        public string Rødlistekategori;

        public static Vurderingsenhet Get(dynamic vurderingsenhet)
        {
            var vurderingsEnhet = new Vurderingsenhet
            {
                Navn = vurderingsenhet.Navn,
                Rødlistekategori = vurderingsenhet.Rødlistekategori,
                Regler = new List<Regel>()
            };

            foreach (var regel in vurderingsenhet.Regler) vurderingsEnhet.Regler.Add(Regel.Get(regel));

            return vurderingsEnhet;
        }
    }
}