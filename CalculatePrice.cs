using supplier_base.Models.Common;

namespace price_calculator
{
    public class CalculatePrice
    {
        public List<MarkupDetails> markupDetails { get; set; }

        public Dictionary<string, string> MarkupConfigs { get; set; }

        public MarkupCalculateParams markupCalculateParams { get; set; }
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
                Amount = markupCalculateParams.supplierAmount,
                CurrencyCode = markupCalculateParams.supplierCurrency,
                Purpose = RateInfoPurpose.supplierAmount.ToString(),
            });

            if (markupCalculateParams.supplierCurrency == markupCalculateParams.agentCurrency)
            {
                //agent base amount
                rateInfo.Add(new RateInfo()
                {
                    Amount = markupCalculateParams.supplierAmount,
                    CurrencyCode = markupCalculateParams.supplierCurrency,
                    Purpose = RateInfoPurpose.baseAmount.ToString(),
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
                    Amount = portalMarkup,
                    CurrencyCode = markupCalculateParams.agentCurrency,
                    Purpose = RateInfoPurpose.portalMarkup.ToString(),
                });
            }

            if (markupDetails != null && markupDetails.Where(x => !x.isPortalAgent).Count() > 0)
            {
                agentMarkup = getCalculatedAmount(agentBaseAmount + agentTaxAmount + portalMarkup, markupDetails.Where(x => !x.isPortalAgent).FirstOrDefault());
                //agent markup
                rateInfo.Add(new RateInfo()
                {
                    Amount = agentMarkup,
                    CurrencyCode = markupCalculateParams.agentCurrency,
                    Purpose = RateInfoPurpose.portalMarkup.ToString(),
                });
            }
            //total amount
            rateInfo.Add(new RateInfo()
            {
                Amount = agentBaseAmount + agentTaxAmount + portalMarkup + agentMarkup,
                CurrencyCode = markupCalculateParams.agentCurrency,
                Purpose = RateInfoPurpose.totalAmount.ToString(),
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