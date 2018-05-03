using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateStudyQuiz
{
    public class StorageEntitiesNameResolver : INameResolver
    {
        public string Resolve(string name)
        {
            if (name == "StudiesContainer")
            {
                return String.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")) ? "studies-local" : "studies";
            }

            throw new ArgumentOutOfRangeException("name");
        }
    }
}
