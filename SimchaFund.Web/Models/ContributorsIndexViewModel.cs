using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFund.Data;

namespace SimchaFund.Web.Models
{
    public class ContributorsIndexViewModel
    {
        public IEnumerable<Contributors> Contributors { get; set; }

    }
}