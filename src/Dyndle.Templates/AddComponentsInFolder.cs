using System;
using System.Linq;
using DD4T.Templates.Base;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Dynamic = DD4T.ContentModel;

namespace Dyndle.Templates
{
    [TcmTemplateTitle("Add all components in (sub)folders to DD4T page")]
    public class AddComponentsInFolder : BasePageTemplate
    {
        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof (AddComponentsInFolder));

        private static string PARAM_SCHEMA_ROOTELEM_NAME = "schemaRootName";
        private static string PARAM_SCHEMA_CT = "ct";
        private static string PARAM_SCHEMA_LINKLEVEL = "linkLevel";

        private string _schemaRootElem;
        private int _linkLevel = 1;
        private Dynamic.ComponentTemplate _ct;

        private void Init()
        {
            _schemaRootElem = Package.GetValue(PARAM_SCHEMA_ROOTELEM_NAME) ?? string.Empty;
            string level = Package.GetValue(PARAM_SCHEMA_LINKLEVEL) ?? string.Empty;
            if (Int32.TryParse(level, out int linkLevel))
            {
                _linkLevel = linkLevel;
            }

            // get CT to use
            string ctId = Package.GetValue(PARAM_SCHEMA_CT) ?? string.Empty;
            if (!string.IsNullOrEmpty(ctId))
            {
                _ct = new Dynamic.ComponentTemplate();
            }

        }

        /// <summary>
        ///     Gets the first component presentation on the page
        ///     The component is a placeholder to indicate the start folder for all components to be added to the page
        /// </summary>
        /// <param name="dd4tPage"></param>
        protected override void TransformPage(Dynamic.Page dd4tPage)
        {
            Init();           
            if (dd4tPage.ComponentPresentations.Count != 1)
            {
                throw new TemplatingException("There must be at least one Component Presentation on this Page");
            }

            // get folder of placeholder component
            Folder startFolder = (Folder) Engine.GetObject(dd4tPage.ComponentPresentations[0].Component.Folder.Id);

            // get CT to use for rest of CPs
            _ct = dd4tPage.ComponentPresentations.First().ComponentTemplate;

            // remove placeholder CP from page
            dd4tPage.ComponentPresentations.RemoveAt(0);

            LOG.Debug($"Using startfolder {startFolder.Id}");
            // create filter once for reuse
            var filter = new OrganizationalItemItemsFilter(startFolder.Session)
            {
                ItemTypes = new[] {ItemType.Component, ItemType.Folder},
                Recursive = true
            };
            ProcessComponentsInFolder(dd4tPage, startFolder, filter);
        }


        // Process all components in folder and any subfolders and add them as CPs to the DD4T page
        private void ProcessComponentsInFolder(Dynamic.Page dd4tPage, Folder startFolder,
            OrganizationalItemItemsFilter filter)
        {
            LOG.Debug($"Processing items in folder {startFolder.Id}");
            var items = startFolder.GetItems(filter);
            foreach (var item in items)
            {
                var component = item as Component;
                if (component != null)
                {
                    if (UseComponent(component))
                    {
                        AddComponentToPage(dd4tPage, component);
                    }
                }
                else
                {
                    var folder = item as Folder;
                    if (folder != null)
                    {
                        ProcessComponentsInFolder(dd4tPage, folder, filter);
                    }
                }
            }
        }

        private bool UseComponent(Component comp)
        {
            if (!string.IsNullOrEmpty(_schemaRootElem))
            {
                return _schemaRootElem.Equals(comp.Schema.RootElementName, StringComparison.InvariantCultureIgnoreCase);
            }
            return true;
        }

        private void AddComponentToPage(Dynamic.Page dd4tPage, Component comp)
        {
            LOG.Debug($"Adding component {comp.Title} to page");
            dd4tPage.ComponentPresentations.Add(
                new Dynamic.ComponentPresentation
                {
                    Component = Manager.BuildComponent(comp, _linkLevel),
                    ComponentTemplate = _ct
                }
                );
        }
    }
}