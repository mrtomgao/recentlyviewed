using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExtronWeb.Models
{
    public class WhatsHappening
    {
        private const int _count = 4;

        public string ArticleHandle { get; set; }
        public string Body { get; set; }
        public int FKContentTypeID { get; set; }
        public string Description { get; set; }
        public string Img { get; set; }
        public string Title { get; set; }

        public static List<WhatsHappening> GetWhatsHappening()
        {
            using (var db = DBConn.Open())
            {
                var param = new DynamicParameters();
                param.Add("@Count", _count);
                param.Add("@RegionID", CurrentUser.UserRegion);
                param.Add("@LangID", CurrentUser.Language);
                var model = db.Query<WhatsHappening>
                (@"SELECT TOP (@Count) COALESCE(trans.Title, a.Title) AS Title, a.ArticleHandle, t.Description, FKContentTypeID, COALESCE(trans.Body, a.Body) As Body
                    FROM tbl_Article AS a
                    INNER JOIN tbl_WebRegionArticles_R AS r ON a.PKArticleID = r.FKArticleID
                    INNER JOIN tbl_ArticleSharedProperties AS asp ON a.ArticleHandle = asp.ArticleHandle
                    INNER JOIN tbl_ContentType AS t ON asp.FKContentTypeID = t.PKContentTypeID
                    LEFT OUTER JOIN
                    (--get translation of article title
                            SELECT a1.ArticleHandle, a1.Title, a1.Body
                            FROM tbl_Article AS a1
                            INNER JOIN tbl_WebRegionArticles_R AS r1 ON a1.PKArticleID = r1.FKArticleID
                            WHERE a1.FKLangID = @LangID AND a1.FKStatusID IN (2, 3)
                            AND r1.FKRegionID IN (@RegionID, 9)
                    ) AS trans ON trans.ArticleHandle = a.ArticleHandle
                    WHERE a.FKLangID = 1 AND a.FKStatusID IN (2, 3)
                    AND r.FKRegionID IN (@RegionID, 9)
                    AND t.PKContentTypeID IN (4, 21, 35)
                    ORDER BY a.AddDate DESC, a.Title", param, commandType: CommandType.Text).ToList();

                foreach (var item in model)
                {
                    item.Img =  "http://dev.extron.com/img/s3-logo.gif";
                    string firstImg = Regex.Match(item.Body, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
                    if (firstImg.IndexOf("http://") > -1 || firstImg.IndexOf("https://") > -1) {
                        firstImg = (firstImg.Replace("http://", "").Replace("https://", ""));
                        firstImg = firstImg.Substring(firstImg.IndexOf("/"), firstImg.Length - firstImg.IndexOf("/"));
                    }
                    if (firstImg != "")
                    {
                        item.Img = "http://dev.extron.com" + firstImg;
                    }
                }
                return model;
            }
        }
    }
}