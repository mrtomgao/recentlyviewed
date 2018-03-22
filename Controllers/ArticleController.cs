using ExtronWeb.Models;
using System.Web.Mvc;
using static ExtronWeb.Models.Insider;
using ExtronWeb.Helpers;

namespace ExtronWeb.Controllers.Article
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index(string articlehandle)
        {
            ExtronWeb.Models.Article article = new ExtronWeb.Models.Article();
            InsiderEntity currentInsider = Insider.GetCurrentInsider();
            var model = article.GetArticle(articlehandle, currentInsider);
            if (model != null)
            {
                return View(model);
            }
            return Redirect("/");
        }

        public PartialViewResult EmbeddedArticle(string articlehandle)
        {
            ExtronWeb.Models.Article article = new ExtronWeb.Models.Article();
            InsiderEntity currentInsider = Insider.GetCurrentInsider();
            var model = article.GetArticle(articlehandle, currentInsider);
            return PartialView("../Widgets/EmbeddedArticle", model);
        }
    } 
}