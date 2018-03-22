using Dapper;
using ExtronWeb.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ExtronWeb.Models
{
    public class CurrentUser
    {
        public static Constants.LANG Language
        {
            get
            {
                Constants.LANG defaultLang = Constants.LANG.ENGLISH;
                if (HttpContext.Current.Request.Cookies["lang"] == null) { SetDefaultRegionLang(); }
                if (Enum.TryParse(HttpContext.Current.Request.Cookies["lang"].Value, out defaultLang))
                {
                    return defaultLang;
                }
                else
                {
                    
                    return defaultLang;
                }
            }
            set
            {
                WriteCookies("lang", ((int)value).ToString());
            }
        }

        public static Constants.USER_REGION UserRegion
        {
            get
            {
                Constants.USER_REGION defaultRegion = Constants.USER_REGION.USA;
                if (HttpContext.Current.Request.Cookies["region"] == null) { SetDefaultRegionLang(); }
                if (Enum.TryParse(HttpContext.Current.Request.Cookies["region"].Value, out defaultRegion))
                {
                    return defaultRegion;
                }
                else
                {
                    return defaultRegion;
                }
            }
            set { WriteCookies("region", ((int)value).ToString()); }
        }

        public static void WriteCookies(string cookieName, string cookieValue, bool isPersistent = true)
        {
            HttpContext.Current.Response.Cookies[cookieName].Value = cookieValue;
            if (isPersistent)
                HttpContext.Current.Response.Cookies[cookieName].Expires = DateTime.Now.AddYears(10);
        }

        public static string GetExtronContactPhone()
        {
            if (UserRegion == Constants.USER_REGION.AUSTRALIA)
            {
                return "1800.EXTRON";
            }
            
            switch (UserRegion)
            {
                case Constants.USER_REGION.MIDDLE_EAST: return "+971.4.299.1800";
                case Constants.USER_REGION.EUROPE: return "+800.3987.6673";
                case Constants.USER_REGION.ASIA: return "+65.6383.4400";
                case Constants.USER_REGION.JAPAN: return "(03) 3511-7655";
                case Constants.USER_REGION.CHINA: return "4000.398766";
                default: return "800.633.9876";
            }
        }

        public static Dictionary<string, string> GetRegionLangSelection()
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            var insider = Insider.GetCurrentInsider();
            bool restrictOptions = true;
            if (insider.Email == "marcomm@extron.com" || insider.GetLoginType() == Constants.LOGIN.NONE)
            {
                restrictOptions = false;
            }
            options.Add("1,1", "US / The Americas - English");
            options.Add("1,5", HttpUtility.HtmlDecode("US / The Americas - Espa&#241;ol"));
            options.Add("1,2", HttpUtility.HtmlDecode("US / The Americas - Fran&#231;ais"));

            options.Add("7,1", "UK & Ireland - English");

            options.Add("6,1", "Africa - English");
            options.Add("6,2", HttpUtility.HtmlDecode("Africa - Fran&#231;ais"));

            options.Add("2,3", "Europe - Deutsch");
            options.Add("2,1", "Europe - English");
            options.Add("2,5", HttpUtility.HtmlDecode("Europe - Espa&#241;ol"));
            options.Add("2,2", HttpUtility.HtmlDecode("Europe - Fran&#231;ais"));
            options.Add("2,4", "Europe - Italiano");

            options.Add("8,1", "Middle East - English");

            options.Add("9,11", HttpUtility.HtmlDecode("Russia - Russian (&#1056;&#1091;&#1089;&#1089;&#1082;&#1080;&#1081;)"));
            options.Add("9,1", "Russia - English");

            options.Add("5,7", HttpUtility.HtmlDecode("China - Chinese (&#31616;&#20307;&#20013;&#25991;)"));
            options.Add("5,1", "China - English");

            options.Add("3,1", "Asia / Pacific - English");
            options.Add("3,9", HttpUtility.HtmlDecode("Asia / Pacific - Korean (&#54620;&#44397;&#50612;)"));
            options.Add("3,10", HttpUtility.HtmlDecode("Asia / Pacific - Thai (&#3652;&#3607;&#3618;)"));

            options.Add("10,1", "Australia - English");

            options.Add("4,6", HttpUtility.HtmlDecode("Japan - Japanese (&#26085;&#26412;&#35486;)"));
            options.Add("4,1", "Japan - English");

            if (restrictOptions)
            {
                var regionOptions = options.Where(s => s.Key.Contains(((int)UserRegion).ToString() + ","))
                                           .ToDictionary(dict => dict.Key, dict => dict.Value);
                return regionOptions;
            }
            else
            {
                return options;
            }
        }

        public static Constants.LANG GetUserLanguageByAvantePref(string avanteLang)
        {
            switch (avanteLang)
            {
                case "01": return Constants.LANG.ENGLISH; 
                case "03": return Constants.LANG.SPANISH; 
                case "04": return Constants.LANG.FRENCH; 
                case "05": return Constants.LANG.JAPANESE; 
                case "06": return Constants.LANG.GERMAN; 
                case "07": return Constants.LANG.CHINESE; 
                case "08": return Constants.LANG.KOREAN; 
                case "09": return Constants.LANG.ITALIAN; 
                default: return Constants.LANG.ENGLISH; 
            }
        }

        public static Constants.USER_REGION GetUserRegionByTerritory(string terr)
        {
            Constants.USER_REGION retVal = default(Constants.USER_REGION);
            switch (terr)
            {
                case "AFR":
                    return Constants.USER_REGION.AFRICA;
                case "AUS":
                    return Constants.USER_REGION.AUSTRALIA;
                case "CHN":
                    return Constants.USER_REGION.CHINA;
                case "ERU":
                    return Constants.USER_REGION.RUSSIA;
                case "JPN":
                    return Constants.USER_REGION.JAPAN;
                case "MIDE":
                    return Constants.USER_REGION.MIDDLE_EAST;
                case "UKI":
                    return Constants.USER_REGION.UK;
                default:
                    if (Functions.TerritoryInAsia(terr))
                        return Constants.USER_REGION.ASIA;
                    if (Functions.TerritoryInUnitedStates(terr) || Functions.TerritoryInCanada(terr) || Functions.TerritoryInLatinAmerica(terr))
                        return Constants.USER_REGION.USA;
                    if (Functions.TerritoryInEurope(terr))
                        return Constants.USER_REGION.EUROPE;
                    break;
            }
            return retVal;
        }

        private static Constants.LANG GetLanguageByISOCode(string isoCode)
        {
            // Convert ISO 639-1 (2-char) or 639-2 (3-char) language code
            switch (isoCode)
            {
                case "ar":
                case "ara":
                    return Constants.LANG.ARABIC;
                case "de":
                case "deu":
                    return Constants.LANG.GERMAN;
                case "en":
                case "eng":
                    return Constants.LANG.ENGLISH;
                case "es":
                case "spa":
                    return Constants.LANG.SPANISH;
                case "fr":
                case "fra":
                    return Constants.LANG.FRENCH;
                case "it":
                case "ita":
                    return Constants.LANG.ITALIAN;
                case "ja":
                case "jpn":
                    return Constants.LANG.JAPANESE;
                case "ko":
                case "kor":
                    return Constants.LANG.KOREAN;
                case "ru":
                case "rus":
                    return Constants.LANG.RUSSIAN;
                case "th":
                case "tha":
                    return Constants.LANG.THAI;
                case "vi":
                case "vie":
                    return Constants.LANG.VIETNAMESE;
                case "zh":
                case "zho":
                    return Constants.LANG.CHINESE;
                default:
                    return Constants.LANG.ENGLISH;
            }
        }

        private static bool IsUserAgentAcceptable
        {
            get
            {
                string userAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
                if (string.IsNullOrEmpty(userAgent))
                    return false;
                if (userAgent.IndexOf("bot") >= 0)
                    return false;
                if (userAgent.IndexOf("spider") > 0)
                    return false;
                if (userAgent.IndexOf("crawler") > 0)
                    return false;
                if (userAgent.IndexOf("fetch") > 0)
                    return false;
                return true;
            }
        }

        public static void SetDefaultRegionLang()
        {
            Constants.USER_REGION region = Constants.USER_REGION.USA;
            Constants.LANG lang = Constants.LANG.ENGLISH;
            //DomainMappings domainList = new DomainMappings();
            //if (domainList.Match)
            //{
            //    region = domainList.UserRegion;
            //    lang = domainList.Lang;
            //}
            //else
            //{
            string countryCode = "";
            string langCode = "";
            string acceptLang = HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"];
            int hyphenPos = acceptLang.IndexOf('-');
            if (hyphenPos >= 2 && acceptLang.Substring(hyphenPos + 1).Length >= 2)
            {
                countryCode = acceptLang.Substring(hyphenPos + 1, 2);
                langCode = acceptLang.Substring(hyphenPos - 2, 2);
            }
            if (!string.IsNullOrEmpty(countryCode) && countryCode != "US" && countryCode != "CA" && IsUserAgentAcceptable)
            {
                using (var db = DBConn.Open())
                {
                    var param = new DynamicParameters();
                    param.Add("@CountryCode", countryCode);
                    string terr = db.QueryFirst("SELECT TerritoryID FROM tbl_Countries_L WHERE PKCountryID = @CountryCode", param, commandType: CommandType.Text);
                    if (!string.IsNullOrEmpty(terr)) region = GetUserRegionByTerritory(terr);                    
                }
            }
            if (!string.IsNullOrEmpty(langCode) && langCode != "en")
            {
                lang = GetLanguageByISOCode(langCode);
                if (!ValidLanguageForUserRegion(lang, region))
                    lang = Constants.LANG.ENGLISH;
            }
            //}
            WriteCookies("region", ((int)region).ToString());
            WriteCookies("lang", ((int)lang).ToString());
        }

        private static bool ValidLanguageForUserRegion(Constants.LANG lang, Constants.USER_REGION userRegion)
        {
            if (lang == Constants.LANG.ENGLISH)
                return true;
            switch (userRegion)
            {
                case Constants.USER_REGION.USA:
                    if (lang == Constants.LANG.FRENCH | lang == Constants.LANG.SPANISH)
                        return true;
                    break;
                case Constants.USER_REGION.EUROPE:
                    if (lang == Constants.LANG.FRENCH | lang == Constants.LANG.GERMAN | lang == Constants.LANG.ITALIAN | lang == Constants.LANG.SPANISH)
                        return true;
                    break;
                case Constants.USER_REGION.ASIA:
                    if (lang == Constants.LANG.KOREAN | lang == Constants.LANG.THAI)
                        return true;
                    break;
                case Constants.USER_REGION.JAPAN:
                    if (lang == Constants.LANG.JAPANESE)
                        return true;
                    break;
                case Constants.USER_REGION.CHINA:
                    if (lang == Constants.LANG.CHINESE)
                        return true;
                    break;
                case Constants.USER_REGION.RUSSIA:
                    if (lang == Constants.LANG.RUSSIAN)
                        return true;
                    break;
                case Constants.USER_REGION.AFRICA:
                    if (lang == Constants.LANG.FRENCH)
                        return true;
                    break;
            }
            return false;
        }

    }
}