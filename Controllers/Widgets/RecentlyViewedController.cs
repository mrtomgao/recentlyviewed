using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExtronWeb.Helpers;
using ExtronWeb.Models;

namespace ExtronWeb.Controllers.Widgets
{
    public class RecentlyViewedController : Controller
    {
        public PartialViewResult Index()
        {
            return PartialView("../Widgets/RecentlyViewed", RecentlyViewed.Get(Constants.LINKTYPE.PRODUCT));
        }

        public PartialViewResult Clear(int? index)
        {
            RecentlyViewed.Clear(Constants.LINKTYPE.PRODUCT, index);
            return PartialView("../Widgets/RecentlyViewed", RecentlyViewed.Get(Constants.LINKTYPE.PRODUCT));
        }
    }
}