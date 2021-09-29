using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Tst.Models.Data
{
    public class Commissions
    {
        public decimal partner_id { get; set; }

        public string partner_name { get; set; }

        public decimal totalTeamPO { get; set; }

        public decimal totalPO { get; set; }

        public decimal totalCommission { get; set; }
    }
}
