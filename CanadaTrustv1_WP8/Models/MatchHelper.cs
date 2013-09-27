using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CanadaTrustv1.Models
{
    public class MatchHelper
    {
        public static string Match(string xPathString, HtmlDocument doc, Regex regex)
        {
            string rawInnerText = doc.DocumentNode.SelectSingleNode(xPathString).InnerText;
            Match matchRegex = regex.Match(rawInnerText);
            return matchRegex.Groups[1].Value;
        }
        public static string MatchAndReturnHtml(string xPathString, HtmlDocument doc, Regex regex)
        {
            string rawInnerHtml = doc.DocumentNode.SelectSingleNode(xPathString).InnerHtml;
            Match matchRegex = regex.Match(rawInnerHtml);
            return matchRegex.Groups[1].Value;
        }
    }
}
