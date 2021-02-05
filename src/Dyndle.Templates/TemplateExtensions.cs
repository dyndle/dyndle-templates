using System.Text.RegularExpressions;
using Tridion.ContentManager.CommunicationManagement;

namespace Dyndle.Templates
{
    public static class TemplateExtensions
    {
        public static string GetFileNameWithExtension(this Page page)
        {
            return page.FileName + "." + page.PageTemplate.FileExtension;
        }
        public static string NormalizeUrl(this string url)
        {
            var t = url.Replace("://", "chittychittybangbang");
            t = URL_REPLACER.Replace(t, "/");
            return t.Replace("chittychittybangbang", "://");
        }

        private static Regex URL_REPLACER = new Regex("/+");
       
    }
}
