using DD4T.Templates.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
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

namespace Dyndle.Templates
{
    [TcmTemplateTitle("Add translated status component metadata")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.AddTranslationStatus Parameters.xsd")]
    public class AddTranslationStatus : BaseComponentTemplate
    {
        public AddTranslationStatus() : base(TemplatingLogger.GetLogger(typeof(AddTranslationStatus)))
        {
        }

        public static readonly string DEFAULT_TRANSLATION_STATUS_METADATA_FIELD = "translationStatus";

        private string TranslationStatusFieldName
        {
            get
            {
                Item fieldNameItem = Package.GetByName("TranslationStatusFieldName");
                if (fieldNameItem == null)
                    return DEFAULT_TRANSLATION_STATUS_METADATA_FIELD;
                return fieldNameItem.GetAsString();
            }
        }


        #region DynamicDeliveryTransformer Members
        protected override void TransformComponent(Dynamic.Component component)
        {
            // make sure the MetadataFields property has been initialized
            if (component.MetadataFields == null)
            {
                Log.Debug("creating metadata fieldset for component");
                component.MetadataFields = new Dynamic.FieldSet();
            }

            // find out if the component is translated by looking at the difference
            // between the title of the context publication and the owning publication
            // The publication titles end with a dash followed by the language.

            Repository contextRepo = GetTcmComponent().ContextRepository;
            Repository originalRepo = FindOriginalPublication(GetTcmComponent().OwningRepository, GetTcmComponent());
            string originalPubLanguage = "EN";
            Repository owningRepo = GetTcmComponent().OwningRepository;

            Log.Debug($"original pub title {originalRepo.Title}");
            Log.Debug($"owning pub title {owningRepo.Title}");
            Log.Debug($"context pub title {contextRepo.Title}");


            if (originalRepo.Metadata != null)
            {
                var lang = originalRepo.Metadata.SelectSingleNode("*[local-name()='language']");
                if (lang != null)
                {
                    originalPubLanguage = lang.InnerText;
                }
            }
            string owningPubLanguage = "EN";
            if (owningRepo.Metadata != null)
            {
                var lang = owningRepo.Metadata.SelectSingleNode("*[local-name()='language']");
                if (lang != null)
                {
                    owningPubLanguage = lang.InnerText;
                }
            }

            Log.Debug($"Owning pub language {owningPubLanguage}");
            Log.Debug($"Original pub language {originalPubLanguage}");
            Log.Debug($"Component version {GetTcmComponent().Version}");
            bool isTranslated = owningPubLanguage != originalPubLanguage && GetTcmComponent().Version > 1;
            var v = isTranslated ? "translated" : "original";
            component.MetadataFields.Add(TranslationStatusFieldName, CreateField(TranslationStatusFieldName, v));
            Log.Debug($"Added metadata field {TranslationStatusFieldName} for component with value {v}");
        }

        #endregion

        #region Private Members
        private Repository FindOriginalPublication(Repository owningRepository, Component component)
        {
            Log.Debug($">> FindOriginalPublication({owningRepository.Title}, {component.Id})");
            if (owningRepository.Parents == null || owningRepository.Parents.Count == 0)
            {
                return owningRepository;
            }

            Repository theRealParent = null;
            foreach (var pp in owningRepository.Parents)
            {
                Log.Debug($"parent repo: {pp.Title}");
                if (!ItemExists(component.Id, pp))
                {
                    continue;
                }
                Log.Debug("page found in publication " + pp.Title);
                theRealParent = FindOriginalPublication(pp, component);
            }
            return theRealParent ?? owningRepository;
        }
        private bool ItemExists(string uri, Repository repo)
        {
            try
            {
                RepositoryLocalObject ppp = (RepositoryLocalObject)Engine.GetObject(GetContextUri(uri, repo.Id));
                return ppp != null;
            }
            catch
            {
                return false;
            }
        }

        private string GetContextUri(string itemUri, string publicationUri)
        {
            TcmUri u1 = new TcmUri(itemUri);
            TcmUri u2 = new TcmUri(publicationUri);
            TcmUri u3 = new TcmUri(u1.ItemId, u1.ItemType, u2.ItemId);
            return u3.ToString();
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
        #endregion
    }
}