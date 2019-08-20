using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SimchaFund.Data;
using SimchaFund.Web.Models;

namespace SimchaFund.Web.Controllers
{
    public class SimchasController : Controller
    {
        public ActionResult Index()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            var viewModel = new SimchaIndexViewModel();
            viewModel.TotalContributors = mgr.GetContributorCount();
            viewModel.Simchas = mgr.GetAllSimchas();
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult New(Simchas simcha)
        {
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            mgr.AddSimcha(simcha);
            TempData["message"] = $"New Simcha Created! Id: {simcha.Id}";
            return RedirectToAction("Index");
        }

        public ActionResult Contributions(int simchaId)
        {
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            Simchas simcha = mgr.GetSimchaById(simchaId);
            IEnumerable<SimchaContributor> contributors = mgr.GetSimchaContributorsOneQuery(simchaId);

            var viewModel = new ContributionsViewModel
            {
                Contributors = contributors,
                Simcha = simcha
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult UpdateContributions(List<ContributionInclusion> contributors, int simchaId)
        {
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            mgr.UpdateSimchaContributions(simchaId, contributors);
            TempData["Message"] = "Simcha updated successfully";
            return RedirectToAction("Index");
        }

    }
}