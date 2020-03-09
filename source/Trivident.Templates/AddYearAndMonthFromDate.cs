using System;
using System.Text.RegularExpressions;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Trivident.Templates
{
    /// <summary>
    /// Creates year and month fields for SI4T which contains the year and month of the date field.
    /// The name of the date field is configurable with a parameter
    /// This TBB should be placed between the Generate Index Data and the Add Index Data to Output TBBs
    /// </summary>
    [TcmTemplateTitle("Add year and month from date to SI4T")]
    [TcmTemplateParameterSchema("resource:Trivident.Templates.Resources.AddTopLevelDirectory Parameters.xsd")]
    public class AddYearAndMonthFromDate : ITemplate
    {
        Engine Engine { get; set; }
        Package Package { get; set; }
        private readonly TemplatingLogger logger;
        private static readonly string PACKAGE_VARIABLE_NAME = "SI4T.Templating.SearchData";

        public AddYearAndMonthFromDate()
        {
            logger = TemplatingLogger.GetLogger(this.GetType());
        }

        public void Transform(Engine engine, Package package)
        {
            Engine = engine;
            Package = package;

            if (IsComponentTemplate)
            {
                logger.Debug("This is a component template");
                Component component = GetComponent();
                string dateFld = component.Fields().Text(FieldName);

                if (string.IsNullOrEmpty(dateFld))
                {
                    logger.Debug("no date field found, stopping now");
                    return;
                }

                DateTime date;
                DateTime.TryParse(dateFld, out date);

                var year = date.Year;
                var month = date.Month;

                var searchData = Package.GetStringItemValue(PACKAGE_VARIABLE_NAME);

                Regex re = new Regex("^(?<firstPart>.*)</custom>(?<lastPart>.*)$");
                Match match = re.Match(searchData);
                if (!match.Success)
                {
                    logger.Debug("cannot find SI4T data in output, stopping now");
                    return;
                }

                var firstPart = match.Groups["firstPart"].Value;
                var lastPart = match.Groups["lastPart"].Value;
                var newYearValue = string.Join("", "<" + FieldName + "Year>" + year + "</" + FieldName + "Year>");
                var newMonthValue = string.Join("", "<" + FieldName + "Month>" + month + "</" + FieldName + "Month>");
                searchData = firstPart + newYearValue + newMonthValue + "</custom>" + lastPart;
                logger.Debug("new search data: " + searchData);
                Package.RemoveItem(PACKAGE_VARIABLE_NAME);
                Package.PushItem(PACKAGE_VARIABLE_NAME, Package.CreateStringItem(ContentType.Xml, searchData));
            }

        }

        protected bool IsComponentTemplate
        {
            get
            {
                return Engine.PublishingContext.ResolvedItem.Item is Component;
            }
        }

        protected Component GetComponent()
        {
            Item component = Package.GetByName("Component");
            return (Component)Engine.GetObject(component.GetAsSource().GetValue("ID"));
        }

        private string FieldName
        {
            get
            {
                if (Package == null)
                {
                    return "FieldNamePackageNull";
                }
                if (Package.GetByName("FieldName") == null)
                {
                    return "FieldNameNotFound";
                }
                return Package.GetByName("FieldName").GetAsString();
            }
        }
    }
}
