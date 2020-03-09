using System.Collections.Generic;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Trivident.Templates
{
    [TcmTemplateTitle("Label mappings - category XML name to category id")]
    [TcmTemplateParameterSchema("resource:Trivident.Templates.Resources.Label Parameters.xsd")]
    public class CategoryXmlNameToIdLabelMapping : LabelMappingAbstract
    {
        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(CategoryXmlNameToIdLabelMapping));

        protected override string DefaultPrefix => "CategoryId";



        public override IDictionary<string, string> GetMappings(Repository repository)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();
            foreach (var category in repository.GetCategories())
            {
                LOG.Debug("Category " + category.XmlName + " - " + category.Id);
                result[category.XmlName] = category.Id;
            }
            return result;
        }
    }
}