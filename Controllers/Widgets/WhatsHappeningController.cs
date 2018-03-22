using System.Collections.Generic;
using System.Web.Mvc;

namespace ExtronWeb.Controllers
{
    public class WhatsHappeningController : Controller
    {
        // GET: WhatsHappening
        [ChildActionOnly]
        public PartialViewResult Index()
        {
            List<Models.WhatsHappening> list = Models.WhatsHappening.GetWhatsHappening();
            return PartialView("../Widgets/WhatsHappening", list);

        }
    }
}