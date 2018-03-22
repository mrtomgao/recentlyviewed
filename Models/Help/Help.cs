using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dapper;

namespace ExtronWeb.Models.Help
{
    public class Help
    {
        public partial class HelpEntity
        {
            [Key]
            public int PKHelpID { get; set; }
            public string Name { get; set; }
            public string HelpHandle { get; set; }
            public Nullable<int> ListOrder { get; set; }
            public String FKFileHandle { get; set; }
        }
        public partial class VersionEntity
        {
            [Key]
            public int PKVersionID { get; set; }
            public int FKHelpID { get; set; }
            public string Version { get; set; }
            public string CreatedBy { get; set; }
            public System.DateTime CreatedOn { get; set; }
        }
        public partial class TopicEntity
        {
            [Key]
            public int PKHelpTopicID { get; set; }
            public int FKHelpContentsID { get; set; }
            public int FKLangID { get; set; }
            public string HelpTopicHandle { get; set; }
            public string Body { get; set; }
            public Nullable<System.DateTime> CreatedOn { get; set; }
            public Nullable<System.DateTime> UpdatedOn { get; set; }
            public string UpdatedBy { get; set; }
            public string Comments { get; set; }
        }
        public partial class TitleEntity
        {
            [Key]
            public int PKHelpTitleID { get; set; }
            public string Title { get; set; }
            public string HelpTitleHandle { get; set; }
            public int FKLangID { get; set; }
            public string CreatedBy { get; set; }
            public System.DateTime CreatedOn { get; set; }
            public string UpdatedBy { get; set; }
            public Nullable<System.DateTime> UpdatedOn { get; set; }
        }
        public partial class ContentsEntity
        {
            [Key]
            public int PKHelpContentsID { get; set; }
            public int FKVersionID { get; set; }
            public int FKHelpTitleID { get; set; }
            public Nullable<int> FKParentContentsID { get; set; }
            public int SortOrder { get; set; }
        }
        public partial class ToCEntity
        {
            public string Title { get; set; }
            public Nullable<int> PKHelpTopicID { get; set; }
            [Key]
            public int PKHelpContentsID { get; set; }
            public int FKHelpTitleID { get; set; }
            public Nullable<int> FKParentContentsID { get; set; }
            public int HierarchyLevel { get; set; }
            public string Sort { get; set; }
        }
        public partial class SearchEntity
        {
            public int RowNum { get; set; }
            public int RankPoints { get; set; }
            public int TypeRank { get; set; }
            public int PKHelpContentsID { get; set; }
            public int PKVersionID { get; set; }
            public string Highlight { get; set; }
            public string Title { get; set; }
            public string HelpName { get; set; }
            public string Version { get; set; }
            public string HelpHandle { get; set; }
        }
        public partial class FeedbackEntity
        {
            [Key]
            public int PKHelpFeedbackID { get; set; }
            public int FKHelpTopicID { get; set; }
            public bool Helpful { get; set; }            
            public string Comment { get; set; }
            public string Email { get; set; }
            public string UserAgent { get; set; }
            public string IPAddress { get; set; }
            public System.DateTime SubmitDate { get; set; }
        }

        public List<HelpEntity> GetIndex()
        {
            using (var db = DBConn.Open())
            {
                var model = db.Query<HelpEntity>("SELECT TOP 12 th.*, thp.[FKFileHandle] AS [File], thp.[ListOrder] FROM [tbl_Help] th LEFT JOIN [tbl_HelpProperties] thp ON th.[PKHelpID] = thp.[FKHelpID] ORDER BY thp.[ListOrder]").ToList();
                return model;
            }    
        }

    }
}