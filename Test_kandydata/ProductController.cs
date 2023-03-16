using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using mplerpsw.WSEntities.Common;
using NLog;
using Test_kandydata;

namespace mplerpsw.DAL.Controllers
{
    public static class ProductController
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        internal static TW GetProduct(string productCode, out string message)
        {
            message = string.Empty;
            TW rep = new TW();
            try
            {
                if (!string.IsNullOrEmpty(productCode))
                {
                    TW tmp;
                    using (HmfModelDataContext dtc = mplerpsw.DAL.Controllers.DatabaseController.GetDatabaseContext())
                    {
                        tmp = dtc.TWs.Where(c => c.kod.Equals(productCode) && c.aktywny == true).FirstOrDefault();
                    }

                    if (tmp == null)
                    {
                        message = Test_kandydata.Properties.Resources.ProductIsNotInTheDatabase;
                        rep.id = -1;
                    }
                    else
                    {
                        rep = tmp;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                rep.id = -1;
                message = Test_kandydata.Properties.Resources.ConnectionErrorDAL;
            }


            return rep;
        }

        /// <summary>
        /// Pobiera cenę dla towaru dla konkretnego kontrahenta.
        /// </summary>
        /// <param name="productCode"></param>
        /// <param name="contractorId"></param>
        /// <param name="mesage"></param>
        /// <returns></returns>
        internal static SF_PRICEBOOKS_GROSS GetProductPricebook(string productCode, int contractorId, out string mesage)
        {
            mesage = string.Empty;
            SF_PRICEBOOKS_GROSS rep = new SF_PRICEBOOKS_GROSS();

            try
            {
                int cntId = contractorId;
                using (HmfModelDataContext dbc = DatabaseController.GetDatabaseContext())
                {
                    IQueryable<SF_PRICEBOOKS_GROSS> query = from a in dbc.SF_ACCOUNTs
                                                     join p in dbc.SF_PRICEBOOKS_GROSSes on a.Lookup_Pricebook equals p.Lookup_Pricebook
                                                     where a.Id == cntId && p.Lookup_ProductCode.Equals(productCode) && p.IsActive == true
                                                     select (p);

                    if (query != null)
                    {
                        if (query.Count() >= 1)
                        {
                            rep.CurrencyIsoCode = query.First().CurrencyIsoCode;
                            rep.id = query.First().id;
                            rep.IsActive = query.First().IsActive;
                            rep.Lookup_Pricebook = query.First().Lookup_Pricebook;
                            rep.Lookup_ProductCode = query.First().Lookup_ProductCode;
                            rep.NettUnitPrice = (query.Where(c => c.NettUnitPrice != 0).FirstOrDefault() == null) ? 0 : query.Where(c => c.NettUnitPrice != 0).FirstOrDefault().NettUnitPrice;
                            rep.GrossUnitPrice = (query.Where(c => c.GrossUnitPrice != 0).FirstOrDefault() == null) ? 0 : query.Where(c => c.GrossUnitPrice != 0).FirstOrDefault().GrossUnitPrice;
                            rep.typ = query.First().typ;
                            rep.vat = query.First().vat;
                            rep.stan = query.First().stan;
                            rep.nazwa = query.First().nazwa;
                            rep.TwId = query.First().TwId;
                            //TODO: dodać komunikat o zerowym stanie towaru
                        }
                        else
                        {
                            if (CheckIsProductExists(productCode))
                            {
                                mesage = Test_kandydata.Properties.Resources.ProductCantBeFoundInPricebook;
                            }
                            else
                            {
                                mesage = Test_kandydata.Properties.Resources.ProductIsNotInTheDatabase;
                            }
                            
                            rep.id = -1;
                        }
                    }
                    else
                    {
                        if (CheckIsProductExists(productCode))
                        {
                            mesage = Test_kandydata.Properties.Resources.ProductCantBeFoundInPricebook;
                        }
                        else
                        {
                            mesage = Test_kandydata.Properties.Resources.ProductIsNotInTheDatabase;
                        }

                        rep.id = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                mesage = Test_kandydata.Properties.Resources.ConnectionErrorDAL;
            }

            return rep;
        }

        private static bool CheckIsProductExists(string productCode)
        {
            bool rep = false;
            try
            {
                using (HmfModelDataContext dbc = DatabaseController.GetDatabaseContext())
                {
                    TW product = dbc.TWs.Where(c => c.kod.Equals(productCode) && c.aktywny == true).FirstOrDefault();
                    if(product != null)
                    {
                        if(product.id > 0)
                        {
                            rep = true;
                        }
                        else
                        {
                            rep = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return rep;
        }

        public static SF_PRICEBOOKS_GROSS GetProductPricebook(string productCode, string prospectType, out string mesage)
        {
            mesage = string.Empty;
            SF_PRICEBOOKS_GROSS rep = new SF_PRICEBOOKS_GROSS();

            try
            {
                using (HmfModelDataContext dbc = DatabaseController.GetDatabaseContext())
                {
                    IQueryable<SF_PRICEBOOKS_GROSS> query = from p in dbc.SF_PRICEBOOKS_GROSSes
                                                            where p.Lookup_Pricebook.Equals(prospectType) && p.Lookup_ProductCode.Equals(productCode) && p.IsActive == true
                                                            select (p);

                    if (query != null)
                    {
                        if (query.Count() >= 1)
                        {
                            rep.CurrencyIsoCode = query.First().CurrencyIsoCode;
                            rep.id = query.First().id;
                            rep.IsActive = query.First().IsActive;
                            rep.Lookup_Pricebook = query.First().Lookup_Pricebook;
                            rep.Lookup_ProductCode = query.First().Lookup_ProductCode;
                            rep.NettUnitPrice = (query.Where(c => c.NettUnitPrice != 0).FirstOrDefault() == null) ? 0 : query.Where(c => c.NettUnitPrice != 0).FirstOrDefault().NettUnitPrice;
                            rep.GrossUnitPrice = (query.Where(c => c.GrossUnitPrice != 0).FirstOrDefault() == null) ? 0 : query.Where(c => c.GrossUnitPrice != 0).FirstOrDefault().GrossUnitPrice;
                            rep.typ = query.First().typ;
                            rep.vat = query.First().vat;
                            rep.stan = query.First().stan;
                            rep.nazwa = query.First().nazwa;
                            rep.TwId = query.First().TwId;
                        }
                    }
                    else
                    {
                        mesage = Test_kandydata.Properties.Resources.ProductIsNotInTheDatabase;
                        rep.id = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                mesage = Test_kandydata.Properties.Resources.ConnectionErrorDAL;
            }

            return rep;
        }
    }
}
