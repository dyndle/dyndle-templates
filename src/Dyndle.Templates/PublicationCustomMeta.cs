using DD4T.Templates.Base;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates.Base.Builder;
using Tridion.ContentManager.ContentManagement.Fields;
using System.Xml;
using System.Web.UI.WebControls;

namespace Dyndle.Templates
{
    public class PublicationCustomMeta : BasePageTemplate
    {
        public static readonly string DEFAULT_PUBLICATIONMETA_COMPONENT_TITLE = "PublicationMeta";

        public static readonly string DEFAULT_PUBLICATIONMETA_COMPONENT_VIEWNAME = "publicationMeta";

        public static readonly string DEFAULT_PUBLICATIONMETA_CT_TITLE = "PublicationMeta";

        public static readonly string DEFAULT_PUBLICATIONMETA_ROOT_ELEMENT_NAME = "publicationMeta";

        public static readonly string DEFAULT_PUBLICATIONMETA_SCHEMA_TITLE = "Sitemap";

        public PublicationCustomMeta() : base(TemplatingLogger.GetLogger(typeof(PublicationCustomMeta)))
        {
        }

        private string PublicationMetaComponentTemplateTitle
        {
            get
            {
                Item item = Package.GetByName("PublicationMetaComponentTemplateTitle");
                if (item == null)
                    return DEFAULT_PUBLICATIONMETA_CT_TITLE;
                return item.GetAsString();
            }
        }

        private string PublicationMetaComponentTitle
        {
            get
            {
                Item item = Package.GetByName("PublicationMetaComponentTitle");
                if (item == null)
                    return DEFAULT_PUBLICATIONMETA_COMPONENT_TITLE;
                return item.GetAsString();
            }
        }

        private string PublicationMetaComponentViewName
        {
            get
            {
                Item item = Package.GetByName("PublicationMetaComponentViewName");
                if (item == null)
                    return DEFAULT_PUBLICATIONMETA_COMPONENT_VIEWNAME;
                return item.GetAsString();
            }
        }

        private string PublicationMetaRootElementName
        {
            get
            {
                Item item = Package.GetByName("PublicationMetaRootElementName");
                if (item == null)
                    return DEFAULT_PUBLICATIONMETA_ROOT_ELEMENT_NAME;
                return item.GetAsString();
            }
        }

        private string PublicationMetaSchemaTitle
        {
            get
            {
                Item item = Package.GetByName("PublicationMetaSchemaTitle");
                if (item == null)
                    return DEFAULT_PUBLICATIONMETA_SCHEMA_TITLE;
                return item.GetAsString();
            }
        }

        protected override void TransformPage(Dynamic.Page page)
        {
            Publication publication = (Publication)GetTcmPage().ContextRepository;
            var filter = new PublicationsFilter(publication.Session); 
            var publications = publication.Session.GetList(filter);

            Log.Info(publications.OuterXml);

            Dynamic.Component c = (Dynamic.Component)DD4TUtil.CreateComponent(PublicationMetaComponentTitle, PublicationMetaRootElementName, "Sitemap", page.Publication.Id, page.OwningPublication.Id);
            Dynamic.ComponentTemplate ct = (Dynamic.ComponentTemplate)DD4TUtil.CreateComponentTemplate(PublicationMetaComponentTemplateTitle, "Sitemap", PublicationMetaComponentViewName, page.Publication.Id, page.OwningPublication.Id);
            Dynamic.ComponentPresentation cp = (Dynamic.ComponentPresentation)DD4TUtil.CreateComponentPresentation(c, ct);

            if (page.ComponentPresentations == null || page.ComponentPresentations.Count == 0)
            {
                page.ComponentPresentations = new List<Dynamic.ComponentPresentation>();
            }
            page.ComponentPresentations.Add(cp);


            foreach (XmlNode pub in publications.ChildNodes)
            {
                var meta = Engine.GetObject<Publication>(pub.Attributes["ID"].Value).Metadata;

                if (!string.IsNullOrEmpty(meta?.InnerXml))
                {
                    foreach (XmlNode field in meta.ChildNodes)
                    {
                        Log.Debug(field.Name + " - " + field.InnerText);
                        cp.Component.Fields.Add($"({pub.Attributes["ID"].Value}) - {field.Name}", new Dynamic.Field()
                        {
                            Name = field.Name,
                            FieldType = Dynamic.FieldType.Text,
                            Values = new List<string>() { field.Value }
                        });
                    }
                }
            }
        }
    }
}
