using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFund.Data;

namespace SimchaFund.Web.Models
{
    public class HistoryViewModel
    {
        public IEnumerable<Transaction> Transactions { get; set; }
        public string ContributorName { get; set; }
        public decimal ContributorBalance { get; set; }
    }
}