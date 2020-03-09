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
    [TcmTemplateTitle("Add owning publication as page metadata")]
    [TcmTemplateParameterSchema("resource:Trivident.Templates.Resources.AddOwningPublication Parameters.xsd")]
    public class AddOwningPublicationToPage : BasePageTemplate
    {
        public AddOwningPublicationToPage() : base(TemplatingLogger.GetLogger(typeof(AddOwningPublicationToPage)))
        {
        }

        public static readonly string DEFAULT_OWNING_PUBLICATION_ID_METADATA_FIELD = "owningPublicationId";

        private string OwningPublicationIdFieldName
        {
            get
            {
                Item owningPublicationIdItem = Package.GetByName("OwningPublicationIdFieldName");
                if (owningPublicationIdItem == null)
                    return DEFAULT_OWNING_PUBLICATION_ID_METADATA_FIELD;
                return owningPublicationIdItem.GetAsString();
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

            // add metadata field with the ID of the page's owning publication
            TcmUri owningPubUri = new TcmUri(page.OwningPublication.Id);
            page.MetadataFields.Add(OwningPublicationIdFieldName, CreateNumberField(OwningPublicationIdFieldName, owningPubUri.ItemId));
            Log.Debug($"Added metadata field {OwningPublicationIdFieldName} for page with value {owningPubUri.ItemId}");
        }

        #endregion

        #region Private Members
        private Dynamic.Field CreateNumberField(string fieldName, double fieldValue, DD4T.ContentModel.FieldType fieldType = DD4T.ContentModel.FieldType.Number)
        {
            return new DD4T.ContentModel.Field()
            {
                Name = fieldName,
                NumericValues = new List<double>() { fieldValue },
                FieldType = fieldType
            };
        }


        #endregion
    }
}