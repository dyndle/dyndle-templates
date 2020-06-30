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
    /// <summary>
    /// Use this template building block to replace any special character from an entire publication.
    /// provide parameter as (replace1=replaceby1)(replace2=replaceby2)...(replaceN=replacebyN)
    /// </summary>
    [TcmTemplateTitle("Replace special character")]
    [TcmTemplateParameterSchema("resource:Dyndle.Templates.Resources.ReplaceCharacter Parameters.xsd")]
    public class ReplaceCharacter : ITemplate
    {
        private Package _package;
        private readonly TemplatingLogger LOG = TemplatingLogger.GetLogger(typeof(ReplaceCharacter));

        #region DynamicDeliveryTransformer Members
        public void Transform(Engine engine, Package package)
        {
            _package = package;

            if (_package.GetByName("CharactersToReplace") != null)
            {
                try
                {
                    var dictionaryOfCharacterToReplace = DictionaryOfCharactersToReplace(_package.GetByName("CharactersToReplace").GetAsString());
                    var compressionEnabled = _package.GetByName("CompressionEnabled");
                    LOG.Debug($"Compression enabled is {compressionEnabled?.ToString().ToLower()}");
                    var outputValue = _package.GetValue("Output");
                    LOG.Debug($"Output is {outputValue}");

                    if (string.IsNullOrEmpty(outputValue))
                        return;

                    if (compressionEnabled != null && compressionEnabled.GetAsString().Equals("true", StringComparison.OrdinalIgnoreCase))
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
                LOG.Debug($"Skipping the Replace because there were no characters to replace provided in paramaters.");
            }
        }
        #endregion

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