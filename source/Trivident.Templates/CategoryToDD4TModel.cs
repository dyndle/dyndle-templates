using DD4T.Templates.Base;
using DD4T.Templates.Base.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;

namespace Trivident.Templates
{
    [TcmTemplateTitle("Category to DD4T Model")]
    [TcmTemplateParameterSchema("resource:Trivident.Templates.Resources.Category Parameters.xsd")]
    public class CategoryToDD4TModel : BasePageTemplate
    {
        private readonly TemplatingLogger log = TemplatingLogger.GetLogger(typeof(CategoryToDD4TModel));

        public CategoryToDD4TModel() : base(TemplatingLogger.GetLogger(typeof(CategoryToDD4TModel)))
        {
        }

        protected override void TransformPage(Dynamic.Page dd4tPage)
        {
            log.Debug($"started TransformPage '{dd4tPage.Title}'");
            var category = Engine.GetObject<Category>(this.WebDavUrl);

            if (category == null)
            {
                throw new TemplatingException(string.Format("category not found. {0}", this.WebDavUrl));
            }

            if (category.ContextRepository != this.Engine.GetContextPublication())
            {
                Log.Debug("Category loaded in the wrong publicaiton, let's load it in the current context publication.");
                category = Engine.GetObject<Category>(this.Engine.LocalizeUri(category.Id));
            }
            Log.Debug(string.Format("keyword Id: {0}", category.Id));

            var dd4tComponent = CreateDynamicComponent(category);

            dd4tComponent.Schema = new DD4T.ContentModel.Schema() { RootElementName = this.RootElementName };
            dd4tPage.ComponentPresentations.Add(
                new DD4T.ContentModel.ComponentPresentation()
                {
                    Component = dd4tComponent,
                    ComponentTemplate = new Dynamic.ComponentTemplate()
                }
            );
        }

        private Dynamic.Component CreateDynamicComponent(Category category)
        {
            Dynamic.Component dd4tComponent = new Dynamic.Component()
            {
                Title = "VirtualComponent",
                Fields = new Dynamic.FieldSet()
            };

            Dynamic.Field keywordField = new Dynamic.Field()
            {
                Name = "values",
                KeywordValues = new List<Dynamic.Keyword>()
            };

            foreach (var item in category.GetKeywords())
            {
                var dynamicKeyword = CreateDynamicKeywordModel(item);
                keywordField.KeywordValues.Add(dynamicKeyword);
            }
            dd4tComponent.Fields.Add("values", keywordField);

            return dd4tComponent;
        }

        private Dynamic.Keyword CreateDynamicKeywordModel(Keyword keyword)
        {
            return KeywordBuilder.BuildKeyword(keyword, 2, Manager);
        }

        protected string WebDavUrl
        {
            get
            {
                if (Package == null)
                {
                    return null;
                }
                if (Package.GetByName("WebDavUrl") == null)
                {
                    return null;
                }
                return Package.GetByName("WebDavUrl").GetAsString();
            }
        }

        private string RootElementName
        {
            get
            {
                if (Package == null)
                {
                    return "RootElementNamePackageNull";
                }
                if (Package.GetByName("RootElementName") == null)
                {
                    return "RootElementNameNotFound";
                }
                return Package.GetByName("RootElementName").GetAsString();
            }
        }
    }
}