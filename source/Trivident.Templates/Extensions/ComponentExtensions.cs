using Tridion.ContentManager.ContentManagement.Fields;

namespace Tridion.ContentManager.ContentManagement
{
    public static class ComponentExtensions
    {
        public static ItemFields Fields(this Component component)
        {
            return new ItemFields(component.Content, component.Schema);
        }

        public static ItemFields MetaFields(this RepositoryLocalObject component)
        {
            return
                null != component.Metadata
                    ? new ItemFields(component.Metadata, component.MetadataSchema)
                    : null != component.MetadataSchema
                        ? new ItemFields(component.MetadataSchema)
                        : default(ItemFields);
        }

        public static void ExtendEmbeddedComponents(this Component component)
        {
        }

        public static ItemFields FieldsOrDefault(this Component component)
        {
            try { return new ItemFields(component.Content, component.Schema); }
            catch { return default(ItemFields); }
        }
    }
}