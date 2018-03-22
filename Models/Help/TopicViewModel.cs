using System;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using System.Data;

namespace ExtronWeb.Models.Help
{
    public class TopicViewModel
    {        
        public Help.TopicEntity SelectedTopic { get; set; }
        public Help.ContentsEntity SelectedContents { get; set; }
        public Help.TitleEntity SelectedTitle { get; set; }
        public Help.TitleEntity ParentTitle { get; set; }
        public Help.VersionEntity SelectedVersion { get; set; }
        public Help.HelpEntity SelectedHelp { get; set; }
        public Help.FeedbackEntity Feedback { get; set; }
        public List<Help.ToCEntity> InnerTOC { get; set; }

        public bool Get(int? contentsID)
        {
            using (var db = DBConn.Open())
            {
                db.Query<Help.TopicEntity, Help.ContentsEntity, Help.TitleEntity, Help.TitleEntity, Help.VersionEntity, Help.HelpEntity, bool>(
                    "Qry_F_HelpGetTopic",
                    (tpc,thc,prt,tht,thv,th) =>
                    {
                        SelectedTopic = tpc;
                        SelectedContents = thc;
                        ParentTitle = prt; 
                        SelectedTitle = tht;
                        SelectedVersion = thv;
                        SelectedHelp = th;                        
                        return true;
                    },
                    new { ContentsID = contentsID },
                    commandType: CommandType.StoredProcedure,                    
                    splitOn: "PKHelpContentsID,PKHelpTitleID,PKHelpTitleID,PKVersionID,PKHelpID").FirstOrDefault();
                    
                    //Try for FIRST CHILD if Selected was an empty hierarchical line item
                    if (SelectedTopic == null)
                    {
                        db.Query<Help.TopicEntity, Help.ContentsEntity, Help.TitleEntity, Help.TitleEntity, Help.VersionEntity, Help.HelpEntity, bool>(
                            "Qry_F_HelpGetTopicFirstChild",
                            (tpc, thc, prt, tht, thv, th) =>
                            {
                                SelectedTopic = tpc;
                                SelectedContents = thc;
                                ParentTitle = prt;
                                SelectedTitle = tht;
                                SelectedVersion = thv;
                                SelectedHelp = th;
                                return true;
                            },
                            new { ContentsID = contentsID },
                            commandType: CommandType.StoredProcedure,
                            splitOn: "PKHelpContentsID,PKHelpTitleID,PKHelpTitleID,PKVersionID,PKHelpID").FirstOrDefault();
                    }

                if (SelectedTopic != null)
                {
                    //Determine if InnerTOC should be loaded from DB or pulled from TempData
                    bool loadFromDB = false;

                    if (InnerTOC == null)
                    {
                        loadFromDB = true;
                    }
                    else
                    {
                        //TempData found, however its from a non-matching ToC
                        if (InnerTOC.Find(x => x.PKHelpContentsID == SelectedContents.PKHelpContentsID) == null)
                        {
                            loadFromDB = true;
                        }
                        else //Matching TempData found, let the DB take a break.
                        {
                            loadFromDB = false;
                        }
                    }

                    if (loadFromDB == true)
                    {
                        InnerTOC = db.Query<Help.ToCEntity>(
                            "Qry_F_HelpGetTOC",
                            new { VersionID = SelectedVersion.PKVersionID },
                            commandType: CommandType.StoredProcedure).ToList();
                    }

                    //Remove unapplicable hierarchy items, keep ony parents and siblings.
                    InnerTOC.RemoveAll(c => c.HierarchyLevel == 0 && c.Sort.Substring(0, 1) != InnerTOC.Find(x => x.PKHelpContentsID == SelectedContents.PKHelpContentsID).Sort.Substring(0, 1));
                    InnerTOC.RemoveAll(s => s.Sort.Substring(0, 1) != InnerTOC.Find(c => c.PKHelpContentsID == SelectedContents.PKHelpContentsID).Sort.Substring(0, 1));

                    //if (loadFromDB == true)
                    //{
                    //    InnerTOC[0].Title = InnerTOC[0].Title + " *DB";
                    //}
                    //else
                    //{
                    //    InnerTOC[0].Title = InnerTOC[0].Title + " *Cached";
                    //}

                    // success
                    return true;
                }
                return false;           
            }
        }
        public void SetInnerTOCFromTemp(List<Help.ToCEntity> TOC)
        {
            if (TOC != null)
            {                
                InnerTOC = TOC;
            }
        }

        public Boolean SubmitTopicFeedback(Help.FeedbackEntity feedback)
        {
            using (var db = DBConn.Open())
            {
                var i = db.Execute("INSERT INTO [tbl_HelpFeedback] (FKHelpTopicID, Helpful, Comment, Email, UserAgent, IPAddress, SubmitDate) VALUES (@FKHelpTopicID, @Helpful, @Comment, @Email, @UserAgent, @IpAddress, CURRENT_TIMESTAMP)",
                    new {
                        FKHelpTopicID = feedback.FKHelpTopicID,
                        Helpful = feedback.Helpful,
                        Comment = feedback.Comment,
                        Email = feedback.Email,
                        UserAgent = feedback.UserAgent,
                        IpAddress = feedback.IPAddress
                    });

                if (i != 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}