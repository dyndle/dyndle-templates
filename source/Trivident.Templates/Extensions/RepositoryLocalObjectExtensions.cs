using System.Collections.Generic;

namespace Tridion.ContentManager.ContentManagement
{
  public static class RepositoryLocalObjectExtensions
  {
    public static IEnumerable<OrganizationalItem> Parents(this RepositoryLocalObject page)
    {
      var sg = page.OrganizationalItem;

      while (null != sg.OrganizationalItem)
      {
        yield return sg;
        sg = sg.OrganizationalItem;
      }
    }
  }
}