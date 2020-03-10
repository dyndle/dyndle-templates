using System.Security.AccessControl;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Dyndle.Templates
{
    /// <summary>
    /// Set the LinkLevels value 
    /// </summary>
    [TcmTemplateTitle("Boost LinkLevel")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.BoostLinkLevel.xsd")]
    public class SetLinkLevels : ITemplate
    {
        private readonly TemplatingLogger _logger;
        private const string ParameterName = "CustomLinkLevels";
        private const string FieldName = "LinkLevels";

        public SetLinkLevels()
        {
            _logger = TemplatingLogger.GetLogger(this.GetType());
        }

        public void Transform(Engine engine, Package package)
        {
            _logger.Debug("Set Link Levels");
            
            var customLevel = package.GetValue(ParameterName);
            var linkLevel = package.GetValue(FieldName);

            if (customLevel == null)
            {
                _logger.Debug("custom link levels is not set, don't do anything");
                return;
            }

            if (linkLevel != null)
            {
                _logger.Debug("link levels is already set, don't do anything");
                return;
            }

            _logger.Debug($"link levels is not yet set, so add it to the package with value {customLevel}");
            var item = package.CreateStringItem(ContentType.Text, customLevel);
            package.PushItem(FieldName, item);
        }
    }
}
