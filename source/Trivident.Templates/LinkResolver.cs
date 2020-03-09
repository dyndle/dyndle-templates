using DD4T.Templates.Base;
using DD4T.Templates.Base.Utils;
using System.Collections.Generic;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Dynamic = DD4T.ContentModel;
using System;
using DD4T.Templates.Base.Xml;
using System.Xml;
using DD4T.Templates.Base.Builder;
using Tridion.ContentManager.Templating.Assembly;

namespace Trivident.Templates
{
    [TcmTemplateTitle("Link Resolver")]
    [TcmTemplateParameterSchema("resource:Trivident.Templates.Resources.Link Resolver Parameters.xsd")]
    public class LinkResolver : BaseComponentTemplate
    {
        public LinkResolver() : base(TemplatingLogger.GetLogger(typeof(LinkResolver)))
        {
        }

        protected override void TransformComponent(Dynamic.Component component)
        {
            ResolveFieldSet(component.Fields);
            ResolveFieldSet(component.MetadataFields);
        }

        private void ResolveFieldSet(Dynamic.IFieldSet fields)
        {
            foreach (var kvp in fields)
            {
                Dynamic.IField field = kvp.Value;
                if (field.FieldType == DD4T.ContentModel.FieldType.Embedded)
                {
                    foreach (var fieldSet in field.EmbeddedValues)
                    {
                        ResolveFieldSet(fieldSet);
                    }
                    continue;
                }
                if (field.FieldType == DD4T.ContentModel.FieldType.Xhtml)
                {
                    List<string> newValues = new List<string>();
                    foreach (var xhtml in field.Values)
                    {
                        Log.Debug("Found rich text field with value:");
                        Log.Debug(xhtml);
                        TridionXml xml = new TridionXml();
                        xml.LoadXml("<tmproot>" + xhtml + "</tmproot>");
                        bool foundBinaryLinks = false;
                        foreach (XmlElement xlinkElement in xml.SelectNodes("//*[@xlink:href[starts-with(string(.),'tcm:')]]", xml.NamespaceManager))
                        {
                            Log.Debug("Found XLink in Rich Text: " + xlinkElement.OuterXml);
                            foundBinaryLinks = ProcessRichTextXlink(xlinkElement) || foundBinaryLinks;
                        }
                        if (foundBinaryLinks)
                        {
                            newValues.Add(BuildManager.PublishBinariesInRichTextField(xml.OuterXml).Replace("<tmproot>", "").Replace("</tmproot>", ""));
                        }
                        else
                        {
                            newValues.Add(xml.OuterXml.Replace("<tmproot>", "").Replace("</tmproot>", ""));
                        }
                    }
                    ((Dynamic.Field)field).Values = newValues;
                }
            }
        }

        /// <summary>
        /// Process a link element. If it contains a link to a component based on the schema 'Link' (name is configurable), internal / external / binary links within that Link component are used instead of the link to the Link component
        /// If there is a parameters field, add the parameters as an attribute 'params'
        /// </summary>
        /// <param name="xlinkElement">Xml element representing a link (e.g. &;t;a href="" /&gt;)</param>
        /// <returns></returns>
        private bool ProcessRichTextXlink(XmlElement xlinkElement)
        {
            bool foundBinaryLinks = false;
            bool isExternalLink = false;
            const string xlinkNamespaceUri = "http://www.w3.org/1999/xlink";

            string xlinkHref = xlinkElement.GetAttribute("href", xlinkNamespaceUri);
            if (string.IsNullOrEmpty(xlinkHref))
            {
                Log.Warning("No xlink:href found: " + xlinkElement.OuterXml);
                return false;
            }

            Component component = Engine.GetObject(xlinkHref) as Component;
            if (component == null || component.Schema.Title != LinkSchemaTitle)
            {
                // XLink doesn't refer to a Link Component; do nothing.
                return false;
            }
            Log.Debug("Processing XLink to Link Component: " + component.Id);
            Dynamic.Component linkComponent = BuildManager.BuildComponent(component);
            Log.Debug($"created dynamic component {linkComponent.Title}");

            if (linkComponent.Fields.ContainsKey(InternalLinkFieldName))
            {
                Log.Debug("Found internal link in link component");
                var internalLink = linkComponent.Fields[InternalLinkFieldName];
                if (internalLink.LinkedComponentValues.Count > 0)
                {
                    var tcmUri = internalLink.LinkedComponentValues[0].Id;
                    xlinkElement.SetAttribute("href", xlinkNamespaceUri, tcmUri);
                    Log.Debug($"Replaced link to {xlinkHref} with {tcmUri}");

                }
            }

            if (linkComponent.Fields.ContainsKey(ExternalLinkFieldName))
            {
                Log.Debug("Found external link in link component");
                isExternalLink = true;
                var externalLinkField = linkComponent.Fields[ExternalLinkFieldName];

                if (externalLinkField.Values.Count > 0)
                {
                    var url = externalLinkField.Values[0];
                    xlinkElement.SetAttribute("href", url);
                    xlinkElement.RemoveAttribute("href", xlinkNamespaceUri);
                    Log.Debug($"Replaced link to {xlinkHref} with {url}");
                }
            }

            if (linkComponent.Fields.ContainsKey(BinaryLinkFieldName))
            {
                foundBinaryLinks = true;
                Log.Debug("Found binary link in link component");
                var binaryLink = linkComponent.Fields[BinaryLinkFieldName];
                if (binaryLink.LinkedComponentValues.Count > 0)
                {
                    var tcmUri = binaryLink.LinkedComponentValues[0].Id;
                    xlinkElement.SetAttribute("href", xlinkNamespaceUri, tcmUri);
                    Log.Debug($"Replaced binary link to {xlinkHref} with {tcmUri}");
                }
            }

            if ((!isExternalLink) && (!string.IsNullOrEmpty(ParametersFieldName)) && linkComponent.Fields.ContainsKey(ParametersFieldName))
            {
                var parameters = linkComponent.Fields[ParametersFieldName];
                if (parameters.Values.Count > 0)
                {
                    var parameter = parameters.Values[0].Replace("\"", "&quot;");
                    xlinkElement.SetAttribute("params", parameter);
                }
            }
            
            if (isExternalLink)
            {
                xlinkElement.RemoveAttribute("title");
            }
            return foundBinaryLinks;
        }

        private BuildManager _buildManager;
        protected BuildManager BuildManager
        {
            get
            {
                if (_buildManager == null)
                {
                    _buildManager = new BuildManager(Package, Engine);
                }
                return _buildManager;
            }
        }

        protected string LinkSchemaTitle
        {
            get
            {
                if (Package == null)
                {
                    return null;
                }
                if (Package.GetByName("LinkSchemaTitle") == null)
                {
                    return null;
                }
                return Package.GetByName("LinkSchemaTitle").GetAsString();
            }
        }
        protected string InternalLinkFieldName
        {
            get
            {
                if (Package == null)
                {
                    return null;
                }
                if (Package.GetByName("InternalLinkFieldName") == null)
                {
                    return null;
                }
                return Package.GetByName("InternalLinkFieldName").GetAsString();
            }
        }
        protected string ExternalLinkFieldName
        {
            get
            {
                if (Package == null)
                {
                    return null;
                }
                if (Package.GetByName("ExternalLinkFieldName") == null)
                {
                    return null;
                }
                return Package.GetByName("ExternalLinkFieldName").GetAsString();
            }
        }
        protected string BinaryLinkFieldName
        {
            get
            {
                if (Package == null)
                {
                    return null;
                }
                if (Package.GetByName("BinaryLinkFieldName") == null)
                {
                    return null;
                }
                return Package.GetByName("BinaryLinkFieldName").GetAsString();
            }
        }
        protected string ParametersFieldName
        {
            get
            {
                if (Package == null)
                {
                    return null;
                }
                if (Package.GetByName("ParametersFieldName") == null)
                {
                    return null;
                }
                return Package.GetByName("ParametersFieldName").GetAsString();
            }
        }
    }
}