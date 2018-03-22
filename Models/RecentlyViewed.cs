using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExtronWeb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ExtronWeb.Helpers;

namespace ExtronWeb.Models
{
    public class RecentlyViewed
    {
        public abstract partial class RecentlyViewedItem
        {
            public string Name;
            public DateTime ViewDate;
        } 

        public partial class RecentlyViewedProduct : RecentlyViewedItem
        {
            public string SubTitle;
            public string Thumbnail
            {
                get { return "/media/img/product/img-sm/" + FileHandle + "-sm.jpg"; }
            }
            public string FileHandle;
            public string TimeAgo { get { return Functions.TimeAgo(ViewDate); } }
        }

        public static IEnumerable<RecentlyViewedItem> Get(Constants.LINKTYPE type)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["ExtronRecView_" + type];

            if (cookie != null)
            {                
                //making sure in JSON Array format
                if (cookie.Value.StartsWith("[") == false && cookie.Value.EndsWith("]") == false)
                    { cookie.Value = "[" + cookie.Value + "]"; }

                switch (type)
                {
                    case Constants.LINKTYPE.PRODUCT:
                        if (Functions.IsValidJson(cookie.Value))
                        {
                            List<RecentlyViewedProduct> items =
                            JsonConvert.DeserializeObject<List<RecentlyViewedProduct>>(cookie.Value);
                            if (items != null) { items.Reverse(); }
                            return items;
                        }
                        else
                        {
                            Clear(Constants.LINKTYPE.PRODUCT, null);
                        }
                        break;
                    case Constants.LINKTYPE.ARTICLE:
                        //TODO: return Article links when ready;
                        break;
                    default:
                        return null;
                }     
            }
            return null;
        }

        public static void Set(dynamic obj, Constants.LINKTYPE type)
        {           
            HttpCookie cookie = HttpContext.Current.Request.Cookies["ExtronRecView_" + type];

            if (cookie == null)
            {
                cookie = new HttpCookie("ExtronRecView_" + type);
            }
            else
            {
                if (Functions.IsValidJson(cookie.Value) == false)
                {
                    //invalid cookie, clear existing and start new.
                    Clear(type, null);
                    cookie = new HttpCookie("ExtronRecView_" + type);
                }
            }
            
            switch (type)
            {
                case Constants.LINKTYPE.PRODUCT:
                    List<RecentlyViewedProduct> items =
                        JsonConvert.DeserializeObject<List<RecentlyViewedProduct>>(cookie.Value ?? "[]");
                    items.Add(new RecentlyViewedProduct { FileHandle = obj.FileHandle, SubTitle = obj.SubTitle, Name = obj.Name, ViewDate = obj.ViewDate });

                    var duplicated = from i in items
                        group i by i.FileHandle into g
                        where g.Count() > 1
                        select g.Key;

                    items.Remove(items.Find(i => duplicated.Contains(i.FileHandle)));

                    if (items.Count > 12)
                    { items.RemoveRange(0, items.Count - 12); }

                    cookie.Value = JsonConvert.SerializeObject(items);
                    break;
                case Constants.LINKTYPE.ARTICLE:
                    //TODO: Set and filter Article links when ready;
                    break;
                default:
                    break;
            }            
            cookie.Expires = DateTime.Now.AddYears(10);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void Clear(Constants.LINKTYPE type, int? index)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["ExtronRecView_" + type];
            if (cookie != null)
            {
                if (index == null)
                {
                    cookie.Value = "";
                    cookie.Expires = DateTime.Now.AddDays(-1d);
                }
                else
                {                  
                    var items = JsonConvert.DeserializeObject<List<object>>(cookie.Value);                 
                    items.RemoveAt((items.Count - 1) - (index ?? default(int)));
                    if (items.Count == 0)
                    {
                        cookie.Value = "";
                        cookie.Expires = DateTime.Now.AddDays(-1d);
                    }
                    else
                    {
                        cookie.Value = JsonConvert.SerializeObject(items);
                        cookie.Expires = DateTime.Now.AddYears(10);
                    }
                }
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }       
    }
}