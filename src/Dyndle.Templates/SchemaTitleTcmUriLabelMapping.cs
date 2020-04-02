using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Dyndle.Templates
{
    [TcmTemplateTitle("Label mappings - schema title to schema URI")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.Label Parameters.xsd")]
    public class SchemaTitleTcmUriLabelMapping : LabelMappingAbstract
    {
        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(SchemaTitleTcmUriLabelMapping));

        protected override string DefaultPrefix => "SchemaTitle";

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
                string schemaTitle = schema.Title;
                if (!string.IsNullOrEmpty(schemaTitle))
                {
                    LOG.Debug(string.Format("    Mapping root name '{0}' --> {1}", schemaTitle, schema.Id));
                    result[schemaTitle] = schema.Id;
                }
            }

            return result;
        }
    }
}