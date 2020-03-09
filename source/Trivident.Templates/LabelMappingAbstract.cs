using DD4T.Templates.Base;
using DD4T.Templates.Base.Utils;
using System;
using System.Collections.Generic;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Dynamic = DD4T.ContentModel;

namespace Trivident.Templates
{
    public abstract class LabelMappingAbstract : BasePageTemplate
    {
        protected LabelMappingAbstract() : base(TemplatingLogger.GetLogger(typeof(LabelMappingAbstract)))
        {
        }

        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(LabelMappingAbstract));

        public abstract IDictionary<string, string> GetMappings(Repository repository);

        protected abstract string DefaultPrefix { get; }

        protected override void TransformPage(Dynamic.Page dd4tPage)
        {
            
            Item pageItem = Package.GetByName(Package.PageName);
            if (pageItem == null)
            {
                throw new TemplatingException("Place this TBB on a Page");
            }

            Page tomPage = (Page)Engine.GetObject(pageItem);
            
            IDictionary<string, string> viewMappings = GetMappings(tomPage.ContextRepository);

            Dynamic.Component dd4tComponent = new Dynamic.Component();
            dd4tComponent.Title = Prefix;
            dd4tComponent.Id = GetRandomTcmUri(tomPage.Id.PublicationId);
            dd4tComponent.Fields = new Dynamic.FieldSet();
            dd4tComponent.Schema = new DD4T.ContentModel.Schema() { RootElementName = LabelGroupRootElementName, Title = LabelGroupSchemaTitle, Id = "tcm:0-0-0" };
            dd4tComponent.ComponentType = Dynamic.ComponentType.Normal;

            if (! (string.IsNullOrEmpty(LabelGroupGroupFieldName) || string.IsNullOrEmpty(LabelGroupGroupFieldValue)))
            {
                Dynamic.Field groupField = new Dynamic.Field()
                {
                    Name = LabelGroupGroupFieldName,
                    Values = new List<string>() { LabelGroupGroupFieldValue },
                    XPath = "/",
                    FieldType = Dynamic.FieldType.Text
                };
                dd4tComponent.Fields.Add(LabelGroupGroupFieldName, groupField);
            }
            dd4tPage.ComponentPresentations.Add(
                new DD4T.ContentModel.ComponentPresentation()
                {
                    Component = dd4tComponent,
                    ComponentTemplate = new Dynamic.ComponentTemplate() {  Title = "Generated" }
                }
                );

            AddLabelMappings(Prefix, dd4tComponent, viewMappings);
        }

        private static string GetRandomTcmUri(int publicationId)
        {
            Random rnd = new Random();
            var itemId = rnd.Next();
            return $"tcm:{publicationId}-{itemId}";
        }

        private void AddLabelMappings(string prefix, Dynamic.Component dd4tComponent, IDictionary<string, string> mappings)
        {
            Dynamic.Field embeddedField = new Dynamic.Field()
            {
                Name = LabelFieldName,
                EmbeddedValues = new List<Dynamic.FieldSet>(),
                EmbeddedSchema = new Dynamic.Schema()
                {
                    RootElementName = LabelRootElementName,
                    Title = LabelRootElementName,
                    Id = "tcm:0-0-0"
                },
                XPath = "/",
                FieldType = Dynamic.FieldType.Embedded
            };

            dd4tComponent.Fields.Add(embeddedField.Name, embeddedField);

            foreach (KeyValuePair<string, string> entry in mappings)
            {
                Dynamic.Field keyField = new Dynamic.Field()
                {
                    Name = KeyFieldName,
                    Values = new List<string> { $"{prefix}.{entry.Key}" },
                    FieldType = Dynamic.FieldType.Text
                };

                Dynamic.Field valueField = new Dynamic.Field()
                {
                    Name = ValueFieldName,
                    Values = new List<string> { entry.Value },
                    FieldType = Dynamic.FieldType.Text
                };

                Dynamic.FieldSet newFieldSet = new Dynamic.FieldSet();
                newFieldSet.Add(KeyFieldName, keyField);
                newFieldSet.Add(ValueFieldName, valueField);
                embeddedField.EmbeddedValues.Add(newFieldSet);
            }
        }

        protected string SchemaWebDavUrl
        {
            get
            {
                if (Package == null)
                {
                    return null;
                }
                if (Package.GetByName("SchemaWebDavUrl") == null)
                {
                    return null;
                }
                return Package.GetByName("SchemaWebDavUrl").GetAsString();
            }
        }


        protected string Prefix
        {
            get
            {
                if (Package == null)
                {
                    return DefaultPrefix;
                }
                if (Package.GetByName("Prefix") == null)
                {
                    return DefaultPrefix;
                }
                return Package.GetByName("Prefix").GetAsString();
            }
        }

        private string LabelFieldName
        {
            get
            {
                if (Package == null)
                {
                    return "LabelFieldNamePackageNull";
                }
                if (Package.GetByName("LabelFieldName") == null)
                {
                    return "LabelFieldNameNotFound";
                }
                return Package.GetByName("LabelFieldName").GetAsString();
            }
        }

        private string KeyFieldName
        {
            get
            {
                if (Package == null)
                {
                    return "KeyFieldNamePackageNull";
                }
                if (Package.GetByName("KeyFieldName") == null)
                {
                    return "KeyFieldNameNotFound";
                }
                return Package.GetByName("KeyFieldName").GetAsString();
            }
        }

        private string ValueFieldName
        {
            get
            {
                if (Package == null)
                {
                    return "ValueFieldNamePackageNull";
                }
                if (Package.GetByName("ValueFieldName") == null)
                {
                    return "ValueFieldNameNotFound";
                }
                return Package.GetByName("ValueFieldName").GetAsString();
            }
        }

        private string LabelRootElementName
        {
            get
            {
                if (Package == null)
                {
                    return "LabelRootElementNamePackageNull";
                }
                if (Package.GetByName("LabelRootElementName") == null)
                {
                    return "LabelRootElementNameNotFound";
                }
                return Package.GetByName("LabelRootElementName").GetAsString();
            }
        }


        private string LabelGroupSchemaTitle
        {
            get
            {
                if (Package == null)
                {
                    return "LabelGroupSchemaTitlePackageNull";
                }
                if (Package.GetByName("LabelGroupSchemaTitle") == null)
                {
                    return "LabelGroupSchemaTitleNotFound";
                }
                return Package.GetByName("LabelGroupSchemaTitle").GetAsString();
            }
        }

        private string LabelGroupRootElementName
        {
            get
            {
                if (Package == null)
                {
                    return "LabelGroupRootElementNamePackageNull";
                }
                if (Package.GetByName("LabelGroupRootElementName") == null)
                {
                    return "LabelGroupRootElementNameNotFound";
                }
                return Package.GetByName("LabelGroupRootElementName").GetAsString();
            }
        }

        private string LabelGroupGroupFieldName
        {
            get
            {
                if (Package == null)
                {
                    return string.Empty;
                }
                if (Package.GetByName("LabelGroupGroupFieldName") == null)
                {
                    return string.Empty;
                }
                return Package.GetByName("LabelGroupGroupFieldName").GetAsString();
            }
        }

        private string LabelGroupGroupFieldValue
        {
            get
            {
                if (Package == null)
                {
                    return string.Empty;
                }
                if (Package.GetByName("LabelGroupGroupFieldValue") == null)
                {
                    return string.Empty;
                }
                return Package.GetByName("LabelGroupGroupFieldValue").GetAsString();
            }
        }
    }
}