using Dapper;
using ExtronWeb.Models.Demo;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using System;

namespace ExtronWeb.Controllers.Demo
{
    public class DemoController : Controller
    {
        public ActionResult demo1()
        {
            //DEMO 1: STANDARD SQL QUERY PULL TO A (STRONGLY TYPED MODEL "HelpDemo")
            using (var db = DBConn.Open())
            {
                //DB(RESULT SET COL NAMES) MUST MATCH MODEL(PROPERTY NAMES) 
                //i.e. I returned [FKFileHandle] as [File] so it pairs w/ the model prop name [File].
                //Otherwise it maps as null!!!!!
                var model = db.Query<HelpDemo>("Select tbl_Help.*, [FKFileHandle] as [File],[ListOrder] from tbl_Help LEFT JOIN tbl_HelpProperties on PKHelpID = FKHelpID").ToList();
                return View(model);
            }
        }

        public ActionResult demo2()
        {
            //DEMO 2: SPROC W/ PARAMETERS EXAMPLE TO A (STRONGLY TYPED MODEL "Sprockly")
            using (var db = DBConn.Open())
            {
                var param = new DynamicParameters();
                param.Add("@VersionID", 2);
                param.Add("@FKLangID", 1);
                var model = db.Query<Sprockly>("Qry_F_HelpVersionFlat", param, commandType: CommandType.StoredProcedure).ToList();
                return View(model);
            }
        }

        public ActionResult demo3()
        {
            //DEMO 3: STANDARD PULL TO AN EXPANDO OBJECT (NO MODEL, DYNAMICALLY CREATED)
            //AKA "THE FELI WAY"
            using (var db = DBConn.Open())
            {
                dynamic model = new ExpandoObject();
                model.HelpDocs = db.Query("Select Top 5 tbl_Help.*, [FKFileHandle] as [File],[ListOrder] from tbl_Help LEFT JOIN tbl_HelpProperties on PKHelpID = FKHelpID").ToList();
                return View(model);
            }
        }

        public ActionResult demo4()
        {
            //DEMO 4: MULTI MAPPING HIERARCHICAL OBJECTS USING 'SPLITON' 
            //AKA "THE MORE LEGIT WAY"
            //RETURN TYPE IS VIEWMODEL 'MULTIMAPPING'. Children 'firstObj,secondObj' are defined within.

            using (var db = DBConn.Open())
            {
                var model = db.Query<Multimapping.VerMMdemo, Multimapping.HelpMMdemo, Multimapping>(
                    "SELECT TOP 1 thv.*, th.* FROM tbl_HelpVersion thv LEFT JOIN tbl_Help th on th.PKHelpID = thv.FKHelpID where th.HelpHandle = 'GlobalScripter'",
                    (thv, th) =>
                    {
                        return new Multimapping { firstObj = thv, secondObj = th };
                    },
                    splitOn: "PKHelpID").First();

                return View(model);
            }

        }
        public ActionResult demo5()
        {
            //DEMO 5: MULTI MAPPING ANY OBJECT (FLAT LEVEL) WITH 'TUPLE' & 'SPLITON' 
            //AKA "THE LESS LEGIT WAY" (Can pass any typed objs from different models)

            //(NO NEED FOR A VIEWMODEL, AS THE RETURN TYPE IS A 'TUPLED' OBJ)            
            //(DOWNFALL IS THAT TUPLE WILL DEFAULT MAPPED OBJ NAMES TO 'ITEM1... ITEM2...ETC...' SEE VIEW)

            using (var db = DBConn.Open())
            {
                var model = db.Query<
                    Multimapping.VerMMdemo,
                    HelpDemo,
                    ExtronWeb.Models.Help.Help.HelpEntity,
                    Tuple<
                        Multimapping.VerMMdemo,
                        HelpDemo,
                        ExtronWeb.Models.Help.Help.HelpEntity>>(
                    "SELECT TOP 1 thv.*, th.*, thp.FKHelpID as PKPropID, thp.FKFileHandle, thp.ListOrder FROM tbl_HelpVersion thv LEFT JOIN tbl_Help th on th.PKHelpID = thv.FKHelpID LEFT JOIN tbl_HelpProperties thp on thp.FKHelpID = th.PKHelpID where th.HelpHandle = 'GlobalScripter'",
                    (thv, th, thp) => Tuple.Create(thv, th, thp),
                    splitOn: "PKHelpID,PKPropID").First();

                return View(model);
            }

        }
    }
}