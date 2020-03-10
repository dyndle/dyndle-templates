using System.Xml;
using Tridion.ContentManager.ContentManagement;

namespace Tridion.ContentManager.Templating
{
    public static class PackageExtensionMethods
    {
        public static Item PushStringItem(this Package package, string name, string value)
        {
            return package.PushStringItem(name, ContentType.Text, value);
        }

        public static Item PushStringItem(this Package package, string name, ContentType contentType, string value)
        {
            var item = package.CreateStringItem(contentType, value);
            package.PushItem(name, item);

            return item;
        }

        public static Item PushHTMLItem(this Package package, string name, string value)
        {
            var item = package.CreateHtmlItem(value);
            package.PushItem(name, item);

            return item;
        }

        public static Item PushXmlItem(this Package package, string name, XmlDocument value)
        {
            var item = package.CreateXmlDocumentItem(ContentType.Xml, value);
            package.PushItem(name, item);

            return item;
        }

        public static void PushBinary(this Package package, Component component)
        {
            var item = package.CreateTridionItem(ContentType.Gif, component);
            package.PushItem(item);
        }

        public static void PutMainComponentOnTop(this Package package)
        {
          package.RepushItem(Package.ComponentName);
        }

        public static void RemoveItem(this Package package, string name)
        {
            var item = package.GetByName(name);

            if (item == null) return;
            package.Remove(item);
        }

        public static void RepushItem(this Package package, string name)
        {
            var item = package.GetByName(name);

            if (item != null)
            {
                package.Remove(item);
                package.PushItem(name, item);
            }
        }

        public static XmlDocument GetXmlItemValue(this Package package, string itemName)
        {
            XmlDocument doc = null;
            var item = package.GetValue(itemName);
            if (!string.IsNullOrEmpty(item))
            {
                doc = new XmlDocument();
                doc.LoadXml(item);
            }
            return doc;
        }

        /// <summary>
        /// Returns the string value for an item in the package with the specified name
        /// </summary>
        /// <param name="package">TOM.Net Package</param>
        /// <param name="itemName">the name of the item to get the value from the package for</param>
        /// <returns>The string value of the item in the package or empty string if the item cannot be found</returns>
        /// <remarks>Same functionalty as Package.GetValue() but instead of null returns empty string if not found</remarks>
        public static string GetStringItemValue(this Package package, string itemName)
        {
            return package.GetStringItemValue(itemName, false);
        }

        /// <summary>
        /// Returns the string value for an item in the package with the specified name
        /// </summary>
        /// <param name="package">TOM.Net Package</param>
        /// <param name="itemName">the name of the item to get the value from the package for</param>
        /// <param name="dblCheck">whether to perfrom a second lookup using the value returned from the package</param>
        /// <returns>The string value of the item in the package or empty string if the item cannot be found</returns>
        /// <remarks>Same functionalty as Package.GetValue() but instead of null returns empty string if not found</remarks>
        public static string GetStringItemValue(this Package package, string itemName, bool dblCheck)
        {
            var res = package.GetValue(itemName);

            if (dblCheck && !string.IsNullOrEmpty(res))
            {
                res = package.GetValue(res);
            }

            return res ?? string.Empty;
        }
    }
}