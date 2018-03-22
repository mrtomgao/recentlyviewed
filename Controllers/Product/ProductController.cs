using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Caching;
using Newtonsoft.Json;
using ExtronWeb.Helpers;
using ExtronWeb.Models;
using static ExtronWeb.Models.Insider;
using static ExtronWeb.Models.Price;

namespace ExtronWeb.Controllers.Product
{
    public class ProductController : Controller
    {
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Selected(string handle)
        {
            ExtronWeb.Models.Product prod = new ExtronWeb.Models.Product();
            var model = prod.GetProduct(handle);
            if (model != null)
            {                
                RecentlyViewed.Set(new { Name = model.Name, SubTitle = model.SubTitle, FileHandle = model.FileHandle, ViewDate = DateTime.Now}, Constants.LINKTYPE.PRODUCT);
                return View(model);
            }
            return Redirect("/Product");
        }

        [Authorize]
        public ActionResult Protected()
        {
            return View();
        }

        [Authorize]
        // /Product/Get/60-415-01 (this works because of default route in RouteConfig.cs)
        public ActionResult Get(string id)
        {
            // TODO: Do we need to worry about a user's authorization tokens here?
            ExtronWeb.Models.Product prod = new ExtronWeb.Models.Product();
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(prod.GetProdModel(id)));
        }

        [Authorize]
        // /Product/PriceTest/[partnum]
        public ActionResult PriceTest()
        {
            return View("ProductPricingTest");
        }

        // might need this for refresh tokens?
        /*
        private async Task<TokenResponse> GetToken()
        {
            var client = new TokenClient(
                "https://localhost:44390/connect/token",
                "mvc.client.service",
                "apisecret");
            return await client.RequestClientCredentialsAsync("api");
        }
        */

    }
}