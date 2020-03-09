using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Trivident.Templates
{
    /// <summary>
    /// Creates a field for SI4T which contains the title of the top level structure group (the sg inside the root)
    /// The name of this field is configurable with a parameter
    /// This TBB should be placed between the Generate Index Data and the Add Index Data to Output TBBs
    /// </summary>
    [TcmTemplateTitle("Add top level directory to SI4T")]
    [TcmTemplateParameterSchema("resource:Trivident.Templates.Resources.AddTopLevelDirectory Parameters.xsd")]
    public class AddTopLevelDirectoryToSI4TData : ITemplate
    {
        Engine Engine { get; set; }
        Package Package { get; set; }
        private TemplatingLogger logger;
        private static readonly string PACKAGE_VARIABLE_NAME = "SI4T.Templating.SearchData";

        public AddTopLevelDirectoryToSI4TData()
        {
            logger = TemplatingLogger.GetLogger(this.GetType());
        }

        public void Transform(Engine engine, Package package)
        {
            Engine = engine;
            Package = package;

            IEnumerable<Page> pages;
            if (IsPageTemplate)
            {
                logger.Debug("This is a page template");
                pages = new List<Page>() { GetPage() };
            }
            else
            {
                logger.Debug("This is a component template");
                pages = GetPagesFromComponent(GetComponent());
            }
            logger.Debug("Found pages: " + string.Join("/", pages.Select(p => p.Title)));

           
            var topLevelValues = new List<string>();
            foreach (var page in pages)
            {
                logger.Debug($"page path: {page.Path}");
                var segments = page.Path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                logger.Debug($"nr of segments: {segments.Length}");
                if (segments.Length <= 2)
                {
                    // page is in root structure group
                    logger.Debug("page is in root structure group, skipping");
                    continue;
                }
                var dir = segments[2];
                logger.Debug($"dir: {dir}");
                Regex reStripPrefix = new Regex("^\\d+\\s(?<nameWithoutPrefix>.*)$");
                Match m = reStripPrefix.Match(dir);
                if (m.Success)
                {
                    logger.Debug("this is a primary SG");
                    var nameWithoutPrefix = m.Groups["nameWithoutPrefix"].Value;
                    if (!topLevelValues.Contains(nameWithoutPrefix))
                    {
                        topLevelValues.Add(nameWithoutPrefix);
                    }
                    
                }
            }
           
            logger.Debug("found next tags: " + string.Join(", ", topLevelValues));

            if (!topLevelValues.Any())
            {
                logger.Debug("no new tags found, stopping now");
                return;
            }

            var searchData = Package.GetStringItemValue(PACKAGE_VARIABLE_NAME);

            Regex re = new Regex("^(?<firstPart>.*)</custom>(?<lastPart>.*)$");
            Match match = re.Match(searchData);
            if (!match.Success)
            {
                logger.Debug("cannot find SI4T data in output, stopping now");
                return;
            }

            var firstPart = match.Groups["firstPart"].Value;
            var lastPart = match.Groups["lastPart"].Value;
            var newValue = string.Join("", topLevelValues.Select(a => "<" + FieldName  + ">" + a + "</" + FieldName  + ">"));
            searchData = firstPart + newValue + "</custom>" + lastPart;
            logger.Debug("new search data: " + searchData);
            Package.RemoveItem(PACKAGE_VARIABLE_NAME);
            Package.PushItem(PACKAGE_VARIABLE_NAME, Package.CreateStringItem(ContentType.Xml, searchData));

        }

        private IEnumerable<Page> GetPagesFromComponent(Component component)
        {
            UsingItemsFilter filter = new UsingItemsFilter(component.Session);
            filter.ItemTypes = new List<ItemType> { ItemType.Page };
            filter.IncludedVersions = VersionCondition.OnlyLatestVersions;

            return component.GetUsingItems(filter).Cast<Page>();
        }

        protected bool IsPageTemplate
        {
            get
            {
                return Engine.PublishingContext.ResolvedItem.Item is Page;
            }
        }

        protected Page GetPage()
        {
            
            //first try to get from the render context
            RenderContext renderContext = Engine.PublishingContext.RenderContext;
            if (renderContext != null)
            {
                Page contextPage = renderContext.ContextItem as Page;
                if (contextPage != null)
                    return contextPage;
            }
            Item pageItem = Package.GetByType(ContentType.Page);
            if (pageItem != null)
                return (Page)Engine.GetObject(pageItem.GetAsSource().GetValue("ID"));

            return null;
        }

        protected Component GetComponent()
        {
            Item component = Package.GetByName("Component");
            return (Component)Engine.GetObject(component.GetAsSource().GetValue("ID"));
        }

        private string FieldName
        {
            get
            {
                if (Package == null)
                {
                    return "FieldNamePackageNull";
                }
                if (Package.GetByName("FieldName") == null)
                {
                    return "FieldNameNotFound";
                }
                return Package.GetByName("FieldName").GetAsString();
            }
        }
    }
}
