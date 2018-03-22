using ExtronWeb.Helpers;
using System;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using static ExtronWeb.Models.Insider;
using static ExtronWeb.Models.Price;

namespace ExtronWeb.Models
{
    public class Price
    {
        public partial class AvantePricingEntity
        {
            public string MktngDesc;
            public bool WebEnabled;
            public string PriceList;
            public string PartGroup;
            public PriceEntity DealerPrice;
            public PriceEntity ListPrice;
            public bool AvanteActive;
            public Constants.PRODUCT_PHASE ProductPhase;
            public Constants.COMPONENT_PHASE ComponentPhase;
            public bool TakeOrderFlag;
            public bool StopShip;

        }
        public partial class PriceEntity
        {
            public decimal value;
            public Constants.CURRENCY_TYPE currency = Constants.CURRENCY_TYPE.USD;
        }

        public const decimal NO_ACCESS = (decimal)999999;

        public static PriceEntity CALL_PRICING = new PriceEntity()
        {
            value = NO_ACCESS
        };


        public static AvantePricingEntity GetInsiderPrice(string partnum, InsiderEntity insider, bool applyDiscounts = true)
        {
            if (insider.AccountNo == "") return null;// new PriceEntity() { value = NO_ACCESS };
            AvantePricingEntity p = GetAvantePricing(partnum, insider, applyDiscounts);
            if (insider.IsMSRPOnly()) { p.DealerPrice = CALL_PRICING; }
            if (insider.HasMsrpPricingException()) { p.ListPrice = CALL_PRICING; }
            return p;
        }

        public static string FormatCurrency(decimal value, Constants.CURRENCY_TYPE currency)
        {
            string price = String.Format("{0:C}", value);
            switch (currency)
            {
                case Constants.CURRENCY_TYPE.EUR:
                    if (CurrentUser.Language != Constants.LANG.ENGLISH)
                    {
                        // swap the dot and comma to x.xxx,xx format
                        price = price.Replace('.', '=').Replace(',', '.').Replace('=', ',');
                    }
                    Constants.LANG[] symbolAtEnd = { Constants.LANG.FRENCH, Constants.LANG.GERMAN, Constants.LANG.SPANISH };
                    if (symbolAtEnd.Contains(CurrentUser.Language))
                    {
                        price = price.Substring(1) + "$";
                        if (CurrentUser.Language == Constants.LANG.FRENCH) { price = price.Replace('.', ' '); }
                    }
                    else
                    {
                        price = price.Replace("$", "$ ");
                    }

                    break;
                case Constants.CURRENCY_TYPE.JPY:
                    int pointIndex = price.IndexOf('.');
                    price = price.Substring(0, pointIndex);
                    break;
            }
            price = price.Replace("$", GetCurrencySymbol(currency));
            return price;
        }

        public static PriceEntity Add(PriceEntity p, PriceEntity add)
        {
            if (!p.IsEligible() && !add.IsEligible())
            {
                return CALL_PRICING;
            }
            if (p.currency != add.currency)
            {
                // currency mismatch
                return p;
            }
            return new PriceEntity() { value = p.value + add.value, currency = p.currency };
        }

        public static PriceEntity Subtract(PriceEntity p, PriceEntity sub)
        {
            if (!p.IsEligible() && !sub.IsEligible())
            {
                return CALL_PRICING;
            }
            if (p.currency != sub.currency)
            {
                // currency mismatch
                return p;
            }
            return new PriceEntity() { value = p.value - sub.value, currency = p.currency };
        }

        public static PriceEntity Multiply(PriceEntity p, decimal multiplier)
        {
            if (!p.IsEligible())
            {
                return CALL_PRICING;
            }
            return new PriceEntity() { value = p.value * multiplier, currency = p.currency };
        }

        public static PriceEntity Divide(PriceEntity p, int divisor)
        {
            if (!p.IsEligible())
            {
                return CALL_PRICING;
            }
            return new PriceEntity() { value = p.value / divisor, currency = p.currency };
        }

        private static string GetCurrencySymbol(Constants.CURRENCY_TYPE currency, bool htmlEncoded = false)
        {
            switch (currency)
            {
                case Constants.CURRENCY_TYPE.AUS:
                case Constants.CURRENCY_TYPE.CAD:
                case Constants.CURRENCY_TYPE.SA:
                case Constants.CURRENCY_TYPE.USD:
                    return "$";
                case Constants.CURRENCY_TYPE.CNY:
                case Constants.CURRENCY_TYPE.JPY:
                    return htmlEncoded ? "&#165;" : "¥";
                case Constants.CURRENCY_TYPE.EUR:
                case Constants.CURRENCY_TYPE.SW:
                    return htmlEncoded ? "&#8364;" : "€";
                case Constants.CURRENCY_TYPE.GBP:
                    return htmlEncoded ? "&#163;" : "£";
                default: return "";

            }
        }

        private static AvantePricingEntity GetAvantePricing(string partnum, InsiderEntity insider, bool applyDiscounts)
        {
            AvantePricingEntity model = new AvantePricingEntity();
            Constants.CURRENCY_TYPE insiderCurrency = GetEnumCurrency(Functions.GetIntlPrefix(insider.PriceList));

            RBAccess oredback = new RBAccess();
            oredback.Command = "RPC_GetPricing_II";
            oredback.SetParameter(1, "");
            oredback.SetParameter(2, "");
            oredback.SetParameter(3, insider.GetBillToAccount());
            oredback.SetParameter(4, partnum);
            oredback.SetParameter(5, insider.PriceList);
            oredback.ClearFields();
            oredback.AddField("MktngDesc", "MktngDesc");
            oredback.AddField("WebEnabled", "WebEnabled");
            oredback.AddField("PriceCode", "PriceCode");
            oredback.AddField("CustPriceList", "CustPriceList");
            oredback.AddField("PartGroup", "PartGroup");
            oredback.AddField("DealerDisc", "DealerDisc");
            oredback.AddField("DealerDiscQtyBrk", "DealerDiscQtyBrk");
            oredback.AddField("DealerPrice", "DealerPrice");
            oredback.AddField("DealerQtyBrk", "DealerQtyBrk");
            oredback.AddField("ItemDisc", "ItemDisc");
            oredback.AddField("ItemDiscQtyBrk", "ItemDiscQtyBrk");
            oredback.AddField("ListPrice", "ListPrice");
            oredback.AddField("ListQtyBrk", "ListQtyBrk");
            oredback.AddField("Active", "AvanteActive");
            oredback.AddField("ProductPhase", "ProductPhase");
            oredback.AddField("ComponentPhase", "ComponentPhase");
            oredback.AddField("TakeOrderFlag", "TakeOrder");
            oredback.AddField("StopShipFlag", "StopShip");

            DataTable avante = oredback.GetDataTable("dlrprc");
            if (oredback.ErrorMessage != "") { return null; }
            if (avante.Rows.Count == 0) { return null; }

            DataRow data = avante.Rows[0];

            model.MktngDesc = data["MktngDesc"].ToString();
            model.WebEnabled = data["WebEnabled"].ToString() == "Y" ? true : false;
            model.AvanteActive = data["AvanteActive"].ToString() == "Y" ? true : false;
            model.TakeOrderFlag = data["TakeOrder"].ToString() == "Y" ? true : false;
            model.StopShip = data["StopShip"].ToString() == "Y" ? true : false;
            model.ProductPhase = GetEnumProductPhase(data["ProductPhase"].ToString());
            model.ComponentPhase = GetEnumComponentPhase(data["ComponentPhase"].ToString());
            model.PartGroup = data["PartGroup"].ToString();
            model.PartGroup = model.PartGroup.Substring(model.PartGroup.IndexOf('*'), model.PartGroup.IndexOf(']'));

            model.PriceList = insider.PriceList == "" ? data["CustPriceList"].ToString() : insider.PriceList;
            if (insider.HasMsrpPricingException())
            {
                model.ListPrice = CALL_PRICING;
            }
            else
            {
                string[] listPrices = data["ListPrice"].ToString().Split(RBAccess.VM);
                model.ListPrice = new PriceEntity() { value = Decimal.Parse(listPrices[0]), currency = insiderCurrency };
            }

            decimal calcPrice;
            int qtyBrk, lineItem = 1, prevQtyBrk = 0;
            string qtyBrkDisplay;
            string[] dealerPrices = NO_ACCESS.ToString().Split(), dealerQtyBrks = NO_ACCESS.ToString().Split();
            if (!insider.HasMsrpPricingException())
            {
                dealerPrices = data["DealerPrice"].ToString().Split(RBAccess.VM);
                dealerQtyBrks = data["DealerQtyBrk"].ToString().Split(RBAccess.VM);
            }

            // The original pricing.vb ran through the loop, but only 
            // ever returned the first price item generated.
            // Let's process only the first item to begin with.
            int i = 0;
            //for (int i = 0; i < dealerPrices.Length; i++)
            //{
            calcPrice = Decimal.Parse(dealerPrices[i]);
            qtyBrk = Math.Min(int.Parse(dealerQtyBrks[i]), (int)NO_ACCESS); // CINTZERO
            qtyBrkDisplay = (prevQtyBrk + 1).ToString();
            if (dealerPrices.Length == lineItem)
            {
                qtyBrkDisplay += "+";
            } else
            {
                qtyBrkDisplay += "-" + qtyBrk.ToString();
            }

            if (applyDiscounts && PriceExtension.IsEligible(calcPrice))
            {
                int lineItemBrk;
                decimal lineItemPrice;
                string[] discounts, discountQtyBrks;

                // type 1 discount - line item
                if (data["ItemDisc"].ToString() != "")
                {
                    discounts = data["ItemDisc"].ToString().Split(RBAccess.VM);
                    discountQtyBrks = data["ItemDiscQtyBrk"].ToString().Split(RBAccess.VM);

                    for (int j = 0; j < discounts.Length; j++)
                    {
                        lineItemPrice = decimal.Parse(discounts[j]);
                        lineItemBrk = Math.Min(int.Parse(discountQtyBrks[j]), (int)NO_ACCESS);
                        if (qtyBrk <= lineItemBrk)
                        {
                            calcPrice += lineItemPrice;
                            break;
                        }
                    }
                }
                    
                // type 4 discount - overall percentage
                if (data["DealerDisc"].ToString() != "")
                {
                    discounts = data["DealerDisc"].ToString().Split(RBAccess.VM);
                    discountQtyBrks = data["DealerDisc"].ToString().Split(RBAccess.VM);
                    for (int j = 0; j < discounts.Length; j++)
                    {
                        lineItemPrice = decimal.Parse(discounts[j]);
                        lineItemBrk = Math.Min(int.Parse(discountQtyBrks[j]), (int)NO_ACCESS);
                        if (qtyBrk <= lineItemBrk)
                        {
                            calcPrice *= (1 + (lineItemPrice / 100));
                            break;
                        }
                    }
                }
                model.DealerPrice = new PriceEntity() { value = calcPrice, currency = insiderCurrency };
            }
            //}
            return model;
        }

        private static Constants.COMPONENT_PHASE GetEnumComponentPhase(string phase)
        {
            switch (phase)
            {
                case "CON": return Constants.COMPONENT_PHASE.CONCEPTUAL;
                case "ACT": return Constants.COMPONENT_PHASE.ACTIVE;
                case "NFN": return Constants.COMPONENT_PHASE.NOT_FOR_NEW_PRODUCT;
                case "NRN": return Constants.COMPONENT_PHASE.NOT_RECOMMENDED;
                case "DISC": return Constants.COMPONENT_PHASE.DISCONTINUED;
                case "OBS": return Constants.COMPONENT_PHASE.OBSOLETE;
                case "DEAD": return Constants.COMPONENT_PHASE.DEAD;
                default: return Constants.COMPONENT_PHASE.NOT_SET;
            }
        }
        private static Constants.PRODUCT_PHASE GetEnumProductPhase(string phase)
        {
            switch (phase)
            {
                case "CON": return Constants.PRODUCT_PHASE.CONCEPTUAL;
                case "DEV": return Constants.PRODUCT_PHASE.DEVELOPMENT;
                case "ACT": return Constants.PRODUCT_PHASE.ACTIVE;
                case "POUT": return Constants.PRODUCT_PHASE.PHASE_OUT;
                case "ROUT": return Constants.PRODUCT_PHASE.RUN_OUT;
                case "RET": return Constants.PRODUCT_PHASE.RETIRED;
                default: return Constants.PRODUCT_PHASE.NOT_SET;
            }
        }

        private static Constants.CURRENCY_TYPE GetEnumCurrency(string prefix)
        {
            if (Enum.IsDefined(typeof(Constants.CURRENCY_TYPE), prefix.ToUpper()))
            {
                return (Constants.CURRENCY_TYPE)Enum.Parse(typeof(Constants.CURRENCY_TYPE), prefix);
            }
            return Constants.CURRENCY_TYPE.USD;
            
        }
    }

    public static class PriceExtension
    {
        public static bool IsEligible(decimal value)
        {
            return value != NO_ACCESS;
        }
        public static bool IsEligible(this PriceEntity p)
        {
            return IsEligible(p.value);
        }
        public static string GetPrice(this PriceEntity p)
        {
            if (p.value == NO_ACCESS)
            {
                string call = Translation.GetTranslation(Translation.SECTION.PRODUCT, 354);
                return call;
            }
            return FormatCurrency(p.value, p.currency);
        }
    }
}