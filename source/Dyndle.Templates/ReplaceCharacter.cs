using DD4T.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Dyndle.Templates
{
    [TcmTemplateTitle("Replace special character")]
    public class ReplaceCharacter : ITemplate
    {
        /// <summary>
        /// Use this template building block to replace any special character from a entire publication
        /// add a new parameter to your publication metadata name "charactersToReplace"
        /// provide parameter as (replace1=replaceby1)(replace2=replaceby2)...(replaceN=replacebyN)
        /// </summary>

        private Package _package;
        private Engine _engine;
        private Publication _publication;
        private Component _component;

        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(ReplaceCharacter));
        private static readonly string CHARACTERS_TO_REPLACE = "charactersToReplace";


        #region DynamicDeliveryTransformer Members
        public void Transform(Engine engine, Package package)
        {
            _engine = engine;
            _package = package;
            _component = GetComponent();
            _publication = GetPublication(_component.Id);

            LOG.Debug($"Trying to run the module for publication: {_publication.Title}");
            if (GetPublicationMetadataField(_publication) != null)
            {
                try
                {
                    var dictionaryOfCharacterToReplace = DictionaryOfCharactersToReplace(GetPublicationMetadataField(_publication));
                    var compressionEnabled = _package.GetByName("compression-enabled");
                    LOG.Debug($"Compression enabled is {compressionEnabled.ToString().ToLower()}");
                    var outputValue = _package.GetValue("Output");
                    LOG.Debug($"Output is {outputValue}");

                    if (string.IsNullOrEmpty(outputValue))
                        return;

                    if (compressionEnabled != null && compressionEnabled.GetAsString().Equals("yes", StringComparison.OrdinalIgnoreCase))
                    {
                        LOG.Debug($"Compression enabled is {compressionEnabled.GetAsString()}");
                        var decompressedOutput = Compressor.Decompress(outputValue);
                        LOG.Debug($"Decompressed output is {decompressedOutput}");
                        var replacedOutput = ReplaceComponentContent(decompressedOutput, dictionaryOfCharacterToReplace);
                        LOG.Debug($"Replaced output is {replacedOutput}");

                        var jsonOutputAfterReplace = Compressor.Compress(replacedOutput);

                        Item outputItem = _package.GetByName("Output");
                        outputItem.SetAsString(jsonOutputAfterReplace);
                    }
                    else
                    {
                        var replacedOutput = ReplaceComponentContent(outputValue, dictionaryOfCharacterToReplace);
                        LOG.Debug($"Replaced output is {replacedOutput}");

                        Item outputItem = _package.GetByName(Package.OutputName);
                        outputItem.SetAsString(replacedOutput);
                    }
                }

                catch (Exception e)
                {
                    LOG.Debug($"Skipping the character replace. Things went wrong with error message {e.Message}");
                }
            }
            else
            {
                LOG.Debug($"Skipping the Replace because it is not configured for this publication.");
            }
        }
        #endregion

        private Publication GetPublication(TcmUri componentId)
        {
            Publication publication = _engine.GetObject(new TcmUri($"tcm:0-{componentId.PublicationId}-1")) as Publication;
            if (publication == null)
            {
                LOG.Error("no publication found ");
                return null;
            }
            return publication;
        }

        private Component GetComponent()
        {
            Item item = _package.GetByName(Package.ComponentName);
            if (item == null)
            {
                LOG.Error("no component found (is this a page template?)");
                return null;
            }

            Component tcmComponent = _engine.GetObject(item) as Component;

            return tcmComponent;
        }

        private string GetPublicationMetadataField(Publication pub)
        {
            ItemFields pubMetaFields = new ItemFields(pub.Metadata, pub.MetadataSchema);
            LOG.Debug($"Looking in publication metadata for value of field = {CHARACTERS_TO_REPLACE}");
            TextField field = pubMetaFields[CHARACTERS_TO_REPLACE] as TextField;
            LOG.Debug($"Configured pub meta value for field = {field.Value}");
            return field.Value;
        }

        //return the list of dictionary. Dictionary is the "value to replace and key is value to be replaceby"
        //provide parameter as (replace1=replaceby1)(replace2=replaceby2)...(replaceN=replacebyN)
        private Dictionary<string, string> DictionaryOfCharactersToReplace(object characterToReplace)
        {

            Dictionary<string, string> dictOfCharacterToReplace = new Dictionary<string, string>();
            try
            {
                var items = characterToReplace.ToString().Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Split(new[] { '=' }));
                foreach (var item in items)
                {
                    dictOfCharacterToReplace.Add(item[0], item[1]);
                }
            }
            catch (Exception e)
            {
                LOG.Debug($"Cannot generate the dictionary therefore returning empty dictionary with message : {e.Message}");
            }
            return dictOfCharacterToReplace;

        }

        private string ReplaceComponentContent(string componentContent, Dictionary<string, string> dictionaryOfCharactersToReplace)
        {
            var output = new StringBuilder(componentContent);
            try
            {
                LOG.Debug($"Replacing the content of the context component.");

                foreach (var kvp in dictionaryOfCharactersToReplace)
                    output.Replace(kvp.Key, kvp.Value);
            }
            catch (Exception e)
            {
                LOG.Debug($"cannot do replace {e.Message}");
            }

            return output.ToString();
        }
    }
}