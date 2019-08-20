using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFund.Data;

namespace SimchaFund.Web.Models
{
    public class SimchaIndexViewModel
    {
        public int TotalContributors { get; set; }
        public IEnumerable<Simchas> Simchas { get; set; }
    }
}