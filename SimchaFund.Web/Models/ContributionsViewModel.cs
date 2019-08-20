using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFund.Data;

namespace SimchaFund.Web.Models
{
    public class ContributionsViewModel
    {
        public Simchas Simcha { get; set; }
        public IEnumerable<SimchaContributor> Contributors { get; set; }
    }
}