using System;

namespace UpdateStudyQuiz
{
    public static class StringExtensions
    {        
        public static string Preview(this string str, int maxLength)
        {
            if (String.IsNullOrEmpty(str))
            {
                return String.Empty;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            string
                previewText,
                previewSuffix = "...";

            int lastIndexOfSpace = str.LastIndexOf(" ", Math.Min(str.Length, maxLength) - 1);

            if (lastIndexOfSpace != -1)
            {
                previewText = str.Substring(0, lastIndexOfSpace);
            }
            else
            {
                previewText = str.Substring(0, Math.Min(str.Length, maxLength));
            }

            return String.Concat(previewText, previewSuffix);
        }
    }
}