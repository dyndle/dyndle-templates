using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Dyndle.Templates
{
    [TcmTemplateTitle("Label mappings - Label schema to Key-value pairs")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.Label Schema Definition Parameters.xsd")]
    public class LabelMapping : LabelMappingAbstract
    {
        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(LabelMapping));

        public override IDictionary<string, string> GetMappings(Repository repository)
        {
            var labels = new Dictionary<string, string>();

            LOG.Debug("Starting GetMappings for repository " + repository.Id + " (" + repository.Title + ")");
            LOG.Debug("configured webdavUrl " + this.SchemaWebDavUrl);
            var schema = repository.Session.GetObject(this.SchemaWebDavUrl) as Schema;
            if (schema.ContextRepository.Id != repository.Id)
            {
                schema = Engine.GetObject<Schema>(new TcmUri(schema.Id.ItemId, schema.Id.ItemType, repository.Id.ItemId));
            }
            LOG.Debug("filtering by schema " + schema.Id);

           
            Folder folderToSearch;
            if (!string.IsNullOrEmpty(FolderWebDavUrl))
            {
                folderToSearch = Engine.GetObject<Folder>(FolderWebDavUrl);
                if (folderToSearch.ContextRepository.Id != repository.Id)
                {
                    folderToSearch = Engine.GetObject<Folder>(new TcmUri(folderToSearch.Id.ItemId, folderToSearch.Id.ItemType, repository.Id.ItemId));
                }
            }
            else
            {
                folderToSearch = ((Tridion.ContentManager.CommunicationManagement.Publication)repository).RootFolder;
            }

            GetLabelsFromFolder(repository, folderToSearch, schema, labels);
          
            return labels;
        }

        private void GetLabelsFromFolder(Repository repository, Folder folderToSearch, Schema schema, Dictionary<string, string> labels)
        {
            LOG.Debug("searching for label components in folder " + folderToSearch.WebDavUrl);

            var itemsFilter = new OrganizationalItemItemsFilter(repository.Session)
            {
                //                Recursive = true,
                ItemTypes = new[] { ItemType.Component },
                BasedOnSchemas = new[] { schema }
            };

            var usingItemsResult = folderToSearch.GetListItems(itemsFilter);

            //Log.Debug("Schema from cm " + schema.Id);
            //var schemaUri = new TcmUri(schema.Id);

            //Log.Debug($"using context publication {this.GetTcmPage().ContextRepository.Title}");
            //var usingItemsFilter = new UsingItemsFilter(repository.Session)
            //{
            //    ItemTypes = new[] { ItemType.Component },
            //    IncludedVersions = VersionCondition.OnlyLatestVersions,
            //};
            //var usingItemsResult = schema.GetListUsingItems(usingItemsFilter);

            var labelComponents = new List<Component>();
            foreach (XmlElement childElmt in usingItemsResult.SelectNodes("./*"))
            {
                TcmUri usingUri = new TcmUri(childElmt.GetAttribute("ID"));
                LOG.Debug("found using item with uri " + usingUri.ToString());
                TcmUri localUsingUri = new TcmUri(usingUri.ItemId, usingUri.ItemType, repository.Id.ItemId);
                LOG.Debug("localversion of uri " + localUsingUri.ToString());
                var labelComponent = Engine.GetObject<Component>(localUsingUri);
                if (labelComponent == null)
                {
                    LOG.Info($"found component using the schema which cannot be instantiated from the current context repository ({repository.Title})");
                    continue;
                }
                labelComponents.Add(labelComponent);
            }
            LOG.Debug($"Found {labelComponents.Count()} components based on schema");
            labelComponents.ForEach(component => GetLabelsFromComponent(component, labels));


            // if there are subfolders, let's process them as well
            var subfolderFilter = new OrganizationalItemItemsFilter(repository.Session)
            {
                ItemTypes = new[] { ItemType.Folder }
            };

            var subFolders = folderToSearch.GetItems(subfolderFilter);
            foreach (Folder subFolder in subFolders)
            {
                GetLabelsFromFolder(repository, subFolder, schema, labels);
            }
        }
        private void GetLabelsFromComponent(Component component, Dictionary<string, string> labels, int level = 0)
        {
            Log.Debug($"Found component {component.Id} ({component.Title})");
            var fields = component.Fields();
            GetLabelsFromFields(component, fields, labels, level);
        }

        private void GetLabelsFromFields(Component component, ItemFields fields, Dictionary<string, string> labels, int level)
        {
            if (level > 1)
            {
                Log.Debug("one recursion too far - stepping back");
                return; // note: our business rules only support 2 levels (0 and 1)
            }
            Log.Debug($"Looking for labels in set of {fields.Count()} fields ({string.Join(",", fields.Select(f => f.Name).ToList())})");

            // The logic is configurable in template parameters. The following parameters are supported:
            // ---- CurrentLabelValueFieldName (defaults to "value")
            // ---- CurrentLabelKeyFieldName (if empty, the key is assumed to be the title of the component)
            // ---- CurrentEmbeddedLabelsFieldName (if it has a value, the embedded fields are assumed to have a key/value pair)
            // This allows 5 different scenarios:
            // 1. Label components have text fields which are used as value (with the field name as the key)
            // 2. Label components have only a value text field, the component title is the key
            // 3. Label components have a key and a value text field
            // 4. Label components have a multiple value embedded field with key/value pairs (both text fields)
            // 5. Label components have a multiple value embedded field with a key text field and a value component link field, which links to a label component of scenario 1


            if (level == 0 && string.IsNullOrEmpty(CurrentEmbeddedLabelsFieldName) && string.IsNullOrEmpty(CurrentLabelKeyFieldName) && CurrentLabelValueFieldName == "CurrentLabelValueFieldNameNotFound")
            {
                // scenario 1
                foreach (var field in fields.Where(f => f is TextField || f is XhtmlField || f is NumberField))
                {
                    labels.Add(field.Name, fields.AsText(field.Name));
                }
                return;
            }
            if (level == 0 && !string.IsNullOrEmpty(CurrentEmbeddedLabelsFieldName))
            {
                var embeddedValues = fields.Embeddeds(CurrentEmbeddedLabelsFieldName);
                if (!embeddedValues.Any())
                {
                    Log.Debug($"no embedded labels found (using field name {CurrentEmbeddedLabelsFieldName}). Do you need to change the 'Name of the INPUT field that contains embedded labels' parameter in the template?");
                    return;
                }

                fields.Embeddeds(CurrentEmbeddedLabelsFieldName).ForEach(item => GetLabelsFromFields(component,item,labels, level + 1));
                return;
            }

            if (fields[CurrentLabelValueFieldName] is ComponentLinkField)
            {
                var linkedComponents = fields.Components(CurrentLabelValueFieldName);
                if (linkedComponents.Any())
                {
                    GetLabelsFromComponent(linkedComponents.FirstOrDefault(), labels, level);
                }
                return;
            }

            if (!(fields[CurrentLabelValueFieldName] is TextField))
            {
                LOG.Warning($"field {CurrentLabelValueFieldName} is not a text field or a component link field, ignoring it");
                return;
            }

            var labelValue = fields.Text(CurrentLabelValueFieldName);
            if (string.IsNullOrEmpty(labelValue))
            {
                Log.Debug($"no label value found (using field name {CurrentLabelValueFieldName}). Do you need to change the 'Name of the INPUT field that contains the label value' parameter in the template?");
                return;
            }

            var labelKey = component.Title;

            if (!string.IsNullOrEmpty(CurrentLabelKeyFieldName))
            {
                if (string.IsNullOrEmpty(fields.Text(CurrentLabelKeyFieldName)))
                {
                    Log.Debug($"no label key found (using field name {CurrentLabelKeyFieldName}), defaulting to title. If you mean to read the key from a field, use the 'Name of the INPUT field that contains the label key' parameter in the template");
                }
                else
                {
                    labelKey = fields.Text(CurrentLabelKeyFieldName);
                    Log.Debug("Found component with key and value field");
                }
            }
            else
            {
                Log.Debug("Found component with value field only, using component title as key");
            }
            AddLabelIfNotPresent(labelKey, labelValue, labels);
            return;

        }

        private void AddLabelIfNotPresent(string key, string value, Dictionary<string, string> labels)
        {
            if (!labels.ContainsKey(key))
            {
                Log.Debug(string.Format("found label {0}:{1}", key, value));
                labels.Add(key, value);
            }
        }

        private string CurrentLabelValueFieldName
        {
            get
            {
                if (Package == null)
                {
                    return "CurrentLabelValueFieldNamePackageNull";
                }
                if (Package.GetByName("CurrentLabelValueFieldName") == null)
                {
                    return "CurrentLabelValueFieldNameNotFound";
                }
                return Package.GetByName("CurrentLabelValueFieldName").GetAsString();
            }
        }

        private string CurrentLabelKeyFieldName
        {
            get
            {
                if (Package == null)
                {
                    return "CurrentLabelKeyFieldNamePackageNull";
                }
                if (Package.GetByName("CurrentLabelKeyFieldName") == null)
                {
                    return string.Empty;
                }
                return Package.GetByName("CurrentLabelKeyFieldName").GetAsString();
            }
        }

        private string CurrentEmbeddedLabelsFieldName
        {
            get
            {
                if (Package == null)
                {
                    return "CurrentEmbeddedLabelsFieldNamePackageNull";
                }
                if (Package.GetByName("CurrentEmbeddedLabelsFieldName") == null)
                {
                    return string.Empty;
                }
                return Package.GetByName("CurrentEmbeddedLabelsFieldName").GetAsString();
            }
        }

        private string FolderWebDavUrl
        {
            get
            {
                if (Package == null)
                {
                    return "FolderWebDavUrlPackageNull";
                }
                if (Package.GetByName("FolderWebDavUrl") == null)
                {
                    return string.Empty;
                }
                return Package.GetByName("FolderWebDavUrl").GetAsString();
            }
        }

        protected override string DefaultPrefix => "Label";
    }
}