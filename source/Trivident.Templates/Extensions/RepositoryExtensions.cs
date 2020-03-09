using System.Collections.Generic;
using System.Linq;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement.Fields;

namespace Tridion.ContentManager.ContentManagement
{
  public static class RepositoryExtensions
  {
    public static ItemFields MetaFields(this Repository publication)
    {
      return
          null != publication.Metadata
              ? new ItemFields(publication.Metadata, publication.MetadataSchema)
              : null != publication.MetadataSchema
                  ? new ItemFields(publication.MetadataSchema)
                  : default(ItemFields);
    }

    /// <summary>
    /// Returns the root structure group for the specified publication
    /// </summary>		
    /// <returns>The Root Structure Group in the publication</returns>
    /// <remarks>copied and modified code from Repository.RootFolder :)</remarks>
    public static StructureGroup GetRootSG(this Repository publication)
    {
      return
        publication.GetItems(
          new RepositoryItemsFilter(publication.Session)
          {
            ItemTypes = new[] { ItemType.StructureGroup }
          }
        ).Cast<StructureGroup>().FirstOrDefault();
    }
  }
}