using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Website
{
    public static class StringExtensions
    {
        public static string HtmlDecode(this string str)
        {
            return HttpUtility.HtmlDecode(str);
        }

        public static string HtmlEncode(this string str)
        {
            return HttpUtility.HtmlEncode(str);
        }

        public static string RemoveHtml(this string str)
        {
            return ReplaceHtml(str, String.Empty);
        }

        public static string ReplaceHtml(this string str, string replacement)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            return Regex.Replace(str, "</?[a-z][a-z0-9]*[^<>]*>", replacement, RegexOptions.IgnoreCase);
        }        

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