using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Caching;

namespace ExtronWeb.Models
{
    public class Translation
    {
        private static bool _debug = false;
        private static string _method = "";

        public enum SECTION 
        {
            NOT_SET = -1,
            GLOBALS = 0,
            PRODUCT = 1,
            APPL = 2,
            TECH = 3,
            COMPANY = 4,
            DOWNLOAD = 5,
            DEALER = 6,
            LOGIN = 10,
            EDUCATION = 11,
            WHATSNEW = 12,
            HOME = 13,
            PRODTYPE = 14, // external English
            SUBTYPE = 15, // external English
            PRODTYPEDESC = 19, // external English
            FILETYPE = 20,  // external English
            PRODLANDINGPAGE = 21,
            CALCULATORS = 23,
            FORMS = 26,
            VIDEOTITLE = 27,
            VIDEODESCRIPTION = 28, // external English
            CONTENTTYPE = 29,
            ARTICLECATEGORY = 30,  // external English
            PRODCATEGORY = 31, // external English
            BUILDERS = 36,
            VIDEOCATEGORY = 40, // external English
            PRODPROMOLOGO = 41, // external English
            ENTWINE = 42, // external English
            ENTWINEHOVER = 43 // external English
        }

        public partial class TranslationEntity
        {
            [Key]
            public int PKRecID;
            public int FKTabID;
            public int FKLangID;
            public int FKStatusID;
            public int TranslationID;
            public string TranslationText;
            public string Context;
            public string Login;
            public Nullable<System.DateTime> LastUpdate;
            public Nullable<System.DateTime> AddedDate;
            public Nullable<System.DateTime> LastUsed;
        }

        public static string GetTranslation(SECTION sectionID, int translationID)
        {
            string retVal = "";
            List<TranslationEntity> model = new List<TranslationEntity>();
            model = GetTranslationSection(sectionID);
            if (model != null)
            {
                TranslationEntity myQuery = model.Find(x => x.TranslationID.Equals(translationID));
                if (myQuery != null)
                {
                    retVal = myQuery.TranslationText;
                    if (_debug) retVal = retVal + _method;
                }
            }
            return retVal;
        }

        private static List<TranslationEntity> GetTranslationSection(SECTION tab)
        {
            string cacheName = "Translations_" + Convert.ToString(tab) + CurrentUser.Language;
            List<TranslationEntity> item = MemoryCache.Default.Get(cacheName) as List<TranslationEntity>;
            if (item == null)
            {
                _method = " (db)";
                item = LoadDataFromDatabase(tab);
                MemoryCache.Default.Add(cacheName, item, DateTime.Now.AddMinutes(10));
            }
            else _method = " (cache)";
            return item;
        }

        private static List<TranslationEntity> LoadDataFromDatabase(SECTION tab)
        {
            using (var db = DBConn.Open())
            {
                var param = new DynamicParameters();
                param.Add("@Tab", tab);
                param.Add("@Lang", CurrentUser.Language);
                var model = db.Query<TranslationEntity>("qry_F_Translations", param, commandType: CommandType.StoredProcedure).ToList();
                return model;
            }
        }
    }
}