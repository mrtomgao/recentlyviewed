using System.Collections.Generic;
using System.Web.Mvc;
using ExtronWeb.Models;
using ExtronWeb.Models.Help;
using static ExtronWeb.Models.Insider;


namespace ExtronWeb.Controllers.Help
{
    public class HelpController : Controller
    {
        [HttpPost]
        public JsonResult SubmitTopicFeedback(ExtronWeb.Models.Help.Help.FeedbackEntity feedback)
        {
            var model = new TopicViewModel();

            InsiderEntity currentInsider = Insider.GetCurrentInsider();
            if (currentInsider.Email == null)
            {
                feedback.Email = "NO_LOGIN";
            }
            else
            {
                feedback.Email = currentInsider.Email;
            }

            feedback.UserAgent = HttpContext.Request.UserAgent;
            feedback.IPAddress = Request.UserHostAddress;

            if (model.SubmitTopicFeedback(feedback))
            {
                return Json(feedback);
            }
            return null;
        }

        public ActionResult Index()
        {
            var model = new ExtronWeb.Models.Help.Help();
            return View(model.GetIndex());
        }

        public ActionResult Selected(string handle, int? v)
        {
            var model = new SelectedViewModel();
            if (model.Get(handle,v))
                {
                    TempData["TOC"] = model.OuterTOC;                    
                    return View(model);
                }            
            return RedirectToAction("Index", "Help");
        }

        public ActionResult Topic(string handle, int? contents)
        {
            var model = new TopicViewModel();

            if (TempData["TOC"] != null)
            {
                model.SetInnerTOCFromTemp(TempData["TOC"] as List<ExtronWeb.Models.Help.Help.ToCEntity>);
            }

            if (model.Get(contents))
            {
                TempData["TOC"] = model.InnerTOC;
                return View(model);                    
            }

            return RedirectToAction("/" + handle, "Help");
        }

        [HttpGet]    
        public ActionResult Search(string keyword, int? version)
        {            
            if (keyword == null)
            { return RedirectToAction("Index", "Help"); }
            ViewBag.Keyword = keyword;

            var model = new SearchViewModel();
            model.Get(keyword, version);
            if (model.Get(keyword, version))
            {
                if (model.SelectedHelp != null)
                {
                    ViewBag.SearchTitle = model.SelectedHelp.Name;
                    ViewBag.SelectedVersion = model.SelectedVersion.PKVersionID;
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Help");
            }
        }

    }
}