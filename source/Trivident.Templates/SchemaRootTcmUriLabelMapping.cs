using System.Collections.Generic;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Trivident.Templates
{
    [TcmTemplateTitle("Label mappings - schema root element name to schema URI")]
    [TcmTemplateParameterSchema("resource:Trivident.Templates.Resources.Label Parameters.xsd")]
    public class SchemaRootTcmUriLabelMapping : LabelMappingAbstract
    {
        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(SchemaRootTcmUriLabelMapping));

        protected override string DefaultPrefix => "SchemaRootElementName";

        public override IDictionary<string, string> GetMappings(Repository repository)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();

            Folder rootFolder = repository.RootFolder;
            OrganizationalItemItemsFilter filter = new OrganizationalItemItemsFilter(repository.Session)
            {
                Recursive = true,
                ItemTypes = new List<ItemType>() { ItemType.Schema }
            };

            foreach (Schema schema in rootFolder.GetItems(filter))
            {
                LOG.Debug("Schema " + schema.Title + " - " + schema.WebDavUrl);
                string rootName = schema.RootElementName;
                if (!string.IsNullOrEmpty(rootName))
                {
                    LOG.Debug(string.Format("    Mapping root name '{0}' --> {1}", rootName, schema.Id));
                    result[rootName] = schema.Id;
                }
            }

            return result;
        }
    }
}