using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class PARTNER
    {
        [Key]
        public decimal PARTNER_ID { get; set; }
        public string PARTNER_NAME { get; set; }
        public decimal? PARENT_PARTNER_ID { get; set; }
        public virtual PARTNER PARENT_PARTNER { get;set;}
        public decimal FEE_PERCENT { get; set; }
    }
}
