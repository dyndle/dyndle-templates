using DD4T.Templates.Base;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates.Base.Builder;
using Tridion.ContentManager.ContentManagement.Fields;
using System.Xml;

namespace Dyndle.Templates
{
    [TcmTemplateTitle("Sitemap")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.Sitemap Parameters.xsd")]
    public class Sitemap : BasePageTemplate
    {
        public static readonly string DEFAULT_CHANGE_FREQUENCY = "weekly";

        public static readonly string DEFAULT_META_ROBOT = "index,follow";

        public static readonly string DEFAULT_PRIORITY = "0.5";

        public static readonly string DEFAULT_SITEMAP_COMPONENT_TITLE = "Sitemap";

        public static readonly string DEFAULT_SITEMAP_COMPONENT_VIEWNAME = "sitemap";

        public static readonly string DEFAULT_SITEMAP_CT_TITLE = "Sitemap";

        public static readonly string DEFAULT_SITEMAP_ROOT_ELEMENT_NAME = "sitemap";

        public static readonly string DEFAULT_SITEMAP_SCHEMA_TITLE = "Sitemap";

        public static readonly string DEFAULT_INDEX_FILENAME = "index";

        public static readonly bool FILTER_BY_NUMERIC_PREFIX = true;

        public static readonly bool FILTER_BY_NUMERIC_PREFIX_ONLY_ON_LEVEL_1 = true;

        private readonly Regex DEFAULT_REGEX_PREFIX = new Regex("^[0-9]+ ?");

        public static readonly int DEFAULT_LINK_LEVEL = 2;

        public static readonly bool DEFAULT_USE_ABSOLUTE_URLS = false;

        
        private Dynamic.Schema embeddedSchema = null;

        public Sitemap() : base(TemplatingLogger.GetLogger(typeof(Sitemap)))
        {
        }

        public enum FieldNames { url, changeFrequency, priority, metaRobot, lastMod, navigationItem }

        private string BaseUrl
        {
            get
            {
                Item baseUrl = Package.GetByName("BaseUrl");
                if (baseUrl == null)
                    return "";
                return baseUrl.GetAsString();
            }
        }

        private string IndexFilename
        {
            get
            {
                Item indexFilename = Package.GetByName("IndexFilename");
                if (indexFilename == null)
                    return DEFAULT_INDEX_FILENAME;
                return indexFilename.GetAsString();
            }
        }

        private Regex RegexPrefix
        {
            get
            {
                Item regexPrefix = Package.GetByName("RegexPrefix");
                if (regexPrefix == null)
                    return DEFAULT_REGEX_PREFIX;
                return new Regex(regexPrefix.GetAsString());
            }
        }

        private string SitemapComponentTemplateTitle
        {
            get
            {
                Item item = Package.GetByName("SitemapComponentTemplateTitle");
                if (item == null)
                    return DEFAULT_SITEMAP_CT_TITLE;
                return item.GetAsString();
            }
        }

        private string SitemapComponentTitle
        {
            get
            {
                Item item = Package.GetByName("SitemapComponentTitle");
                if (item == null)
                    return DEFAULT_SITEMAP_COMPONENT_TITLE;
                return item.GetAsString();
            }
        }

        private string SitemapComponentViewName
        {
            get
            {
                Item item = Package.GetByName("SitemapComponentViewName");
                if (item == null)
                    return DEFAULT_SITEMAP_COMPONENT_VIEWNAME;
                return item.GetAsString();
            }
        }

        private string SitemapRootElementName
        {
            get
            {
                Item item = Package.GetByName("SitemapRootElementName");
                if (item == null)
                    return DEFAULT_SITEMAP_ROOT_ELEMENT_NAME;
                return item.GetAsString();
            }
        }

        private string SitemapSchemaTitle
        {
            get
            {
                Item item = Package.GetByName("SitemapSchemaTitle");
                if (item == null)
                    return DEFAULT_SITEMAP_SCHEMA_TITLE;
                return item.GetAsString();
            }
        }

        private int LinkLevels
        {
            get
            {
                Item item = Package.GetByName("LinkLevels");
                if (item == null)
                    return DEFAULT_LINK_LEVEL;
                return Convert.ToInt32(item.GetAsString()); ;
            }
        }

        private bool UseAbsoluteUrls
        {
            get
            {
                Item item = Package.GetByName("UseAbsoluteUrls");
                if (item == null)
                    return DEFAULT_USE_ABSOLUTE_URLS;
                var raw = item.GetAsString();
                return raw.Equals("yes", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        #region DynamicDeliveryTransformer Members

        protected override void TransformPage(Dynamic.Page page)
        {
            Publication pub = (Publication)GetTcmPage().ContextRepository;
            Dynamic.Component c = (Dynamic.Component)DD4TUtil.CreateComponent(SitemapComponentTitle, SitemapRootElementName, SitemapSchemaTitle, page.Publication.Id, page.OwningPublication.Id);
            Dynamic.ComponentTemplate ct = (Dynamic.ComponentTemplate)DD4TUtil.CreateComponentTemplate(SitemapComponentTemplateTitle, SitemapSchemaTitle, SitemapComponentViewName, page.Publication.Id, page.OwningPublication.Id);
            Dynamic.ComponentPresentation cp = (Dynamic.ComponentPresentation)DD4TUtil.CreateComponentPresentation(c, ct);
            if (page.ComponentPresentations == null || page.ComponentPresentations.Count == 0)
            {
                page.ComponentPresentations = new List<Dynamic.ComponentPresentation>();
            }
            page.ComponentPresentations.Add(cp);

            embeddedSchema = new Dynamic.Schema()
            {
                RootElementName = EmbeddedRootElementName,
                Title = EmbeddedRootElementName,
                Id = "tcm:0-0-0"
            };

            // set the title of the top level fieldset to the title of the root structure group
            cp.Component.Fields.Add("title", new Dynamic.Field()
            {
                Name = "title",
                FieldType = Dynamic.FieldType.Text,
                Values = new List<string>() { pub.RootStructureGroup.Title }
            });

            if (pub.RootStructureGroup.Metadata != null)
            {
                AddMetadataToFieldSet(pub.RootStructureGroup, cp.Component.Fields);
            }


            TraverseStructureGroup(pub.RootStructureGroup, "", cp.Component.Fields);
        }

        #endregion DynamicDeliveryTransformer Members

        #region Private Members

        private OrganizationalItemItemsFilter _pageAndSGFilter;

        private string EmbeddedRootElementName
        {
            get
            {
                return FieldNames.navigationItem.ToString();
            }
        }

        private OrganizationalItemItemsFilter PageAndSGFilter
        {
            get
            {
                if (_pageAndSGFilter == null)
                {
                    _pageAndSGFilter = new OrganizationalItemItemsFilter(Engine.GetSession());
                    _pageAndSGFilter.ItemTypes = new List<ItemType>() { ItemType.Page, ItemType.StructureGroup };
                }
                return _pageAndSGFilter;
            }
        }

        private Dynamic.Field CreateField(string fieldName, string fieldValue, DD4T.ContentModel.FieldType fieldType = DD4T.ContentModel.FieldType.Text)
        {
            return new DD4T.ContentModel.Field()
            {
                Name = fieldName,
                Values = new List<string>() { fieldValue },
                FieldType = fieldType
            };
        }
        private Dynamic.Field CreateField(string fieldName, DateTime fieldValue, DD4T.ContentModel.FieldType fieldType = DD4T.ContentModel.FieldType.Date)
        {
            return new DD4T.ContentModel.Field()
            {
                Name = fieldName,
                DateTimeValues = new List<DateTime>() { fieldValue },
                FieldType = fieldType
            };
        }

        private Dynamic.FieldSet CreateFieldSet(Page page, string directoryPath)
        {
            Dynamic.FieldSet fieldSet = new Dynamic.FieldSet();

            fieldSet.Add("id", new Dynamic.Field()
            {
                Name = "id",
                FieldType = Dynamic.FieldType.Text,
                Values = new List<string>() { page.Id }
            });

            fieldSet.Add("title", new Dynamic.Field()
            {
                Name = "title",
                FieldType = Dynamic.FieldType.Text,
                Values = new List<string>() { RegexPrefix.Replace(page.Title, "") }
            });

            fieldSet.Add("visible", new Dynamic.Field()
            {
                Name = "visible",
                FieldType = Dynamic.FieldType.Text,
                Values = new List<string>() { Convert.ToString(true) }
            });

            bool status = FillFieldSet(fieldSet, page, directoryPath);
            if (status)
                return fieldSet;
            return null;
        }

        private bool FillFieldSet(Dynamic.FieldSet fieldSet, Page page, string directoryPath)
        {
            bool isPublished = false;
            DateTime lastMod = DateTime.MinValue;
            Uri uri = null;
            if (Engine.PublishingContext.PublicationTarget != null)
            {
                Log.Debug(string.Format("Found PublicationTarget {0}, checking if page {1} is published to this target", Engine.PublishingContext.PublicationTarget.Title, page.Title));
                System.Collections.Generic.ICollection<PublishInfo> publishCollections = PublishEngine.GetPublishInfo(page);
                foreach (PublishInfo publishInfo in publishCollections)
                {
                    if (publishInfo.PublicationTarget.Id == Engine.PublishingContext.PublicationTarget.Id && publishInfo.Publication.Id == page.ContextRepository.Id)
                    {
                        Log.Debug(string.Format("Page was published to target {0} at {1}", publishInfo.PublicationTarget.Title, publishInfo.PublishedAt));
                        lastMod = publishInfo.PublishedAt;
                        uri = page.GetPublishUrl(publishInfo.TargetType);
                        isPublished = true;
                    }
                }
            }
            else
            {
                Log.Debug(string.Format("Checking if page {0} is published to any target", page.Title));
                System.Collections.Generic.ICollection<PublishInfo> publishCollections = PublishEngine.GetPublishInfo(page);
                foreach (PublishInfo publishInfo in publishCollections)
                {
                    if (publishInfo.Publication.Id == page.ContextRepository.Id)
                    {
                        Log.Debug(string.Format("Page was published to target {0} at {1}", publishInfo.PublicationTarget.Title, publishInfo.PublishedAt));
                        lastMod = publishInfo.PublishedAt;
                        isPublished = true;
                    }
                }
                Log.Debug($"BaseUrl: {BaseUrl}, PublicationUrl: {((Publication)page.ContextRepository).PublicationUrl}, directoryPath: {directoryPath}, filename: {page.GetFileNameWithExtension()}");
                var calculatedUrl = (string.IsNullOrEmpty(BaseUrl) ? "http://localhost" : BaseUrl) + ((Publication)page.ContextRepository).PublicationUrl + "/" + directoryPath + "/" + page.GetFileNameWithExtension();
                Log.Debug($"Calculated url: {calculatedUrl}");
                var normalizedUrl = calculatedUrl.NormalizeUrl();
                Log.Debug($"Normalized url: {normalizedUrl}");
                uri = new Uri (normalizedUrl);
            }


            if (!isPublished)
            {
                Log.Info(string.Format("page {0} ({1}) is excluded because it is not published to the current target", page.Title, page.Id));
                return false;
            }


            if (fieldSet.ContainsKey(FieldNames.url.ToString()))
            {
                Log.Debug($"found {FieldNames.url.ToString()} already, removing it now");
                fieldSet.Remove(FieldNames.url.ToString());
            }
            fieldSet.Add(FieldNames.lastMod.ToString(), CreateField(FieldNames.lastMod.ToString(), lastMod));
            Log.Debug(string.Format("Added field {0} with default value {1}", FieldNames.lastMod.ToString(), lastMod));

            string url;
            if (UseAbsoluteUrls && !string.IsNullOrEmpty(BaseUrl))
            {
                uri = new Uri($"{BaseUrl}{uri.LocalPath}".NormalizeUrl());
                Log.Debug("you want absolute URLs and you have configured your own base url, I have converted the url to {uri}");
            }

            Log.Debug($"Page metadata: {page.Metadata}");
            if (page.Metadata == null)
            {
                // page has no metadata, no need to go looking any further
                // just set the url and the various defaults and return              

                fieldSet.Add(FieldNames.url.ToString(), CreateField(FieldNames.url.ToString(), UseAbsoluteUrls ? uri.AbsoluteUri : uri.LocalPath));
                Log.Debug(string.Format("Added field {0} with value {1}", FieldNames.url.ToString(), UseAbsoluteUrls ? uri.AbsoluteUri : uri.LocalPath));

                fieldSet.Add(FieldNames.changeFrequency.ToString(), CreateField(FieldNames.changeFrequency.ToString(), DEFAULT_CHANGE_FREQUENCY));
                Log.Debug(string.Format("Added field {0} with value {1}", FieldNames.changeFrequency.ToString(), DEFAULT_CHANGE_FREQUENCY));

                fieldSet.Add(FieldNames.priority.ToString(), CreateField(FieldNames.priority.ToString(), DEFAULT_PRIORITY));
                Log.Debug(string.Format("Added field {0} with value {1}", FieldNames.priority.ToString(), DEFAULT_PRIORITY));

                fieldSet.Add(FieldNames.metaRobot.ToString(), CreateField(FieldNames.metaRobot.ToString(), DEFAULT_META_ROBOT));
                Log.Debug(string.Format("Added field {0} with value {1}", FieldNames.metaRobot.ToString(), DEFAULT_META_ROBOT));

                return true;
            }
            ItemFields metadataFields = new ItemFields(page.Metadata, page.MetadataSchema);

            var urlField = GetOrCreateField(metadataFields, FieldNames.url.ToString(), UseAbsoluteUrls ? uri.AbsoluteUri : uri.LocalPath);
            fieldSet.Add(FieldNames.url.ToString(), urlField);
            Log.Debug(string.Format("Added field {0} with value {1}", urlField.Name, urlField.Value));

            var changeFreq = GetOrCreateField(metadataFields, FieldNames.changeFrequency.ToString(), DEFAULT_CHANGE_FREQUENCY);
            fieldSet.Add(FieldNames.changeFrequency.ToString(), changeFreq);
            Log.Debug(string.Format("Added field {0} with value {1}", changeFreq.Name, changeFreq.Value));

            var priority = GetOrCreateField(metadataFields, FieldNames.priority.ToString(), DEFAULT_PRIORITY);
            fieldSet.Add(FieldNames.priority.ToString(), priority);
            Log.Debug(string.Format("Added field {0} with value {1}", priority.Name, priority.Value));

            var metaRobot = GetOrCreateField(metadataFields, FieldNames.metaRobot.ToString(), DEFAULT_META_ROBOT);
            fieldSet.Add(FieldNames.metaRobot.ToString(), metaRobot);
            Log.Debug(string.Format("Added field {0} with value {1}", metaRobot.Name, metaRobot.Value));

            return true;
        }

        private Dynamic.Field GetOrCreateField(ItemFields fields, string fieldName, string defaultValue)
        {
            try
            {
                Log.Debug($"Processing field {fieldName}, defaultValue = {defaultValue}");
                if (fields.Contains(fieldName))
                {
                    Log.Debug($"Building field for {fieldName}");
                    return FieldBuilder.BuildField(fields[fieldName], 1, new BuildManager(Package, Engine));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}");
            }
            if (defaultValue == null)
            {
                return null;
            }

            return new DD4T.ContentModel.Field()
            {
                Name = fieldName,
                Values = new List<string>() { defaultValue },
                FieldType = DD4T.ContentModel.FieldType.Text
            };
        }

        private bool TraverseStructureGroup(StructureGroup structureGroup, string directoryPath, Dynamic.FieldSet currentFieldSet)
        {
            Log.Debug(string.Format("examining structure group {0} (base url {1}", structureGroup.Title, directoryPath));
            List<Dynamic.FieldSet> children = new List<Dynamic.FieldSet>();
            currentFieldSet.Add("childItems", new Dynamic.Field()
            {
                FieldType = Dynamic.FieldType.Embedded,
                Name = "childItems",
                EmbeddedSchema = embeddedSchema,
                EmbeddedValues = children
            }
            );
            bool hasPublishedPages = false;
            foreach (var child in structureGroup.GetItems(PageAndSGFilter).OrderBy(a => a.Title))
            {
                if (child.Title.StartsWith("_"))
                {
                    Log.Debug(string.Format("skipping {0} ({1}) because title starts with underscore", child.Title, child.Id));
                    continue;
                }
                Page childPage = child as Page;
                if (childPage != null)
                {
                    if (childPage.FileName == IndexFilename)
                    {
                        Log.Debug(string.Format("found index page {0} ({1}), using it to enrich the parent node in the sitemap ({2})", childPage.Title, childPage.Id, structureGroup.Title));
                        hasPublishedPages = hasPublishedPages | FillFieldSet(currentFieldSet, childPage, directoryPath);
                        continue;
                    }
                    Log.Debug(string.Format("found regular page {0} ({1}), adding it to the sitemap", childPage.Title, childPage.Id));
                    Dynamic.FieldSet fieldset = CreateFieldSet(childPage, directoryPath);
                    hasPublishedPages = hasPublishedPages | fieldset != null;
                    if (fieldset != null)
                    {
                        children.Add(fieldset);
                    }
                }
                else
                {
                    bool visible = true;
                    // Child is a Structure Group
                    Log.Debug(string.Format("found SG {0}", child.Title));
                    if (FILTER_BY_NUMERIC_PREFIX && (FILTER_BY_NUMERIC_PREFIX_ONLY_ON_LEVEL_1 == false || directoryPath == "") && !RegexPrefix.IsMatch(child.Title))
                    {
                        Log.Debug("excluding from navigation because it does not start with a numeric prefix");
                        visible = false;
                    }

                    Dynamic.FieldSet childFieldSet = new Dynamic.FieldSet();
                    childFieldSet.Add("id", new Dynamic.Field()
                    {
                        Name = "id",
                        FieldType = Dynamic.FieldType.Text,
                        Values = new List<string>() { child.Id }
                    }
                    );
                    childFieldSet.Add("title", new Dynamic.Field()
                    {
                        Name = "title",
                        FieldType = Dynamic.FieldType.Text,
                        Values = new List<string>() { RegexPrefix.Replace(child.Title, "") }
                    }
                    );
                 
                    childFieldSet.Add("visible", new Dynamic.Field()
                    {
                        Name = "visible",
                        FieldType = Dynamic.FieldType.Text,
                        Values = new List<string>() { Convert.ToString(visible) }
                    }
                    );

                    if (child.Metadata != null)
                    {
                        AddMetadataToFieldSet(child, childFieldSet);
                    }

                    // hasPublishedPages is true if:
                    // - it was already true (i.e. the current structure group contains one or more published pages), OR
                    // - it is true for one of its children
                    Log.Debug(string.Format("before traversing into {1}, hasPublishedPages is {0}", hasPublishedPages, child.Title));
                    bool childHasPublishedPages = TraverseStructureGroup((StructureGroup)child, directoryPath + "/" + ((StructureGroup)child).Directory, childFieldSet);
                    if (childHasPublishedPages)
                    {
                        children.Add(childFieldSet);
                        Log.Debug(string.Format("{1} has published pages: {0}", childHasPublishedPages, child.Title));
                    }
                    hasPublishedPages = hasPublishedPages | childHasPublishedPages;
                    Log.Debug(string.Format("after traversing into {1}, hasPublishedPages is {0}", hasPublishedPages, child.Title));
                }
            }

            return hasPublishedPages;
        }

        private void AddMetadataToFieldSet(RepositoryLocalObject item, Dynamic.FieldSet childFieldSet)
        {
            Log.Debug(string.Format("Structure Group {0} has metadata", item.Title));

            if (childFieldSet.ContainsKey("metadata"))
            {
                Log.Debug($"found metadata already, removing it now");
                childFieldSet.Remove("metadata");
            }
            var bm = new BuildManager(Package, Engine);

            ItemFields metadataFields = new ItemFields(item.Metadata, item.MetadataSchema);

            List<Dynamic.FieldSet> embeddedValuesList = new List<Dynamic.FieldSet>();
            childFieldSet.Add("metadata", new Dynamic.Field()
            {
                FieldType = Dynamic.FieldType.Embedded,
                Name = "metadata",
                EmbeddedSchema = embeddedSchema,
                EmbeddedValues = embeddedValuesList
            });
            Dynamic.FieldSet dynamicFieldSet = bm.BuildFields(metadataFields, LinkLevels);
            embeddedValuesList.Add(dynamicFieldSet);
        }

        #endregion Private Members
    }
}