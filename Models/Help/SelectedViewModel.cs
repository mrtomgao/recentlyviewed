using System.Collections.Generic;
using Dapper;
using System.Linq;
using System.Data;

namespace ExtronWeb.Models.Help
{
    public class SelectedViewModel
    {        
        public Help.HelpEntity SelectedHelp { get; set; }
        public Help.VersionEntity SelectedVersion { get; set; }      
        public List<Help.ToCEntity> OuterTOC { get; set; }
        public List<Help.VersionEntity> OtherVersions { get; set; }

        public bool Get(string helpHandle, int? version)
        {
            using (var db = DBConn.Open())
            {
                //Set SelectedHelp Property
                SelectedHelp = db.Query<Help.HelpEntity>(
                    "SELECT * FROM tbl_Help th LEFT JOIN tbl_HelpProperties thp on th.PKHelpID = thp.FKHelpID WHERE th.HelpHandle = @HelpHandle",
                    new { HelpHandle = helpHandle }).FirstOrDefault();

                //Set SelectedVersion Property
                if (SelectedHelp != null)
                {
                    var param = new DynamicParameters();
                    param.Add("@HelpID", SelectedHelp.PKHelpID);
                    var query = "SELECT TOP 1 * FROM tbl_HelpVersion thv where thv.FKHelpID = @HelpID";
                    if (version != null)
                    {
                        param.Add("@VersionID", version);
                        query += " AND thv.PKVersionID = @VersionID";
                    }
                    SelectedVersion = db.Query<Help.VersionEntity>(
                        query + " ORDER BY ListOrder DESC, CreatedOn",
                        param).FirstOrDefault();
                }
               
                //Set OtherVersions Property
                if (SelectedVersion != null)
                {
                    OtherVersions = db.Query<Help.VersionEntity>(
                        "SELECT * FROM tbl_HelpVersion WHERE FKHelpID = @HelpID ORDER BY ListOrder DESC",
                        new { HelpID = SelectedHelp.PKHelpID }).ToList();

                    //Set TOC Property
                    OuterTOC = db.Query<Help.ToCEntity>(
                        "Qry_F_HelpGetTOC",
                        new { VersionID = SelectedVersion.PKVersionID },
                        commandType: CommandType.StoredProcedure).ToList();

                    return true;
                }
                return false;
            }
        }

    }


}