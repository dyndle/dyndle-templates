using System.Collections.Generic;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Dyndle.Templates
{
    [TcmTemplateTitle("Label mappings - view name to template URI")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.Label Parameters.xsd")]
    public class ViewNameTcmUriLabelMapping : LabelMappingAbstract
    {
        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(ViewNameTcmUriLabelMapping));
        public const string FIELD_VIEW_NAME = "view";

        protected override string DefaultPrefix => "View";

        public override IDictionary<string, string> GetMappings(Repository repository)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();

            Folder rootFolder = repository.RootFolder;
            OrganizationalItemItemsFilter filter = new OrganizationalItemItemsFilter(repository.Session)
            {
                Recursive = true,
                ItemTypes = new List<ItemType>() { ItemType.ComponentTemplate, ItemType.PageTemplate }
            };

            foreach (Template template in rootFolder.GetItems(filter))
            {
                LOG.Debug("Template " + template.Title + " - " + template.WebDavUrl);
                string viewName = GetViewName(template);
                if (viewName != null)
                {
                    LOG.Debug(string.Format("    Mapping view '{0}' --> {1}", viewName, template.Id));
                    result[viewName] = template.Id;
                }
            }

            return result;
        }

        private string GetViewName(Template template)
        {
            string result = null;

            if (template.Metadata != null && template.MetadataSchema != null)
            {
                ItemFields metaFields = new ItemFields(template.Metadata, template.MetadataSchema);
                if (metaFields.Contains(FIELD_VIEW_NAME))
                {
                    TextField metaField = metaFields[FIELD_VIEW_NAME] as TextField;
                    if (metaField != null)
                    {
                        result = metaField.Value;
                    }
                }
            }

            return result;
        }
    }
}