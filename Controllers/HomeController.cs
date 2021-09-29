using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Tst.Models;
using WebApp.Tst.Models.Data;

namespace WebApp.Tst.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebAppDbContext _dbContext;

        public HomeController(WebAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            if (_dbContext.PARTNER.ToList().Count == 0)
                GenerateData();

            IQueryable<PARTNER> partnersGrid = _dbContext.PARTNER;

            return View(partnersGrid);
        }

        public IActionResult FinancialItem()
        {
            return View();
        }

        public IActionResult Commission() 
        {
            return View();
        }

        public ActionResult GetPartnerData() 
        {
            var partnersGrid = _dbContext.PARTNER.Select(x => new { 
                partnerid =  x.PARTNER_ID,
                partnername= x.PARTNER_NAME,
                fee = x.FEE_PERCENT,
                parentpartnername = x.PARENT_PARTNER.PARTNER_NAME
            } );
            return Json(new { result = partnersGrid.ToList(), count = partnersGrid.Count() });
        }

        public ActionResult GetFinancialItems() 
        {
            var financialGrid = _dbContext.FINANCIAL_ITEM.Select(x => new {
                x.financialitem_id,
                x.partner_id,
                x.date,
                x.amount
            });
            return Json(new { result = financialGrid, count = financialGrid.Count() });
        }

        public ActionResult CreateFinancialItem([FromBody] CRUDModel<FINANCIAL_ITEM> financialItem) 
        {
            _dbContext.Add(financialItem.value);
            _dbContext.SaveChanges();
            return Json(financialItem);
        }
        public ActionResult UpdateFinancialItem([FromBody] CRUDModel<FINANCIAL_ITEM> financialItem)
        {
            _dbContext.Update(financialItem.value);
            _dbContext.SaveChanges();
            return Json(financialItem.value);
        }
        public ActionResult RemoveFinancialItem([FromBody] DeleteModel financialItem)
        {
            
            var financeItemEntity = _dbContext.FINANCIAL_ITEM.Where(x => x.financialitem_id == financialItem.key).FirstOrDefault();
            _dbContext.Remove(financeItemEntity);
            _dbContext.SaveChanges();
            return Json(financeItemEntity);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public void GenerateData()
        {
            List<PARTNER> partners = new List<PARTNER>();
            var parentPartnerCnt = 3;
            var partnerMaxCnt = 11;

            #region Generate Parent Partner

            for (var cnt = 1; cnt <= parentPartnerCnt; cnt++)
            {
                Random random = new Random();
                var partner = new PARTNER()
                {
                    PARTNER_ID = cnt,
                    PARTNER_NAME = String.Format("{0}-{1}", "PARTNER", cnt.ToString()),
                    FEE_PERCENT = (decimal)Math.Round((decimal)(random.Next(1,19) + random.NextDouble()), 3),
                };
                partners.Add(partner);
            }

            #endregion

            #region Generate Child Data

            for (var i = 4; i <= partnerMaxCnt; i++)
            {
                for (var cnt = 1; cnt <= parentPartnerCnt; cnt++)
                {
                    if (i == 11)
                        break;
                    Random random = new Random();
                    var partner = new PARTNER()
                    {
                        PARTNER_ID = i,
                        PARTNER_NAME = String.Format("{0}-{1}", "PARTNER", i.ToString()),
                        FEE_PERCENT = (decimal)Math.Round((random.Next(1, 19) + random.NextDouble()), 3),
                        PARENT_PARTNER = partners.Where<PARTNER>(p => p.PARTNER_ID.Equals(cnt)).FirstOrDefault(),
                        PARENT_PARTNER_ID = cnt
                    };
                    i++;
                    partners.Add(partner);
                }
                if (i == 11)
                    break;
                i--;
            }

            #endregion

            _dbContext.AddRange(partners);
            _dbContext.SaveChanges();
        }

        private decimal GetTeamShoppingAmount(decimal PartnerId) 
        {
            var partnerIds = _dbContext.PARTNER.Where(x => x.PARENT_PARTNER_ID == PartnerId).Select(s => s.PARTNER_ID).ToList();
            var sumAmount = _dbContext.FINANCIAL_ITEM.Where(y => partnerIds.Contains(y.partner_id)).Sum(x => x.amount);
            return sumAmount;
        }

        private decimal GetTotalShoppingAmount(decimal PartnerId)
        {
            var ownAmout = _dbContext.FINANCIAL_ITEM.Where(x => x.partner_id.Equals(PartnerId)).Select(x => x.amount).FirstOrDefault();
            var teamShoppingAmout = GetTeamShoppingAmount(PartnerId);
            var total = ownAmout + teamShoppingAmout;
            return (teamShoppingAmout + ownAmout);
        }


        private decimal GetPartnerBonus(decimal PartnerId)
        {
            var ownAmout = _dbContext.FINANCIAL_ITEM.Where(x => x.partner_id.Equals(PartnerId)).Select(x => x.amount).FirstOrDefault();
            var percentFee = _dbContext.PARTNER.Where(x => x.PARTNER_ID.Equals(PartnerId)).Select(s => s.FEE_PERCENT).FirstOrDefault();

            var fee = (ownAmout * percentFee);

            return fee;
        }

        private decimal GetTotalCommission(decimal PartnerId)
        {
            var totalTeamAmount = GetTotalShoppingAmount(PartnerId);
            var ownFeePercent = _dbContext.PARTNER.Where(x => x.PARTNER_ID.Equals(PartnerId)).Select(y => y.FEE_PERCENT).FirstOrDefault();
            var partnerIds = _dbContext.PARTNER.Where(x => x.PARENT_PARTNER_ID == PartnerId).Select(s => s.PARTNER_ID).ToList();
            var totalCommission = 0m;

            if (partnerIds.Count > 0)
            {
                _dbContext.PARTNER.Where(y => partnerIds.Contains(y.PARTNER_ID)).Select(x => x.FEE_PERCENT).ToList().ForEach(fe =>
                {
                    if (ownFeePercent > fe)
                    {
                        totalCommission += (totalTeamAmount * (ownFeePercent - fe));
                    }
                    else {
                        totalCommission += (totalTeamAmount * (fe));
                    }
                });

                return totalCommission > 0 ? totalCommission / 100 : totalCommission;
            }
            else {
                totalCommission = (totalTeamAmount * ownFeePercent);

                return totalCommission > 0 ? totalCommission / 100 : totalCommission;
            }
           
        }

        public ActionResult GetCommissions() 
        {
            var partnerList = _dbContext.PARTNER;
            List<Commissions> commissions = new List<Commissions>();
            foreach (var partner in partnerList)
            {
                var totalCommission = GetTotalCommission(partner.PARTNER_ID);
                var totalTeamPO = GetTeamShoppingAmount(partner.PARTNER_ID);
                var totalPO = GetTotalShoppingAmount(partner.PARTNER_ID);
                var commission = new Commissions()
                {
                    partner_id = partner.PARTNER_ID,
                    totalCommission = totalCommission,
                    totalTeamPO = totalTeamPO,
                    totalPO = totalPO,
                    partner_name = partner.PARTNER_NAME
                };

                commissions.Add(commission);
            }

            return Json(new { result = commissions.ToList(), count = commissions.Count() });
        }
    }
}
