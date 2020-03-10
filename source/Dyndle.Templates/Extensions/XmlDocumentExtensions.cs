namespace System.Xml
{
    using Linq;
    
    public static class XmlDocumentExtensions
    {
        public static XElement ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XElement.Load(nodeReader);
            }
        }
    }

    namespace Linq
    {
        public static class XElementExtensions
        {
            public static XmlDocument ToXmlDocument(this XElement xDocument)
            {
                var xmlDocument = new XmlDocument();
                using (var xmlReader = xDocument.CreateReader())
                {
                    xmlDocument.Load(xmlReader);
                }
                return xmlDocument;
            }

            public static XElement ToXElement(this XmlElement element)
            {
                return XElement.Load(element.CreateNavigator().ReadSubtree());
            }

            public static XmlElement ToXmlElement(this XElement element)
            {
                XmlDocument doc = new XmlDocument();
                return doc.ReadNode(element.CreateReader()) as XmlElement;
            }
        }
    }
}