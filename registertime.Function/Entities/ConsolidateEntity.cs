using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace registertime.Function.Entities
{
    public class ConsolidateEntity : TableEntity
    {
        public int IdEmployee { get; set; }

        public DateTime InTime { get; set; }

        public int minutes { get; set; }
    }
}
