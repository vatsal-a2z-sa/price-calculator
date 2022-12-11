using supplier_base.Models.Common;

namespace price_calculator
{
    public class CalculatePrice
    {
        public List<MarkupDetails> markupDetails { get; set; }

        //public Dictionary<string, string> MarkupConfigs { get; set; }

        public MarkupCalculateParams markupCalculateParams { get; set; }

        public CalculatePrice(List<MarkupDetails> markupDetails, MarkupCalculateParams markupCalculateParams)
        {
            this.markupDetails = markupDetails;
            this.markupCalculateParams = markupCalculateParams;
        }
        public async Task<List<RateInfo>> process()
        {
            //TODO : Round off Related changes
            decimal agentBaseAmount = 0;
            decimal agentTaxAmount = 0;
            decimal portalMarkup = 0;
            decimal agentMarkup = 0;

            List<RateInfo> rateInfo = new List<RateInfo>();
            //supplier base amount
            rateInfo.Add(new RateInfo()
            {
                Amount = Convert.ToDecimal(String.Format("{0:.##}", markupCalculateParams.supplierAmount)),
                CurrencyCode = markupCalculateParams.supplierCurrency,
                Description = ChargeType.supplierAmount.ToString(),
                Purpose = ((int)ChargeType.supplierAmount).ToString(),
            });

            if (markupCalculateParams.supplierCurrency == markupCalculateParams.agentCurrency)
            {
                //agent base amount
                rateInfo.Add(new RateInfo()
                {
                    Amount = Convert.ToDecimal(String.Format("{0:.##}", markupCalculateParams.supplierAmount)),
                    CurrencyCode = markupCalculateParams.supplierCurrency,
                    Description = ChargeType.baseAmount.ToString(),
                    Purpose = ((int)ChargeType.baseAmount).ToString(),
                });
                agentBaseAmount = markupCalculateParams.supplierAmount;
            }
            else
            {
                //TODO : Get currency conversion factor, and convert amount to agent currency
            }

            if (markupDetails != null && markupDetails.Where(x => x.isPortalAgent).Count() > 0)
            {
                portalMarkup = getCalculatedAmount(agentBaseAmount + agentTaxAmount, markupDetails.Where(x => x.isPortalAgent).FirstOrDefault());
                //portal markup
                rateInfo.Add(new RateInfo()
                {
                    Amount = Convert.ToDecimal(String.Format("{0:.##}", portalMarkup)),
                    CurrencyCode = markupCalculateParams.agentCurrency,
                    Description = ChargeType.parentMarkup.ToString(),
                    Purpose = ((int)ChargeType.parentMarkup).ToString(),
                });
            }

            if (markupDetails != null && markupDetails.Where(x => !x.isPortalAgent).Count() > 0)
            {
                agentMarkup = getCalculatedAmount(agentBaseAmount + agentTaxAmount + portalMarkup, markupDetails.Where(x => !x.isPortalAgent).FirstOrDefault());
                //agent markup
                rateInfo.Add(new RateInfo()
                {
                    Amount = Convert.ToDecimal(String.Format("{0:.##}", agentMarkup)),
                    CurrencyCode = markupCalculateParams.agentCurrency,
                    Description = ChargeType.agentMarkup.ToString(),
                    Purpose = ((int)ChargeType.agentMarkup).ToString(),
                });
            }
            //total amount
            rateInfo.Add(new RateInfo()
            {
                Amount = Convert.ToDecimal(String.Format("{0:.##}", agentBaseAmount + agentTaxAmount + portalMarkup + agentMarkup)),
                CurrencyCode = markupCalculateParams.agentCurrency,
                Description = ChargeType.totalAmount.ToString(),
                Purpose = ((int)ChargeType.totalAmount).ToString(),
            });

            return rateInfo;
        }

        public decimal getCalculatedAmount(decimal amount, MarkupDetails markupDetails)
        {
            decimal calculatedAmount = 0;
            if (markupDetails.markupType == "percentage")
            {
                calculatedAmount = amount * (markupDetails.markupValue / 100);
            }
            else if (markupDetails.markupType == "fixed")
            {
                calculatedAmount = markupDetails.markupValue;
            }
            return calculatedAmount;
        }
    }
}