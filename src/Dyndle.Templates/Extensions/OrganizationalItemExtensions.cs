using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;

namespace Tridion.ContentManager.ContentManagement.Fields
{
    public static class OrganizationalItemExtensions
    {
        public static IEnumerable<Component> GetComponents(this OrganizationalItem folder, bool recursive)
        {
            return GetComponents(folder, Enumerable.Empty<Schema>(), recursive);
        }

        public static IEnumerable<Component> GetComponents(this OrganizationalItem folder, IEnumerable<Schema> basedOnSchemas, bool recursive)
        {
            var filter = new OrganizationalItemItemsFilter(folder.Session)
                             {
                                 ItemTypes = new[] { ItemType.Component },
                                 Recursive = recursive,
                                 BasedOnSchemas = basedOnSchemas
                             };

            return folder.GetItems(filter).Cast<Component>();
        }

        public static IEnumerable<Page> GetPages(this OrganizationalItem sg)
        {
            return GetPages(sg, false);
        }

        public static IEnumerable<Page> GetPages(this OrganizationalItem sg, bool recursive)
        {
            var filter = new OrganizationalItemItemsFilter(sg.Session)
                             {
                                 ItemTypes = new[] { ItemType.Page },
                                 Recursive = recursive
                             };

            return sg.GetItems(filter).Cast<Page>();
        }

        public static IEnumerable<StructureGroup> GetStructureGroups(this OrganizationalItem sg)
        {
            return GetStructureGroups(sg, false);
        }

        public static IEnumerable<StructureGroup> GetStructureGroups(this OrganizationalItem sg, bool recursive)
        {
            var filter = new OrganizationalItemItemsFilter(sg.Session)
                             {
                                 ItemTypes = new[] { ItemType.StructureGroup },
                                 Recursive = recursive
                             };

            return sg.GetItems(filter).Cast<StructureGroup>();
        }

        public static IList<KeyValuePair<TcmUri, string>> GetOrganizationalItemContents(this OrganizationalItem orgItem, ItemType[] itemTypes, bool recursive)
        {
            OrganizationalItemItemsFilter filter = new OrganizationalItemItemsFilter(orgItem.Session)
            {
                ItemTypes = itemTypes,
                Recursive = recursive
            };

            return (from XmlNode item in orgItem.GetListItems(filter).SelectNodes("/*/*")
                    select new KeyValuePair<TcmUri, string>(new TcmUri(item.Attributes["ID"].Value), item.Attributes["Title"].Value)).ToList();
        }
    }
}