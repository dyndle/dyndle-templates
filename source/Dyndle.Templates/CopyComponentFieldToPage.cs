using System;
using System.Linq;
using DD4T.Templates.Base;
using Tridion.ContentManager;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using DD4T.ContentModel;

namespace Dyndle.Templates
{
    [TcmTemplateTitle("Copy field from component to page")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.CopyComponentFieldToPage Parameters.xsd")]
    public class CopyComponentFieldToPage : BasePageTemplate
    {
        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(CopyComponentFieldToPage));

        private static string PARAM_COMPONENT_FIELD_NAME = "ComponentFieldName";
        private static string PARAM_PAGE_FIELD_NAME = "PageFieldName";
        private static string PARAM_FIRST_CP_ONLY = "FirstCpOnly";

        private string ComponentFieldName
        {
            get
            {
                Item paramVal = Package.GetByName(PARAM_COMPONENT_FIELD_NAME);
                if (paramVal == null)
                {
                    return "";
                }
                return paramVal.GetAsString();
            }
        }

        private string PageFieldName
        {
            get
            {
                Item paramVal = Package.GetByName(PARAM_PAGE_FIELD_NAME);
                if (paramVal == null)
                {
                    return "";
                }
                return paramVal.GetAsString();
            }
        }

        private bool FirstCpOnly
        {
            get
            {
                Item paramVal = Package.GetByName(PARAM_FIRST_CP_ONLY);
                if (paramVal == null)
                {
                    return true;
                }
                return Convert.ToBoolean(paramVal.GetAsString());
            }
        }


        protected override void TransformPage(Page page)
        {
            if (page.ComponentPresentations.Count == 0)
            {
                return;
            }

            LOG.Debug($"started CopyComponentFieldToPage with parameters {ComponentFieldName} and {PageFieldName}");
            
            foreach (DD4T.ContentModel.ComponentPresentation cp in page.ComponentPresentations)
            {
                if (cp.Component.Fields.ContainsKey(ComponentFieldName))
                {
                    CopyFieldToPage(cp.Component.Fields[ComponentFieldName], page);
                    return;
                }
                if (cp.Component.MetadataFields.ContainsKey(ComponentFieldName))
                {
                    CopyFieldToPage(cp.Component.MetadataFields[ComponentFieldName], page);
                    return;
                }
                if (FirstCpOnly)
                {
                    break;
                }
            }
        }

        private void CopyFieldToPage(IField field, Page page)
        {
            if (page.MetadataFields.ContainsKey(field.Name))
            {
                LOG.Warning($"page already contains a field named {field.Name}, no field added to metadata");
            }            
            else
            {
                string usePageFieldName = ComponentFieldName;
                if (!String.IsNullOrEmpty(PageFieldName))
                {
                    usePageFieldName = PageFieldName;
                }
                if (usePageFieldName != ComponentFieldName)
                {
                    ((Field)field).Name = usePageFieldName;
                }
                page.MetadataFields.Add(usePageFieldName, field);
                LOG.Info($"added field named {usePageFieldName} to page metadata");
            }
        }
    }
}