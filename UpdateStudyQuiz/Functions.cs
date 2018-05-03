using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;

namespace UpdateStudyQuiz
{
    public class Functions
    {
        public static void ProcessQueueMessage([BlobTrigger("%StudiesContainer%/StudyMaterial.html")] string studyMaterialInput, TextWriter log, [Blob("%StudiesContainer%/StudyReport.json", FileAccess.Write)] out string studyMaterialJsonOutput)
        {
            var nameResolver = new StorageEntitiesNameResolver();

            var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureStorage"]);
            var studyReportOriginalBlob = new CloudBlockBlob(new Uri(account.BlobEndpoint.AbsoluteUri + nameResolver.Resolve("StudiesContainer") + "/StudyReport.json"), account.Credentials);
            bool studyReportOriginalExists = studyReportOriginalBlob.Exists();
            JArray studyReportOriginalJson = null;

            if (studyReportOriginalExists)
            {
                string json = studyReportOriginalBlob.DownloadText();

                if ((int)json[0] == 65279)
                {
                    json = json.Substring(1);
                }

                studyReportOriginalJson = JsonConvert.DeserializeObject<JArray>(json); 
            }

            var list = new JArray();
            JObject nextJson;

            var document = new HtmlDocument();
            document.LoadHtml(studyMaterialInput);

            string question;
            StringBuilder answer = new StringBuilder();
            var body = document.DocumentNode.SelectSingleNode("/html/body");
            HtmlNode next = body.FirstChild;

            TrySearchSibling(sibling => sibling.Name == "hr" && sibling.Attributes.Count == 0, ref next);

            while (true) // begins at clean "<hr />"
            {
                if (!TrySearchSibling(sibling => !String.IsNullOrWhiteSpace(sibling.InnerText), ref next))
                {
                    break;
                }
                
                question = next.StripAttributes().OuterHtml;

                answer.Clear();

                while (next.Name != "hr")
                {
                    next = next.NextSibling;

                    if (next == null)
                    {
                        break;
                    }

                    answer.Append(next.StripAttributes().OuterHtml);
                }

                if (next == null)
                {
                    break;
                }

                if (next.Attributes.Count > 0)
                {
                    if (!TrySearchSibling(sibling => sibling.Name == "hr" && sibling.Attributes.Count == 0, ref next))
                    {
                        break;
                    }
                }
                else
                {
                    if (studyReportOriginalExists && studyReportOriginalJson.Count > list.Count)
                    {
                        nextJson = (JObject)studyReportOriginalJson[list.Count];

                        if (question != nextJson.Value<string>("Question") || answer.ToString() != nextJson.Value<string>("Answer"))
                        {
                            nextJson["Successes"] = 0;
                            nextJson["Failures"] = 0;
                        }
                    }
                    else
                    {
                        nextJson = new JObject();
                    }

                    nextJson["Question"] = question;
                    nextJson["Answer"] = answer.ToString();
                    nextJson["Revise"] = false;

                    list.Add(nextJson); 
                }
            }

            studyMaterialJsonOutput = JsonConvert.SerializeObject(list, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }

        private static bool TrySearchSibling(Func<HtmlNode, bool> predicate, ref HtmlNode current)
        {
            HtmlNode nextInner = current.NextSibling;

            while (nextInner != null && !predicate.Invoke(nextInner))
            {
                nextInner = nextInner.NextSibling;
            }

            current = nextInner;

            return nextInner != null;
        }
    }
}