using System.Collections.Generic;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Dyndle.Templates
{
    [TcmTemplateTitle("Label mappings - publication metadata")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.Label Parameters.xsd")]
    public class PublicationMetadataLabelMapping : LabelMappingAbstract
    {
        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(PublicationMetadataLabelMapping));
        protected override string DefaultPrefix => "PublicationMeta";

        public override IDictionary<string, string> GetMappings(Repository repository)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();
            if (repository.Metadata != null && repository.MetadataSchema != null)
            {
                var metaFields = ((Publication)repository).MetaFields();
                foreach (var field in metaFields)
                {
                    if (field is TextField)
                    {
                        var value = metaFields.Text(field.Name);
                        LOG.Debug("Publication metadata " + field.Name + " - " + value);
                        result[field.Name] = value;
                    }
                    else
                    {
                        Log.Debug($"found metadata field {field.Name} of unsupported type {field.GetType().Name}; skipping it");
                    }
                }
            }
            return result;
        }
    }
}