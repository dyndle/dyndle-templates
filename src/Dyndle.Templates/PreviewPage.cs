using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.CommunicationManagement;
using Dynamic = DD4T.ContentModel;
using DD4T.Templates;
using System.Net;
using System.IO;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Publishing;
using DD4T.Templates.Base.Utils;
using Tcm = Tridion.ContentManager;
using System.Net.Http;

namespace Dyndle.Templates
{
    /// <summary>
    /// Preview the page in the Content Manager Explorer
    /// </summary>
    [TcmTemplateTitle("Preview page")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.Preview Parameters.xsd")]
    public class PreviewPage : ITemplate
    {
        protected TemplatingLogger _log = TemplatingLogger.GetLogger(typeof(PreviewPage));
        protected Package _package;

        public void Transform(Engine engine, Package package)
        {

            // do NOT execute this logic when we are actually publishing! (similair for fast track publishing)
            if (engine.RenderMode == RenderMode.Publish || (engine.PublishingContext.PublicationTarget != null && !Tcm.TcmUri.IsNullOrUriNull(engine.PublishingContext.PublicationTarget.Id)))
            {
                return;
            }

            _package = package;

            Item outputItem = _package.GetByName("Output");
            String inputValue = _package.GetValue("Output");

            if (string.IsNullOrEmpty(inputValue))
            {
                _log.Warning("Could not find 'Output' in the package, nothing to preview");
                return;
            }


            string outputValue = HttpPost(StagingUrl, inputValue);
            if (string.IsNullOrEmpty(outputValue))
            {
                outputValue = "<h2>There was an error while generating the preview.</h2>";
            }

            // replace the Output item in the package
            _package.Remove(outputItem);
            _package.PushItem("Output", package.CreateStringItem(ContentType.Html, outputValue));
        }


        string HttpPost(string uri, string json)
        {
            _log.Debug("About to post to " + uri);
            var values = new Dictionary<string, string>
            {
                { "data", json }
            };

            var content = new FormUrlEncodedContent(values);

            using (var client = new HttpClient())
            {
                var response = client.PostAsync(uri, content).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var html = response.Content.ReadAsStringAsync().Result;
                    _log.Debug("status 200, found html of length " + html.Length);
                    return html;
                }
                _log.Debug($"status {response.StatusCode}, returning null");
                return null;
            }
        }

        private string StagingUrl
        {
            get
            {
                Item stagingUrlItem = _package.GetByName("StagingUrl");
                if (stagingUrlItem == null)
                    return null;
                var url = stagingUrlItem.GetAsString();
                url = url.TrimEnd(new[] { '/' });
                if (url.EndsWith("/cme/preview"))
                {
                    return url;
                }
                return url + "/cme/preview";
            }
        }

        private string ProxyUrl
        {
            get
            {
                Item proxyUrlItem = _package.GetByName("ProxyUrl");
                if (proxyUrlItem == null)
                    return null;
                return proxyUrlItem.GetAsString();
            }
        }
    }
}
