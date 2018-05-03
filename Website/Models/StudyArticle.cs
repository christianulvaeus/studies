using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Globalization;

namespace Website.Models
{
    public class StudyArticle
    {
        [JsonProperty]
        public string Question { get; private set; }

        [JsonProperty]
        public string Answer { get; private set; }

        [JsonProperty]
        public int Successes { get; private set; }

        [JsonProperty]
        public int Failures { get; private set; }

        [JsonProperty]
        public bool Revise { get; private set; }

        [JsonIgnore]
        public bool Passed { get; private set; }

        [JsonIgnore]
        public bool NeverAttemped
        {
            get { return Successes + Failures == 0; }
        }

        [JsonIgnore]
        public bool HighFailureQuota
        {
            get
            {
                return Failures > 3 ? 
                    Failures > 0.7 * Successes :
                    Failures >= Math.Max(1, Successes);
            }
        }

        public void SetLastResult(StudyArticleResult value)
        {
            switch (value)
            {
                case StudyArticleResult.Success:
                    Passed = true;
                    Successes++;
                    break;
                case StudyArticleResult.Failure:
                    Passed = false;
                    Failures++;
                    break;
                case StudyArticleResult.Revise:
                    Passed = true;
                    Revise = true;
                    break;
            }
        }

        internal void Reset()
        {
            Successes = 0;
            Failures = 0;
        }
    }
    
    public enum StudyArticleResult
    {
        Success,
        Failure,
        Revise
    }    
}