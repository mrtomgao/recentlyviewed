
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Web.Http;
using System;
using ExtronWeb.Models;
using static ExtronWeb.Models.Insider;

namespace ExtronWeb.Controllers.api.v1
{
    public class InsiderController : ApiController
    {
        // GET api/<controller>
        [Authorize]
        public IHttpActionResult Get()
        {
            InsiderEntity insider = Insider.GetCurrentInsider();
            if (insider == null) { return NotFound(); }
            return Ok(insider);
        }
    }
}