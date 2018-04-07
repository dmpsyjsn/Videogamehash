//using System.IO;
//using Recaptcha;
//using System.Web.UI;
//using System.Web.Mvc;
//using System.Configuration;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Web;


namespace VideoGameHash.Helpers
{
    public static class HtmlHelpers
    {
        //public static MvcHtmlString GenerateCaptcha(this HtmlHelper htmlHelper)
        //{
        //    var html = RecaptchaControlMvc.GenerateCaptcha(htmlHelper);
        //    return MvcHtmlString.Create(html);
        //}

        public static string ParseContent(string section, string source, string content)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(content ?? "");

            if (section == "Reviews")
            {
                var parsedContent = doc.DocumentNode.InnerText;
                if (source == "GameSpot")
                {
                    var index = parsedContent.IndexOf("Read and Post Comments");

                    if (index > 0)
                        parsedContent = parsedContent.Substring(0, index).Trim();

                    if (parsedContent.IndexOf("Score:") > 0)
                    {
                        var find = new char[] { '.', '!', '?' };
                        index = parsedContent.IndexOfAny(find);
                        if (index > 0)
                        {
                            parsedContent = parsedContent.Substring(0, index + 1).Trim();
                        }
                    }
                }

                return HttpUtility.HtmlDecode(parsedContent);
            }
            else if (section == "Media")
            {
                var parsedContent = doc.DocumentNode.InnerText;
                if (source == "GameSpot")
                {
                    var index = parsedContent.IndexOf("Read and Post Comments");

                    if (index > 0)
                        parsedContent = parsedContent.Substring(0, index).Trim();
                }

                return HttpUtility.HtmlDecode(parsedContent);
            }
            else
            {
                var newContent = new StringBuilder(doc.DocumentNode.InnerText);

                if (source == "CVG")
                {
                    var index = newContent.ToString().IndexOf("Click here");

                    if (index > 0)
                        newContent = newContent.Remove(index, newContent.Length - index);
                }

                return HttpUtility.HtmlDecode(newContent.ToString());
            }
        }
    }

    public class PaginatedList<T> : List<T>
    {

        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            this.AddRange(source.Skip(PageIndex * PageSize).Take(PageSize));
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 0);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex + 1 < TotalPages);
            }
        }
    }
}
