using System;
using System.Linq;
using Dapper;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace ExtronWeb.Models
{
    public class Product
    {
        public partial class ProductEntity
        {
            [Key]
            public int ProdID;
            public string Name;
            public string SubTitle;
            public string FileHandle;
            public int ProdStatus;
            public string DescriptionA;
            public string DescriptionB;
            public string Specs;
            public string Comments;
            public int ProdType;
            public string ProdTypeDesc;
            public int SubType;
            public string SubTypeDesc;
            public string Title;
            public string MetaDescription;
            public string MetaKeywords;
            public string EnglishName;
        }
        public partial class ProdModelEntity
        {
            public String PartNum,
                        Filehandle,
                        ModelName,
                        Description,
                        ProductName,
                        ProductSubtitle,
                        Version;
            public DateTime AddDate;
        }
        
        public ProdModelEntity GetProdModel(string partnum)
        {
            using (var db = DBConn.Open())
            {                
                var param = new DynamicParameters();
                param.Add("@PartNum", partnum);
                param.Add("@Server", "Prod"); // TODO: update to the equivalent to SystemConstants.WHICH_SERVER
                param.Add("@RegionID", CurrentUser.UserRegion);
                param.Add("@LangID", CurrentUser.Language);
                param.Add("@IncludeRetired", 0);

                return db.Query<ProdModelEntity>("Qry_F_ProdModelByPartNum", param, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
        }

        public ProductEntity GetProduct(string handle)
        {
            using (var db = DBConn.Open())
            {
                var param = new DynamicParameters();
                param.Add("@Filehandle", handle);                
                param.Add("@RegionID", CurrentUser.UserRegion);
                param.Add("@Lang", CurrentUser.Language);
                return db.Query<ProductEntity>("Qry_F_ProductByFileHandleAndLang", param, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
        }

    }
}