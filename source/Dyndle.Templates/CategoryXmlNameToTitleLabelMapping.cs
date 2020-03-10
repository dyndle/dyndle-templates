using System.Collections.Generic;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Dyndle.Templates
{
    [TcmTemplateTitle("Label mappings - category XML name to category title")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.Label Parameters.xsd")]
    public class CategoryXmlNameToTitleLabelMapping : LabelMappingAbstract
    {
        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(CategoryXmlNameToTitleLabelMapping));

        protected override string DefaultPrefix => "CategoryTitle";


        public override IDictionary<string, string> GetMappings(Repository repository)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();
            foreach (var category in repository.GetCategories())
            {
                LOG.Debug("Category " + category.XmlName + " - " + category.Id);
                result[category.XmlName] = category.Title;
            }
            return result;
        }
    }
}