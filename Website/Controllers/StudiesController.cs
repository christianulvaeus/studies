using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Website.Models;

namespace Website.Controllers
{
    public class StudiesController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var report = LoadReport();

            ViewBag.Total = report.Count;
            ViewBag.NeverAttempted = report.Sum(article => Convert.ToInt32(article.NeverAttemped));
            ViewBag.HighFailureQuota = report.Sum(article => Convert.ToInt32(article.HighFailureQuota));
            ViewBag.Revised = report.Count(article => article.Revise);

            return View();
        }
        
        [HttpPost]
        public ActionResult Begin(int numberOfQuestions)
        {
            var document = LoadReport();

            Study.Prepare(document, numberOfQuestions);

            return RedirectToAction("Question", new { index = 0 });
        }

        [HttpPost]
        public ActionResult Reset()
        {
            var report = LoadReport();
            report.ForEach(article => article.Reset());
            SaveReport(report);

            return RedirectToAction("Index", new { message = "Studierapporten har återställts." });
        }

        [HttpGet]
        public ActionResult Question(int index)
        {
            ViewData.Model = Study.Articles[index];

            return View();
        }

        [HttpPost]
        public ActionResult Question(int index, string result)
        {
            switch (result)
            {
                case "Rätt":
                    Study.Articles[index].SetLastResult(StudyArticleResult.Success);
                    break;
                case "Fel":
                    Study.Articles[index].SetLastResult(StudyArticleResult.Failure);
                    break;
                case "Revidera":
                    Study.Articles[index].SetLastResult(StudyArticleResult.Revise);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("value");
            }


            index++;

            if (index >= Study.Articles.Count)
            {
                SaveReport(Study.Articles);

                if (Study.Repeat())
                {
                    return RedirectToAction("Question", new { index = 0, message = "Nu går vi igenom alla frågor där du hade fel en gång till." });
                }
                else
                {
                    return RedirectToAction("Index", new { message = "Grattis! Nu har du klarat alla frågor." });
                }
            }
            else
            {
                return RedirectToAction("Question", new { index = index });
            }
        }

        [HttpGet]
        public ActionResult Revise()
        {
            var report = LoadReport();

            ViewBag.Articles = report.Where(article => article.Revise).ToList();

            return View();
        }

        public StudySession Study
        {
            get
            {
                if (Session["Studies"] == null)
                {
                    Session["Studies"] = new StudySession();
                }

                return (StudySession)Session["Studies"];
            }
        }

        private void SaveReport(IReadOnlyList<StudyArticle> articles)
        {
            // Load report
            var lastReport = LoadReport();

            // Update report
            foreach (var studiedArticle in articles)
            {
                var saved = lastReport.First(savedArticle => savedArticle.Question == studiedArticle.Question);
                lastReport[lastReport.IndexOf(saved)] = studiedArticle;
            }

            // Upload report
            var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureStorage"]);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("studies");
            var blob = container.GetBlockBlobReference("StudyReport.json");
            blob.Properties.ContentType = "application/json";

            using (var writer = new StreamWriter(blob.OpenWrite()))
            {
                writer.Write(JsonConvert.SerializeObject(lastReport));
            }
        }

        private List<StudyArticle> LoadReport()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureStorage"]);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("studies");
            var blob = container.GetBlockBlobReference("StudyReport.json");

            using (var reader = new StreamReader(blob.OpenRead()))
            {
                return JsonConvert.DeserializeObject<List<StudyArticle>>(reader.ReadToEnd());
            }
        }
    }
}