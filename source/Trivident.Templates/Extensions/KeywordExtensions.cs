using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.ContentManagement;

namespace TemplatingExtensions.ExtensionMethods.Tridion
{
    public static class KeywordExtensions
    {
        public static Keyword GetRootParentKeyword(this Keyword keyword)
        {
            return !keyword.IsRoot ? GetRootParentKeyword(keyword.ParentKeywords[0]) : keyword;
        }

        public static IList<Keyword> GetKeywordTrail(Keyword keyword)
        {
            List<Keyword> keywords = new List<Keyword>();
            Keyword kw = keyword;

            while(!kw.IsRoot)
            {
                keywords.Add(kw);
                kw = kw.ParentKeywords[0];
            }

            return keywords;
        }
    }
}
