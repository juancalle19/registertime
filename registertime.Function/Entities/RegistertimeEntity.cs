using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace registertime.Function.Entities
{
    public class RegistertimeEntity : TableEntity
    {
        public int IdEmployee { get; set; }

        public DateTime Time { get; set; }

        public int Type { get; set; }

        public bool Consolidate { get; set; }
    }
}
