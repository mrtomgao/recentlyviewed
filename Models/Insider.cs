using Dapper;
using ExtronWeb.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.OleDb;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Web;
using static ExtronWeb.Models.Insider;

namespace ExtronWeb.Models
{
    public class Insider
    {

        public enum ACCESS
        {
            LIMITED = 0,
            BASIC = 1,
            FULL = 2
        }
      
        public enum PRICING
        {
            PRICE_LIST = 0,
            MSRP_ONLY = 1,
            NO_PRICING = 2,
            PRICE_LIST_CONTRACT_PRICE = 3
        }

        public partial class InsiderEntity
        {
            // SQL tbl_DealerOnly
            [Key]
            public int PKRecID;
            public string Account;
            public string Email;
            public DateTime? RequestDate;
            public DateTime? ActivateDate;
            public DateTime? ValidateDate;
            public string FKCountryID;
            public string SalesRep;
            public string AccessLevel;
            public int FKAccountTypeID;
            public int Admin;
            public int MSRP;
            public int ContactID;

            // Avante
            public string AccountNo; // dupe of SQL Account
            public string BillingNo;
            public string CompanyName;
            public string PriceClass;
            public string PriceList;
            public string Address1;
            public string Address2;
            public string Address3;
            public string City;
            public string State;
            public string Zip;
            public string Territory;
            public string Country;
            public string BillingName;
            public string BillingAddress1;
            public string BillingAddress2;
            public string BillingAddress3;
            public string BillingCity;
            public string BillingZip;
            public string BillingState;
            public string BillingCountry;
            public string ViaCode;
            public string ShippingAcct;
            public string TermsDesc;
            public string ECC;
            public bool EFlash;
            public bool ENews;
            public bool AVWire;
            public bool ExtroNews;
            public string Fax;
            public string Phone;
            public string Title;
            public string FirstName;
            public string LastName;
            public string Dept;
            public string CustStatus;
            public string AcctStatus;
            public string LangPref;
        }
        
        public static InsiderEntity GetCurrentInsider()
        {
            if (!HttpContext.Current.Request.IsAuthenticated)
            {
                return new InsiderEntity();
            }

            var user = HttpContext.Current.Request.GetOwinContext().Authentication.User;
            var jwtToken = new JwtSecurityToken(user.FindFirst("id_token").Value);
            string email = jwtToken.Subject;
            InsiderEntity model = MemoryCache.Default.Get(Constants.USER_CACHE_PREFIX + email) as InsiderEntity;
            if (model == null)
            {
                model = GetInsiderEntity(email);
                MemoryCache.Default.Add(Constants.USER_CACHE_PREFIX + email, model, DateTime.Now.AddMinutes(60));
            }
            if (model.Email != "marcomm@extron.com") { SetUserRegionLanguage(model.Territory, model.LangPref); }
            return model;
        }
        
        private static InsiderEntity GetInsiderEntity(string email)
        {
            using (var db = DBConn.Open("DB_WEB_STG"))
            {
                string qry = "SELECT tbl_DealerOnly.*" +
                    " from tbl_DealerOnly where Email = @Email";
                InsiderEntity model = db.Query<InsiderEntity>(qry, new { Email = email }).FirstOrDefault();
                if (model != null)
                {
                    GetAvanteData(model);
                    if (model.FirstName != null) { return model; }
                } 
            }
            return null;
        }
        private static void GetAvanteData(InsiderEntity model)
        {
            if (model.Account.Length < 4) return;
            model.AccountNo = model.Account.ToUpper();
            RBAccess oredback = new RBAccess();
            oredback.Command = "W$CRM_GetAccount";
            oredback.SetParameter(1, "Y");
            oredback.SetParameter(2, "Get");
            oredback.SetParameter(3, "WEBADMIN");
            oredback.SetParameter(4, model.AccountNo.Substring(0, 2));
            oredback.SetParameter(5, model.AccountNo.Substring(3));

            OleDbDataReader avante;
            avante = oredback.GetDataReader();
            if (oredback.ErrorMessage != "") { return; }
            avante.Read();
            if (avante.HasRows)
            {
                if (avante["SERVERMESSAGE"].ToString() != "") return;
                if (avante["USERMESSAGE"].ToString() != "") return;
                if (avante["CustStatus"].ToString().Equals("3")) return;

                string[] afn = avante["ContactFname"].ToString().Split((char)252);
                string[] aln = avante["ContactLname"].ToString().Split((char)252);
                string[] aem = avante["Email"].ToString().Split((char)252);
                string[] att = avante["Title"].ToString().Split((char)252);
                string[] aenews = avante["ENews"].ToString().Split((char)252);
                string[] aavwire = avante["AVWire"].ToString().Split((char)252);
                string[] aeflash = avante["EFlash"].ToString().Split((char)252);
                string[] aextronews = avante["ExtrNews"].ToString().Split((char)252);
                string[] ael = avante["Elang"].ToString().Split((char)252);
                string[] aedept = avante["Dept"].ToString().Split((char)252);

                for (int i = 0; i < afn.Length; i++)
                {
                    if (model.Email.ToLower().Equals(aem[i].ToString().ToLower()))
                    {
                        model.FirstName = afn[i];
                        model.LastName = aln[i];
                        model.Title = att[i];
                        model.LangPref = ael[i];
                        model.ENews = aenews[i].Equals("1");
                        model.AVWire = aavwire[i].Equals("1");
                        model.EFlash = aeflash[i].Equals("1");
                        model.Dept = aedept[i];
                    }
                }

                if (model.FirstName == null) return;

                model.CompanyName = avante["CompanyName"].ToString();
                model.BillingNo = avante["BillNo"].ToString();
                model.Territory = avante["Territory"].ToString();
                model.Address1 = avante["Add1"].ToString();
                model.Address2 = avante["Add2"].ToString();
                model.Address3 = avante["Add3"].ToString();
                model.City = avante["City"].ToString();
                model.State = avante["State"].ToString();
                model.Zip = avante["Zip"].ToString();
                model.ECC = avante["ECC"].ToString();
                model.Country = avante["Country"].ToString();
                model.BillingName = avante["BillToName"].ToString();
                model.BillingAddress1 = avante["BillToAdd1"].ToString();
                model.BillingAddress2 = avante["BillToAdd2"].ToString();
                model.BillingAddress3 = avante["BillToAdd3"].ToString();
                model.BillingCity = avante["BillToCity"].ToString();
                model.BillingState = avante["BillToState"].ToString();
                model.BillingZip = avante["BillToZip"].ToString();
                model.BillingCountry = avante["BillToCountry"].ToString();
                model.ViaCode = avante["ViaCode"].ToString();
                model.ShippingAcct = avante["ShippingAcct"].ToString();
                model.TermsDesc = avante["TermsDesc"].ToString();
                model.Phone = avante["Phone"].ToString();
                model.Fax = avante["Fax"].ToString();
                model.PriceList = avante["PriceList"].ToString();
                model.PriceClass = avante["PriceClass"].ToString();
                model.CustStatus = avante["CustStatus"].ToString();
                model.AcctStatus = avante["AcctStatus"].ToString();
            }
        }

        private static void SetUserRegionLanguage(string terr, string pref)
        {
            Constants.USER_REGION userRegion = CurrentUser.GetUserRegionByTerritory(terr);
            bool useAvantePref = false;
            bool cookieUpdated = false;
            if (userRegion != CurrentUser.UserRegion)
            {
                // set region
                CurrentUser.UserRegion = userRegion;
                cookieUpdated = true;
                //set language
                switch (userRegion)
                {
                    case Constants.USER_REGION.CHINA: CurrentUser.Language = Constants.LANG.CHINESE; break;
                    case Constants.USER_REGION.JAPAN: CurrentUser.Language = Constants.LANG.JAPANESE; break;
                    default:
                        CurrentUser.Language = CurrentUser.GetUserLanguageByAvantePref(pref);
                        break;
                }
                
            }
            else if (CurrentUser.Language == Constants.LANG.ENGLISH)
            {
                useAvantePref = true;
            }
            if (useAvantePref)
            {
                Constants.LANG userLang = CurrentUser.GetUserLanguageByAvantePref(pref);
                switch (userRegion)
                {
                    case Constants.USER_REGION.USA: 
                        if (userLang == Constants.LANG.FRENCH || userLang == Constants.LANG.SPANISH)
                        {
                            CurrentUser.Language = userLang;
                            cookieUpdated = true;
                        }
                        break;
                    case Constants.USER_REGION.EUROPE: 
                        if (userLang == Constants.LANG.FRENCH || userLang == Constants.LANG.SPANISH || userLang == Constants.LANG.ITALIAN || userLang == Constants.LANG.GERMAN)
                        {
                            CurrentUser.Language = userLang;
                            cookieUpdated = true;
                        }
                        break;
                    case Constants.USER_REGION.ASIA:
                        if (userLang == Constants.LANG.KOREAN)
                        {
                            CurrentUser.Language = userLang;
                            cookieUpdated = true;
                        }
                        break;
                }
            }
            if (cookieUpdated)
            {
                // reload the page
                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.ToString(), true);
            }
        }
    }

    /// <summary>
    /// Extension methods for the InsiderEntity model.
    /// These methods do no getting/setting of properties on the InsiderEntity
    /// and are rather used to derive information based on the InsiderEntity properties.
    /// </summary>
    public static class InsiderExtension
    {

        private class PRICE_CLASS
        {
            // prefixes
            public const string COMPANY_O2_KEY_CORPORATE = "82-09";
            public const string CONSULTANT = "15";
            public const string KEY_CORPORATE = "09";
            public const string PROGRAMMER = "20";
            public const string VERTICAL_MARKET_INTEGRATOR = "13";

            public const string EDUCATION_K12 = "19";
            public const string EDUCATION_UNIVERSITY = "02";
            public const string GOVERNMENT = "14";
            public const string MANUFACTURER_SALES = "03";
            public const string MANUFACTURER_SPECIAL = "32";
        }

        public static bool CanViewPricing(this InsiderEntity i)
        {
            return i.MSRP != 2;
        }

        public static bool CanViewEdContractPricing(this InsiderEntity i)
        {
            if (i.GetLoginType() == Constants.LOGIN.DEALER && (PRICING)i.MSRP == PRICING.PRICE_LIST_CONTRACT_PRICE &&
                i.PriceList != "GOVS" && i.Country == "UNITED STATES")
            {
                return true;
            }
            return false;
        }

        public static string GetDefaultHome(this InsiderEntity i)
        {
            Constants.LOGIN loginType = i.GetLoginType();
            switch (loginType)
            {

            }
            return "/";
        }

        public static Constants.LOGIN GetLoginType(this InsiderEntity i)
        {
            var type = i.FKAccountTypeID;
            if (Enum.IsDefined(typeof(Constants.LOGIN), type))
            {
                return (Constants.LOGIN)type;
            }
            return Constants.LOGIN.NONE;
        }

        public static string GetFullName(this InsiderEntity i)
        {
            return i.FirstName + " " + i.LastName;
        }
        public static string GetFullAddress(this InsiderEntity i)
        {
            return i.Address1 + " " + i.City + " " + i.State + " " + i.Zip;
        }

        public static string GetCompanyNo(this InsiderEntity i)
        {
            return i.AccountNo.Substring(0, 2);
        }

        public static string GetCityStateZip(this InsiderEntity i)
        {
            return i.City + ", " + i.State + " " + i.Zip;
        }

        public static string GetBillToAccount(this InsiderEntity i)
        {
            return i.GetCompanyNo() + "*" + i.BillingNo;
        }

        public static string GetBillingCityStateZip(this InsiderEntity i)
        {
            return i.BillingCity + ", " + i.BillingState + " " + i.BillingZip;
        }

        public static string GetBillingAddressLabel(this InsiderEntity i)
        {
            string addressBlock = i.BillingName + "<br>" +
                                 i.BillingAddress1 + "<br>" +
                                 i.BillingAddress2 + "<br>" +
                                 i.BillingAddress3 + "<br>" +
                                 i.GetBillingCityStateZip() + "<br>" +
                                 i.BillingCountry;
            return addressBlock;
        }

        public static string GetAddressLabel(this InsiderEntity i)
        {
            string addressBlock = i.GetFullName() + "<br>" +
                                 i.Address1 + "<br>" +
                                 i.Address2 + "<br>" +
                                 i.Address3 + "<br>" +
                                 i.GetCityStateZip() + "<br>" +
                                 i.Country;
            return addressBlock;
        }

        public static string GetPriceClassDescription(this InsiderEntity i)
        {
            using (var db = DBConn.Open("DB_WEB"))
            {
                string qry = "SELECT Description FROM tbl_PriceClass_L WHERE PriceClass = @PriceClass";
                var desc = db.Query(qry, new { PriceClass = i.PriceClass }).FirstOrDefault().ToString();
                return desc;
            }
        }

        public static string GetPriceList(this InsiderEntity i)
        {
            string overridePriceList = i.OverridePriceList();
            if (overridePriceList != "") { return overridePriceList; }
            string userPriceList = i.PriceList;
            string prefix = Functions.GetIntlPrefix(userPriceList);
            if (prefix == "SA")
            {
                userPriceList = userPriceList.Substring(3);
                prefix = "";
            }
            string companyNo = i.GetCompanyNo();
            if (i.IsConsultant() && (companyNo == Constants.COMPANY_NO.ASIA || companyNo == Constants.COMPANY_NO.MIDDLE_EAST))
            {
                return prefix + "EU";
            }
            else if (i.MSRP == 1)
            {
                return prefix + "EU";
            }
            else if (i.GetLoginType() == Constants.LOGIN.EDUCATOR && i.ECC == "US" && i.PriceList == "EU")
            {
                return "GOVS";
            }
            return userPriceList;
        }

        public static PRICING GetPricingAccess(this InsiderEntity i)
        {
            switch (i.MSRP)
            {
                case 0:
                    return PRICING.PRICE_LIST;
                case 1:
                    return PRICING.MSRP_ONLY;
                case 2:
                    return PRICING.NO_PRICING;
                case 3:
                    return PRICING.PRICE_LIST_CONTRACT_PRICE;
                default:
                    return PRICING.NO_PRICING;
            }
        }

        public static string GetSupportEmail(this InsiderEntity i)
        {
            string terr = i.Territory;
            bool territoryInUSOrCanada = Functions.TerritoryInCanada(terr) || Functions.TerritoryInUnitedStates(terr);
            if (territoryInUSOrCanada)
            {
                if (i.IsEducatorK12()) { return "USA-Sales-EducationSupport-K-12@extron.com"; }
                if (i.IsEducatorUniv())
                {
                    switch (terr)
                    {
                        case "CEN": return "USA-Sales-HigherEdCentral@extron.com";
                        case "MA": return "USA-Sales-HigherEdMidAtlantic@extron.com";
                        case "MW": return "USA-Sales-HigherEdMidwest@extron.com";
                        case "NE": return "USA-Sales-HigherEdNortheast@extron.com";
                        case "PNW": return "USA-Sales-HigherEdPacificNW@extron.com";
                        case "SE": return "USA-Sales-HigherEdSoutheast@extron.com";
                        case "SW": return "USA-Sales-HigherEdSouthwest@extron.com";
                        case "EC": return "USA-Sales-HigherEdCanada@extron.com";
                        case "WC": return "USA-Sales-HigherEdCanada@extron.com";
                    }
                }

            }
            if (Functions.TerritoryInAsia(terr))
            {
                if (i.IsEducatorK12() || i.IsEducatorUniv()) { return "SGP-Sales-Education@extron.com"; }
            }
            if (Functions.TerritoryInEurope(terr))
            {
                if (i.IsEducatorK12() || i.IsEducatorUniv()) { return "edu-" + terr + "@extron.com"; }
            }

            if (i.IsConsultant())
            {
                if (territoryInUSOrCanada) return "consultants@extron.com";
                if (Functions.TerritoryInEurope(terr)) return "europeconsultants@extron.com";
                if (Functions.TerritoryInAsia(terr)) return "asianconsultants@extron.com";
            }


            if (i.IsProgrammer())
            {
                if (territoryInUSOrCanada) { return "programmers@extron.com"; }

            }

            if (i.IsVerticalMarketIntegrator())
            {
                if (territoryInUSOrCanada) { return "USA-Sales-NationalAccounts@extron.com"; }
            }

            return terr + "-team@extron.com";
        }

        // TODO: need to enable role manager
        public static string GetUserRoles(this InsiderEntity i)
        {
            string[] roles = System.Web.Security.Roles.GetRolesForUser(i.Email);
            for (int r = 0; r < roles.Length; r++)
            {
                roles[r] = GetRole(roles[r]).ToString();
            }
            return String.Join(",", roles);
        }

        public static bool HasMsrpPricingException(this InsiderEntity i)
        {
            if (!i.CanViewPricing()) return true;

            // Per Minnie Tan: do not show MSRP to Asia resellers
            if (Functions.TerritoryInAsia(i.Territory) &&
                !i.IsAussieDistributor() &&
                i.PriceList != "EU") { return true; }

            return false;

        }

        public static bool IsAussieDistributor(this InsiderEntity i)
        {
            if (i.AccountNo == "03*AS0612" || i.Email.EndsWith("@extron.com.au")) return true;
            return false;
        }

        public static bool IsConsultant(this InsiderEntity i)
        {
            return i.PriceClass.Contains(PRICE_CLASS.CONSULTANT);
        }

        public static bool IsEducatorK12(this InsiderEntity i)
        {
            return i.PriceClass.Equals(PRICE_CLASS.EDUCATION_K12);
        }

        public static bool IsEducatorUniv(this InsiderEntity i)
        {
            return i.PriceClass.Equals(PRICE_CLASS.EDUCATION_UNIVERSITY);
        }

        public static bool IsMSRPOnly(this InsiderEntity i)
        {
            return i.MSRP == 1;
        }

        public static bool IsProgrammer(this InsiderEntity i)
        {
            return i.PriceClass.Contains(PRICE_CLASS.PROGRAMMER);
        }

        public static bool IsVerticalMarketIntegrator(this InsiderEntity i)
        {
            if (i.PriceClass.Contains(PRICE_CLASS.VERTICAL_MARKET_INTEGRATOR) ||
                i.PriceClass.Contains(PRICE_CLASS.KEY_CORPORATE) ||
                i.PriceClass.Contains(PRICE_CLASS.COMPANY_O2_KEY_CORPORATE))
            {
                return true;
            }

            switch (i.PriceClass)
            {
                case PRICE_CLASS.GOVERNMENT:
                case PRICE_CLASS.MANUFACTURER_SALES:
                case PRICE_CLASS.MANUFACTURER_SPECIAL:
                    return true;
                default:
                    return false;
            }
        }

        public static string OverridePriceList(this InsiderEntity i)
        {
            if (i.Email.EndsWith("@extron.com"))
            {
                int recording;
                int.TryParse(HttpContext.Current.Response.Cookies["recording"].Value, out recording);
                if (recording > 0)
                {
                    using (var db = DBConn.Open("DB_WEB"))
                    {
                        string qry = "SELECT ISNULL(PriceList, '') AS OverridePriceList, WantMsrp FROM tbl_DeckSession WHERE PKSessionID = @SessionId";
                        string overridePriceList = db.Query(qry, new { SessionId = recording }).FirstOrDefault().ToString();
                        return overridePriceList;
                    }
                }
            }
            return "";
        }

        // legacy homebrew SSO for AVSD and Extron Classroom
        // set SSOAuthenID for current Insider for immediate consumption
        public static string SetSSOAuthenID()
        {
            var user = HttpContext.Current.Request.GetOwinContext().Authentication.User as ClaimsPrincipal;
            var jwtToken = new JwtSecurityToken(user.FindFirst("id_token").Value);
            string email = jwtToken.Subject;
            string sso = Guid.NewGuid().ToString().Replace("-", String.Empty);
            using (var db = DBConn.Open("DB_WEB"))
            {
                var param = new DynamicParameters();
                param.Add("@SSOAuthenId", sso);
                param.Add("@Email", email);
                var model = db.Query("Qry_U_SingleSignOn_SetSSOAuthenId", param, commandType: CommandType.StoredProcedure);
            }
            return sso;
        }

        // legacy homebrew SSO for AVSD and Extron Classroom
        // if valid, returns the email corresponding to the given ssoId
        public static string GetSSOAuthenID(string sso)
        {
            using (var db = DBConn.Open("DB_WEB"))
            {
                var param = new DynamicParameters();
                param.Add("@SSOAuthenId", sso);
                var email = db.Query("Qry_F_SingleSignOn", param, commandType: CommandType.StoredProcedure).FirstOrDefault().ToString();
                return email;
            }
        }


        private static Constants.ROLE GetRole(string role)
        {
            switch (role)
            {
                case "dealeronly": return Constants.ROLE.RESELLER;
                case "educator": return Constants.ROLE.EDUCATOR;
                case "eapcertified": return Constants.ROLE.EAP_CERTIFIED;
                case "affiliate": return Constants.ROLE.AFFILIATE;
                default: return Constants.ROLE.NONE;
            }
        }
    }
}