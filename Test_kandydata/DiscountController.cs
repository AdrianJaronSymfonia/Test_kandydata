using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test_kandydata;

namespace mplerpsw.DAL.Controllers
{
    public class DiscountController
    {
        /// <summary>
        /// rabat indywidualny produktu dla kontrahenta
        /// </summary>
        private decimal _productDiscounts;
        /// <summary>
        /// cena produktu po rabacie indywidualnym produktu dla kontrahenta
        /// </summary>
        private decimal _netPriceAfterProductDiscount;
        /// <summary>
        /// rabat kontrahenta
        /// </summary>
        private decimal _contractorDiscount;
        /// <summary>
        /// cena produktu po rabacie kontrahenta
        /// </summary>
        private decimal _netPriceAfterContrctorDiscount;
        /// <summary>
        /// suma rabatu indywidualnego produktu i rabatu kontrahenta
        /// </summary>
        private decimal _sumLineDiscounts;
        /// <summary>
        /// Obiekt z cennika toawru
        /// </summary>
        private SF_PRICEBOOKS_GROSS _pricebook;

        public decimal ProductDiscounts
        {
            get
            {
                return _productDiscounts;
            }
        }

        public decimal NetPriceAfterProductDiscount
        {
            get
            {
                return _netPriceAfterProductDiscount;
            }
        }

        public decimal ContractorDiscount
        {
            get
            {
                return _contractorDiscount;
            }
        }

        public decimal NetPriceAfterContrctorDiscount
        {
            get
            {
                return _netPriceAfterContrctorDiscount;
            }
        }

        public decimal SumLineDiscounts
        {
            get
            {
                return _sumLineDiscounts;
            }
        }

        public SF_PRICEBOOKS_GROSS Pricebook
        {
            get
            {
                return _pricebook;
            }
        }

        private List<hm_GetUpustIndywResult> GetAllDiscountsForProduct(int contractorId, int productId)
        {
            List<hm_GetUpustIndywResult> result;

            using (HmfModelDataContext dtc = mplerpsw.DAL.Controllers.DatabaseController.GetDatabaseContext())
            {
                //Pobiera upust indywidualny kontrahent na konkretny produkt
                ISingleResult<hm_GetUpustIndywResult> query = dtc.hm_GetUpustIndyw(productId, contractorId, 0, 0, 1, 5);
                result = query.ToList();
            }

            return result;
        }

        private double GetDiscountForProduct(int contractorId, int productId, double netPrice, out string message)
        {
            double rep = 0;
            message = string.Empty;
            List<hm_GetUpustIndywResult> dicounts = GetAllDiscountsForProduct(contractorId, productId);

            if (dicounts != null)
            {
                //Jeżeli jest cena indywidualnej towaru dla kontrahenta
                double totalValueDiscount = 0;
                //Cena wyrażona w kwocie
                IEnumerable<hm_GetUpustIndywResult> ttvd = dicounts.Where(d => d.waluta.Equals("PLN"));
                if (ttvd != null)
                {
                    totalValueDiscount = ttvd.Sum(r => r.cena);
                }

                double totalPercentageDiscount = 0;
                //Upust wyrażony w procencie
                IEnumerable<hm_GetUpustIndywResult> tpd = dicounts.Where(d => d.waluta.Equals("%"));
                if (tpd != null)
                {
                    totalPercentageDiscount = tpd.Sum(r => r.cena);
                }

                double percentageDiscount = 0;
                if (totalPercentageDiscount > 0)
                {
                    percentageDiscount = netPrice * (totalPercentageDiscount / 100);
                }

                rep = totalValueDiscount + percentageDiscount;
                if (rep > netPrice)
                {
                    message = Test_kandydata.Properties.Resources.NetPriceIsBelowZeroAfterDiscount;
                }
            }

            return rep;
        }

        private decimal GetDiscountForContractor(int contractorId, decimal price, out string message)
        {
            message = string.Empty;
            decimal rep = 0;
            List<hm_GetUpustIndywResult> result = new List<hm_GetUpustIndywResult>();
            using (HmfModelDataContext dtc = mplerpsw.DAL.Controllers.DatabaseController.GetDatabaseContext())
            {
                //Pobiera rabat dla kontrahenta.
                ISingleResult<hm_GetUpustIndywResult> query = dtc.hm_GetUpustIndyw(0, contractorId, 0, 0, 0, 15);
                result = query.ToList();
            }

            if (result.Count() > 0)
            {
                double discount = result.Where(d => d.waluta.Equals("%")).First().cena;
                if (discount > 0)
                {
                    rep = (price * (decimal)(discount / 100));
                }
            }

            return rep;
        }

        public bool CalculatePricesForProspect(string prospectType, string productCode, out List<string> messages)
        {
            bool rep = false;
            messages = new List<string>();
            string errMessage;

            // TODO: narazie nie ma znaczenia co jest przesyłąne jako prospekt - zawsze jest brany standardowy cennik
            prospectType = "Standardowy";

            //Pobiera rekord cennika dla towaru
            _pricebook = ProductController.GetProductPricebook(productCode, prospectType, out errMessage);

            if (_pricebook.id > 0)
            {
                #region RABATY INDYWIDUALNE DLA TOWARÓW
                errMessage = string.Empty;
                double discount = 0;

                _productDiscounts = Convert.ToDecimal(discount);
                _netPriceAfterProductDiscount = (decimal)_pricebook.NettUnitPrice - _productDiscounts;

                if (!string.IsNullOrEmpty(errMessage))
                {
                    messages.Add(errMessage);
                }
                #endregion

                #region RABATY DLA KONTRAHENTA
                errMessage = string.Empty;
                decimal discountForContractor = 0;
                if (discountForContractor > 0)
                {
                    _contractorDiscount = Convert.ToDecimal(discountForContractor);
                    _netPriceAfterContrctorDiscount = _netPriceAfterProductDiscount - _contractorDiscount;
                }
                else
                {
                    _contractorDiscount = 0;
                    _netPriceAfterContrctorDiscount = _netPriceAfterProductDiscount;
                }

                rep = true;
                #endregion
            }
            else
            {
                messages.Add(errMessage);
                rep = false;
            }

            _sumLineDiscounts = _productDiscounts + _contractorDiscount;

            return rep;
        }

        /// <summary>
        /// Oblicza cenę towaru dla kontrahenta. Obecnie szuka w cenniku, pobiera cenę indywidualną i odejmuje ją od ceny z cennika, potem pobiera upust kontrahenta i odejmuje go od wcześniej obliczonej ceny cennika i ceny indywidualnej.
        /// Trzeba zmodyfikować, aby metoda zwracała cenę z cennika jeśli jest. Jeśli nie ma to brak informacji o cenie. 
        /// Jeśli jest upust indywidualny kontrahenta to zwracamy cenę po tym upuście. 
        /// Jeśli nie ma ceny indywidualnej to pobieramy rabat kontrahenta i obliczmy cenę dla towaru.
        /// </summary>
        /// <param name="contractorId"></param>
        /// <param name="productCode"></param>
        /// <param name="requestedQuantity"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public bool CalculatePrices(int contractorId, string productCode, decimal requestedQuantity, out List<string> messages)
        {
            bool rep = false;
            messages = new List<string>();
            string errMessage;

            _pricebook = ProductController.GetProductPricebook(productCode, contractorId, out errMessage);

            if (!string.IsNullOrWhiteSpace(errMessage))
            {
                messages.Add(errMessage);
                rep = false;
            }
            
            // zakładamy że towary są zawsze w cenniku
            if (_pricebook.id > 0)
            {

                #region RABATY INDYWIDUALNE DLA TOWARÓW
                errMessage = string.Empty;
                double discount = GetDiscountForProduct(contractorId, _pricebook.TwId, _pricebook.NettUnitPrice, out errMessage);

                _productDiscounts = Convert.ToDecimal(discount);
                _netPriceAfterProductDiscount = (decimal)_pricebook.NettUnitPrice - _productDiscounts;

                if (!string.IsNullOrWhiteSpace(errMessage))
                {
                    messages.Add(errMessage);
                }
                #endregion

                #region RABATY DLA KONTRAHENTA
                errMessage = string.Empty;
                decimal discountForContractor = GetDiscountForContractor(contractorId, _netPriceAfterProductDiscount, out errMessage);
                if (discountForContractor > 0)
                {
                    _contractorDiscount = Convert.ToDecimal(discountForContractor);
                    _netPriceAfterContrctorDiscount = _netPriceAfterProductDiscount - _contractorDiscount;
                }
                else
                {
                    _contractorDiscount = 0;
                    _netPriceAfterContrctorDiscount = _netPriceAfterProductDiscount;
                }

                rep = true;

                #endregion
            }
            else
            {
                //if (!string.IsNullOrWhiteSpace(errMessage))
                //{
                //    messages.Add(errMessage);
                //    rep = false;
                //}

                // jeśli towaru nie ma w cenniku to ceny i rabaty 0

                _contractorDiscount = 0;
                _productDiscounts = 0;
                _netPriceAfterContrctorDiscount = 0;
                _productDiscounts = 0;

                rep = true;
            }

            _sumLineDiscounts = _productDiscounts + _contractorDiscount;

            double stan = _pricebook.stan.HasValue ? _pricebook.stan.Value : 0;

            if (requestedQuantity > (decimal)stan)
            {
                messages.Add(Test_kandydata.Properties.Resources.ProductIsOutOfStock);
            }

            return rep;
        }

        public void ClearCalculations()
        {
            _productDiscounts = 0;
            _netPriceAfterProductDiscount = 0;
            _contractorDiscount = 0;
            _netPriceAfterContrctorDiscount = 0;
            _sumLineDiscounts = 0;
            _pricebook = null;
        }
    }
}
