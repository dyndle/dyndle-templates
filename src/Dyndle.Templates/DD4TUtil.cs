using DD4T.ContentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyndle.Templates
{
    public static class DD4TUtil
    {
        public static IComponentTemplate CreateComponentTemplate(string templateTitle, string schemaTitle, string viewName = "", string publicationId = "tcm:0-0-0", string owningPublicationId = "tcm:0-0-0", string templateId = "tcm:0-0-0", string schemaId = "tcm:0-0-0")
        {
            string useViewName = viewName == "" ? schemaTitle.Replace(" ", "-").ToLower() : viewName;
            ComponentTemplate ct = new ComponentTemplate()
            {
                Title = templateTitle,
                Id = templateId,
                MetadataFields = new FieldSet()
            };
            ct.MetadataFields.Add("view", new Field()
            {
                Name = "view",
                Values = new List<string>() { useViewName },
                FieldType = FieldType.Text
            });
            return ct;
        }
        public static IComponent CreateComponent(string title, string rootElementName, string schemaTitle, string publicationId = "tcm:0-0-0", string owningPublicationId = "tcm:0-0-0", string templateId = "tcm:0-0-0", string schemaId = "tcm:0-0-0")
        {
            Component c = new Component()
            {
                Title = title,
                Fields = new FieldSet(),
                Id = "tcm:0-0-0",
                Schema = new Schema() { RootElementName = rootElementName, Title = schemaTitle, Id = schemaId },
                ComponentType = ComponentType.Normal,
                Publication = new Publication()
                {
                    Id = publicationId
                },
                OwningPublication = new Publication()
                {
                    Id = owningPublicationId
                }
            };


            return c;
        }

        public static IComponentPresentation CreateComponentPresentation(IComponent component, IComponentTemplate componentTemplate)
        {
            return new DD4T.ContentModel.ComponentPresentation()
            {
                Component = component as Component,
                ComponentTemplate = componentTemplate as ComponentTemplate
            };
        }


        public static ComponentPresentation AddTextField(this ComponentPresentation cp, string name, string value)
        {
            Field field = new Field()
            {
                Name = name,
                Values = new List<string>() { value },
                XPath = "/",
                FieldType = FieldType.Text
            };
            cp.Component.Fields.Add(name, field);
            return cp;
        }

        public static ComponentPresentation AddEmbeddedField(this ComponentPresentation cp, string fieldName, string embeddedRootElementName)
        {
           
            Schema embeddedSchema = new Schema()
            {
                RootElementName = embeddedRootElementName,
                Title = embeddedRootElementName
            };
            Field field = new Field()
            {
                Name = fieldName,
                EmbeddedValues = new List<FieldSet>(),
                XPath = "/",
                FieldType = FieldType.Embedded,
                EmbeddedSchema = embeddedSchema
            };
            cp.Component.Fields.Add(fieldName, field);
            return cp;
        }
    }
}
