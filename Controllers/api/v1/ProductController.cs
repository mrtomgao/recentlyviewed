using ExtronWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static ExtronWeb.Models.Insider;
using static ExtronWeb.Models.Price;

namespace ExtronWeb.Controllers.api.v1
{
    [Authorize]
    public class ProductController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/60-415-01
        public IHttpActionResult GetPartNum(string partnum)
        {
            ExtronWeb.Models.Product prod = new ExtronWeb.Models.Product();

            return Ok(prod.GetProdModel(partnum));
        }

        public IHttpActionResult GetPricing(string partnum)
        {
            InsiderEntity insider = Insider.GetCurrentInsider();
            AvantePricingEntity pricing = Price.GetInsiderPrice(partnum, insider);
            ProdPricing result = new ProdPricing()
            {
                YourPrice = pricing.DealerPrice.GetPrice(),
                MSRP = pricing.ListPrice.GetPrice()
            };
            return Ok(result);
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        private class ProdPricing
        {
            public string YourPrice;
            public string MSRP;
        }
    }
}