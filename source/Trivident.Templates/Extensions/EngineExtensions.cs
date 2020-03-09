using System;
using System.Linq;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing;

namespace Tridion.ContentManager.Templating
{
    public static class EngineExtensions
    {
        /// <summary>
        /// Retrieves object identified by TcmUri, relative URI, or absolute WebDAV URI.
        /// </summary>
        public static T GetObject<T>(this Engine engine, string itemUriOrWebDavUrl)
            where T: IdentifiableObject
        {
            var item = GetObjectByUriOrDefault(engine, itemUriOrWebDavUrl);

            ThrowExceptionWhenReturnedIdentifiableObjectIsNull(item, itemUriOrWebDavUrl);

            return item as T;
        }

        public static IdentifiableObject GetObjectByUriOrDefault(this Engine engine, string itemUriOrWebDavUrl)
        {
            if (!TcmUri.IsValid(itemUriOrWebDavUrl) && IsPublicationRelativeUrl(itemUriOrWebDavUrl))
            {
                var publication = engine.GetContextPublication();
                itemUriOrWebDavUrl = GetFullWebDavUrl(publication.Title, itemUriOrWebDavUrl);
            }
            
            return engine.GetObject(itemUriOrWebDavUrl);
        }

        public static IdentifiableObject GetObjectFromPublication(this Engine engine, string publicationTitle, string publicationRelativeUrl)
        {
            var webDavUrl = GetFullWebDavUrl(publicationTitle, publicationRelativeUrl.Replace("webdav", string.Empty).Replace("//", "/"));
            
            return engine.GetObjectByUriOrDefault(webDavUrl);
        }

        private static bool IsPublicationRelativeUrl(string itemUriOrWebDavUrl)
        {
            return itemUriOrWebDavUrl.StartsWith("/") &&
                   !itemUriOrWebDavUrl.StartsWith("/webdav/", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetFullWebDavUrl(string publicationTitle, string publicationRelativeUrl)
        {
            return Uri.UnescapeDataString(String.Concat("/webdav/", publicationTitle, publicationRelativeUrl));
        }

        private static void ThrowExceptionWhenReturnedIdentifiableObjectIsNull(IdentifiableObject item, string itemUriOrWebDavUrl)
        {
            if (item == null)
            {
                throw new InvalidOperationException(String.Format("The specified object {0} could not be found.", itemUriOrWebDavUrl));
            }
        }

        public static Publication GetContextPublication(this Engine engine)
        {
            var item = engine.GetContextItem() as RepositoryLocalObject;

            if (item == null)
            {
                throw new InvalidOperationException("Could not get the Resolved Item from the Publishing Context.");
            }

            return item.ContextRepository as Publication;
        }

        public static IdentifiableObject GetContextItem(this Engine engine)
        {
            return engine.PublishingContext.ResolvedItem.Item;
        }

        public static Publication GetPublication(this Engine engine, string pubTitle)
        {
            var webDavUrl = Uri.UnescapeDataString(String.Concat("/webdav/", pubTitle));
            return engine.GetObject<Publication>(webDavUrl);
        }

        public static bool IsPublishing(this Engine engine)
        {
            return engine.RenderMode == RenderMode.Publish;
        }

        public static bool IsPreview(this Engine engine)
        {
            return engine.RenderMode == RenderMode.PreviewDynamic
                || engine.RenderMode == RenderMode.PreviewStatic;
        }

        public static bool IsStaging(this Engine engine)
        {
            return engine
                .PublishingContext
                .PublicationTarget
                .TargetTypes
                .Any(tt => string.Equals(tt.Title, "Staging", StringComparison.OrdinalIgnoreCase));
        }
    }
}