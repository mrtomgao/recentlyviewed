using ExtronWeb.Helpers;
using ExtronWeb.Models;
using System.IdentityModel.Tokens;
using System.Runtime.Caching;
using System.Security.Claims;
using System;
using System.Web;
using System.Web.Mvc;
using static ExtronWeb.Models.Insider;
using Microsoft.Owin.Security;
using System.Web.Configuration;

namespace ExtronWeb.Controllers.Home
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Login()
        {
            InsiderEntity currentInsider = Insider.GetCurrentInsider();
            return View(currentInsider);
        }

        [Authorize]
        public ActionResult Logout()
        {
            var user = Request.GetOwinContext().Authentication.User;
            Request.GetOwinContext().Authentication.SignOut(new AuthenticationProperties()
            {
                RedirectUri = WebConfigurationManager.AppSettings["SSO_Return"]
            });
            return Redirect("/");
        }


        // footer idLanguage dropdown
        public void SetRegionLang(int reg, int lang)
        {
            if (Enum.IsDefined(typeof(Constants.USER_REGION), reg))
            {
                CurrentUser.WriteCookies("region", reg.ToString());
            }
            if (Enum.IsDefined(typeof(Constants.LANG), lang))
            {
                CurrentUser.WriteCookies("lang", lang.ToString());
            }
        }
    }
}