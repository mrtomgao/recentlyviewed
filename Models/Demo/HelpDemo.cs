using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExtronWeb.Models.Demo
{
    public class HelpDemo
    {
        public int PKHelpID { get; set; }
        public string Name { get; set; }
        public string HelpHandle { get; set; }
        public Nullable<int> ListOrder { get; set; }
        public String File { get; set; }

    }
}