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
            t = UrlReplacer.Replace(t, "/");
            return t.Replace("chittychittybangbang", "://");
        }

        private static Regex _urlReplacer;
        private static Regex UrlReplacer
        {
            get
            {
                if (_urlReplacer == null)
                    _urlReplacer = new Regex("/+");
                return _urlReplacer;
            }
        }
    }
}
