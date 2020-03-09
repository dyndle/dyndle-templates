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

namespace Trivident.Templates
{
    [TcmTemplateTitle("Include original page title as metadata")]
    [TcmTemplateParameterSchema("resource:Trivident.Templates.Resources.OriginalPageTitle Parameters.xsd")]
    public class OriginalPageTitle : BasePageTemplate
    {
        public OriginalPageTitle() : base(TemplatingLogger.GetLogger(typeof(OriginalPageTitle)))
        {
        }

        public static readonly string DEFAULT_ORIGINAL_PAGE_TITLE_METADATA_FIELD = "originalPageTitle";

        private string OriginalPageTitleFieldName
        {
            get
            {
                Item originalPageTitleItem = Package.GetByName("OriginalPageTitleFieldName");
                if (originalPageTitleItem == null)
                    return DEFAULT_ORIGINAL_PAGE_TITLE_METADATA_FIELD;
                return originalPageTitleItem.GetAsString();
            }
        }


        #region DynamicDeliveryTransformer Members
        protected override void TransformPage(Dynamic.Page page)
        {

            // make sure the MetadataFields property has been initialized
            if (page.MetadataFields == null)
            {
                Log.Debug("creating metadata fieldset for page");
                page.MetadataFields = new Dynamic.FieldSet();
            }

            // find nearest English ancestor publication
            Repository originalPublication = FindOriginalPublication(GetTcmPage().OwningRepository, GetTcmPage());

            // check if the page being published is an original
            if (page.Publication.Id == originalPublication.Id)
            {
                // original page title is the same as the current page title
                page.MetadataFields.Add(OriginalPageTitleFieldName, CreateField(OriginalPageTitleFieldName, page.Title));
                Log.Debug($"Added metadata field {OriginalPageTitleFieldName} for page with value {page.Title} (page is created within current publication)");
                return;
            }

            var originalPageUri = GetContextUri(page.Id, originalPublication.Id);
            Log.Debug($"{page.Id} | {originalPageUri}");
            Page originalPage = (Page) Engine.GetObject(originalPageUri.ToString());
            page.MetadataFields.Add(OriginalPageTitleFieldName, CreateField(OriginalPageTitleFieldName, originalPage.Title));
            Log.Debug($"Added metadata field {OriginalPageTitleFieldName} for page with value {originalPage.Title} (page is shared from parent publication {originalPublication.Id})");

        }

        private Repository FindOriginalPublication(Repository owningRepository, Page page)
        {
            Log.Debug($">> FindOriginalPublication({owningRepository.Title}, {page.Id})");
            if (owningRepository.Parents == null || owningRepository.Parents.Count == 0)
            {
                return owningRepository;
            }

            Repository theRealParent = null;
            foreach (var pp in owningRepository.Parents)
            {
                Log.Debug($"parent repo: {pp.Title}");
                if (! ItemExists(page.Id, pp))
                {
                    continue;
                }
                Log.Debug("page found in publication " + pp.Title);
                theRealParent = FindOriginalPublication(pp, page);
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

        #endregion

        #region Private Members
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