using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ExtronWeb.Models.Help;

namespace ExtronWeb.Models.Demo
{
    public class Multimapping
    {

        public VerMMdemo firstObj { get; set; }
        public HelpMMdemo secondObj { get; set; }


        public partial class HelpMMdemo
        {
            [Key]
            public int PKHelpID { get; set; }
            public string Name { get; set; }
            public string HelpHandle { get; set; }
            public Nullable<int> ListOrder { get; set; }
            public String File { get; set; }
        }
        public partial class VerMMdemo
        {
            [Key]
            public int PKVersionID { get; set; }
            public int FKHelpID { get; set; }
            public string Version { get; set; }
            public string CreatedBy { get; set; }
            public System.DateTime CreatedOn { get; set; }
        }
    }
}