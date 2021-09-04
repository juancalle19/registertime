using System;
using System.Collections.Generic;
using System.Text;

namespace registertime.Common.Models
{
    public class Registertime
    {
        public int IdEmployee { get; set; }

        public DateTime Time { get; set; }

        public int  Type { get; set; }

        public bool Consolidate { get; set; }
    }
}
