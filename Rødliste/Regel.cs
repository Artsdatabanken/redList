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
    }
}