using System.Collections.Generic;
using Dapper;
using System.Linq;
using System.Data;
namespace ExtronWeb.Models.Help
{
    public class SearchViewModel
    {
        public Help.VersionEntity SelectedVersion { get; set; }
        public Help.HelpEntity SelectedHelp { get; set; }
        public List<Help.SearchEntity> Results { get; set; }

        public bool Get(string keyword, int? version)
        {
            using (var db = DBConn.Open())
            {
                if (version != null)
                {

                db.Query<Help.VersionEntity, Help.HelpEntity, bool>(
                    "SELECT * FROM tbl_HelpVersion thv LEFT JOIN tbl_Help th ON th.PKHelpID = thv.FKHelpID LEFT JOIN tbl_HelpProperties thp ON thp.FKHelpID = th.PKHelpID where PKVersionID = @VersionID",
                    (thv,th) =>
                    {
                        SelectedVersion = thv;
                        SelectedHelp = th;
                        return true;
                    },
                    new { VersionID = version },
                    splitOn: "PKHelpID").FirstOrDefault();
                }

                if (SelectedVersion == null)
                {
                    Results = db.Query<Help.SearchEntity>("Qry_F_HelpSearch",
                    new { searchTerm = keyword},
                    commandType: CommandType.StoredProcedure).ToList();
                }
                else
                {
                    Results = db.Query<Help.SearchEntity>("Qry_F_HelpSearch",
                    new { searchTerm = keyword , versionID = SelectedVersion.PKVersionID},
                    commandType: CommandType.StoredProcedure).ToList();
                }
            }

            if (Results != null)
            {
                return true;
            }
            return false;
        }
    }

    
}