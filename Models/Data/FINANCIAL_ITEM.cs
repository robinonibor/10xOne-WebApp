using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class FINANCIAL_ITEM
    {
        [Key]
        public int financialitem_id { get; set; }
        public decimal partner_id { get; set; }
        public DateTime date { get; set; }
        public decimal amount { get; set; }
    }
}
