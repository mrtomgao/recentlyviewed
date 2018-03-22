using Dapper;
using ExtronWeb.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExtronWeb.Models
{
    public class Article        
    {
        public partial class ArticleEntity
        {
            [Key]
            public int PKArticleID;
            public string Title;
            public string SubTitle;
            public string ArticleHandle;
            public int FKContentTypeID;
            public int DealerOnly;
            public int Tab;
            public string Body;
            public int NoTitle;
            public string SEOTitle;
            public string MetaDescription;
            public string MetaKeywords;
            public string EnglishTitle;
            public string ContentTypeDescription;
        }

        public ArticleEntity GetArticle(string articleHandle, Insider.InsiderEntity login)
        {
            using (var db = DBConn.Open())
            {
                var param = new DynamicParameters();
                param.Add("@ArticleHandle", articleHandle);
                param.Add("@Server", "Dev");
                param.Add("@Lang", CurrentUser.Language);
                param.Add("@RegionID", CurrentUser.UserRegion);
                ArticleEntity thisArticle= db.Query<ArticleEntity>("Qry_F_Article", param, commandType: CommandType.StoredProcedure).FirstOrDefault();
                if (thisArticle != null)
                {
                    thisArticle.Body = FilterRegionSpecificContent(thisArticle.Body);
                    thisArticle.Body = FilterLoginSpecificContent(thisArticle.Body, login);
                }
                return (thisArticle);
            }
        }
        private static string AccountType(Insider.InsiderEntity insider)
        {
            Constants.LOGIN type = insider.GetLoginType();
            switch (type)
            {
                case Constants.LOGIN.DEALER:
                case Constants.LOGIN.CONSULTANT:
                    if (insider.IsProgrammer())
                    {
                        return "PROGRAMMER";
                    }
                    else {
                        return type.ToString();
                    }
                default:
                    return type.ToString();
            }
        }

        private static string FilterLoginSpecificContent(string body, Insider.InsiderEntity user)
        {
            Regex re = new Regex(@"\[LOGIN_SPECIFIC(?:\s+)(.*?)Type=(.*?)\]([.\s\S]*?)\[\/LOGIN_SPECIFIC\]");
            List<string> List = new List<string>(); 
            MatchCollection matches = re.Matches(body);
            string accountType = AccountType(user);
            bool canViewPricing = false;
            if (user.GetLoginType() != Constants.LOGIN.NONE) {
                canViewPricing = user.CanViewPricing();
            }
            if (matches.Count > 0)
            {
                foreach (Match item in matches)
                {
                    bool loginTypeMatch = false, pricingMatch = false;
                    List<string> tags = item.Groups[2].Value.Split(',').ToList();
                    List<string> others = item.Groups[1].Value.Split(' ').ToList();
                    if (tags.Contains(accountType) || (user.GetLoginType() != Constants.LOGIN.NONE && tags.Contains("ALL")))
                    {
                        loginTypeMatch = true;
                    }
                    if (!(user.GetLoginType() != Constants.LOGIN.NONE && others.Contains("Pricing=True") && canViewPricing == false))
                    {
                        pricingMatch = true;
                    }
                    if (loginTypeMatch && pricingMatch) {
                        body = body.Replace(item.Groups[0].Value, item.Groups[3].Value);
                    }
                    else
                    {
                        body = body.Replace(item.Groups[0].Value, "");
                    }

                }

            }
            return body;
        }
        private static string FilterRegionSpecificContent(string body)
        {
            Regex re = new Regex(@"\[REGION_SPECIFIC(?:\s+)Region=(.*?)\]([.\s\S]*?)\[\/REGION_SPECIFIC\]");
            List<string> List = new List<string>();
            MatchCollection matches = re.Matches(body);
            string userRegion = CurrentUser.UserRegion.ToString();
            if (matches.Count > 0)
            {
                foreach (Match item in matches)
                {
                    List<string> regions = item.Groups[1].Value.Split(',').ToList();
                    if (regions.Contains(userRegion))
                    {
                        body = body.Replace(item.Groups[0].Value, item.Groups[2].Value);
                    }
                    else
                    {
                        body = body.Replace(item.Groups[0].Value, "");
                    }
                }

            }
            return body;
        }
    }
}
