using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace Website.Models
{
    public class StudySession
    {
        private List<StudyArticle> articles;        

        public StudySession()
        {
            this.articles = new List<StudyArticle>();
        }

        public int CurrentQuestion { get; set; }

        public IReadOnlyList<StudyArticle> Articles
        {
            get { return this.articles.AsReadOnly(); }
        }

        internal bool Repeat()
        {
            CurrentQuestion = 0;
            this.articles.RemoveAll(article => article.Passed);

            return this.articles.Count > 0;
        }

        internal void Prepare(List<StudyArticle> document, int amount)
        {            
            int remaining = amount;

            this.articles.Clear();

            foreach (var article in Randomize(document, amount))
            {
                this.articles.Add(article);
            }
        }

        private IEnumerable<StudyArticle> Randomize(List<StudyArticle> document, int amount)
        {
            var selected = new HashSet<StudyArticle>();
            var random = new Random();

            var published = document.Where(article => !article.Revise).ToList();
            int totalAttempts = published.Sum(article => article.Failures + article.Successes);
            double averageAttempts = totalAttempts / (double)published.Count;

            var neverAttempted = published.Where(article => article.NeverAttemped).ToList();
            var seldomAttempted = published.Where(article => (article.Failures + article.Successes) < averageAttempts).ToList();
            var highFailureQuota = published.Where(article => article.HighFailureQuota).ToList();

            for (int i = 0; i < highFailureQuota.Count && selected.Count / (double)amount < 0.5; i++)
            {
                selected.Add(highFailureQuota[random.Next(0, highFailureQuota.Count)]);
            }

            for (int i = 0; i < neverAttempted.Count && selected.Count / (double)amount < 0.9; i++)
            {
                selected.Add(neverAttempted[random.Next(0, neverAttempted.Count)]);
            }

            for (int i = 0; i < seldomAttempted.Count && selected.Count / (double)amount < 0.9; i++)
            {
                selected.Add(seldomAttempted[random.Next(0, seldomAttempted.Count)]);
            }

            while (selected.Count < amount)
            {
                var candidate = published[random.Next(0, published.Count)];

                selected.Add(candidate);
            }

            return selected.OrderBy(article => random.Next());
        }
    }
}