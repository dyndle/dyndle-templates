using System;
using System.Collections.Generic;
using System.Linq;

namespace Tridion.ContentManager.ContentManagement.Fields
{
    public static class ItemFieldsExtensions
    {
        public static double? Number(this ItemFields fields, string fieldName)
        {
            return fields.Numbers(fieldName).Cast<double?>().FirstOrDefault();
        }

        public static IEnumerable<double> Numbers(this ItemFields fields, string fieldName)
        {
            return
                null != fields && fields.Contains(fieldName)
                    ? ((NumberField)fields[fieldName]).Values
                    : new double[0];
        }
        
        public static Keyword Keyword(this ItemFields fields, string fieldName)
        {
            return fields.Keywords(fieldName).FirstOrDefault();
        }

        public static IEnumerable<Keyword> Keywords(this ItemFields fields, string fieldName)
        {
            return
                null != fields && fields.Contains(fieldName)
                    ? ((KeywordField)fields[fieldName]).Values
                    : new Keyword[0];
        }

        public static Component Component(this ItemFields fields, string fieldName)
        {
            return fields.Components(fieldName).FirstOrDefault();
        }

        public static IEnumerable<Component> Components(this ItemFields fields, string fieldName)
        {
            return
                null != fields && fields.Contains(fieldName)
                    ? ((ComponentLinkField)fields[fieldName]).Values
                    : new Component[0];
        }

        public static string ExternalLink(this ItemFields fields, string fieldName)
        {
            return fields.ExternalLinks(fieldName).FirstOrDefault() ?? string.Empty;
        }
        
        public static IEnumerable<string> ExternalLinks(this ItemFields fields, string fieldName)
        {
            return
                null != fields && fields.Contains(fieldName)
                    ? ((ExternalLinkField)fields[fieldName]).Values
                    : new string[0];
        }

        public static Component Multimedia(this ItemFields fields, string fieldName)
        {
            return fields.Multimedias(fieldName).FirstOrDefault();
        }

        public static IEnumerable<Component> Multimedias(this ItemFields fields, string fieldName)
        {
            return
                null != fields && fields.Contains(fieldName)
                    ? ((MultimediaLinkField)fields[fieldName]).Values
                    : new Component[0];
        }

        public static ItemFields Embedded(this ItemFields fields, string fieldName)
        {
            return fields.Embeddeds(fieldName).FirstOrDefault();
        }

        public static IEnumerable<ItemFields> Embeddeds(this ItemFields fields, string fieldName)
        {
            return
                null != fields && fields.Contains(fieldName)
                    ? ((EmbeddedSchemaField)fields[fieldName]).Values
                    : new ItemFields[0];
        }

        public static string Text(this ItemFields fields, string fieldName)
        {
            return Texts(fields, fieldName).FirstOrDefault() ?? string.Empty;
        }
        
        public static IEnumerable<string> Texts(this ItemFields fields, string fieldName)
        {
            return
                null != fields && fields.Contains(fieldName)
                    ? ((TextField)fields[fieldName]).Values
                    : new string[0];
        }

        public static DateTime? Date(this ItemFields fields, string fieldName = "date")
        {
            return fields.Dates(fieldName)
              .Cast<DateTime?>()
              .DefaultIfEmpty(default(DateTime?))
              .FirstOrDefault();
        }

        public static IEnumerable<DateTime> Dates(this ItemFields fields, string fieldName = "date")
        {
            return
                null != fields && fields.Contains(fieldName)
                    ? ((DateField)fields[fieldName]).Values
                    : new DateTime[0];
        }

        public static int GetFieldValueCount(this ItemFields fields, string fieldName)
        {
            if (null == fields)
            {
                return 0;
            }

            var field = fields[fieldName];

            return
                field is ComponentLinkField
                    ? ((ComponentLinkField)field).Values.Count
                    : field is TextField
                        ? ((TextField)field).Values.Count
                        : field is EmbeddedSchemaField
                            ? ((EmbeddedSchemaField)field).Values.Count
                            : 0;
        }

        /// <summary>
        /// Manual unification of different field types logic to overcome native tridion implementation shortcoming,
        /// which is not polymorphic.
        /// </summary>
        static readonly IDictionary<Type, Func<ItemFields, string, string>> valueResolver =
            new Dictionary<Type, Func<ItemFields, string, string>> {
                { typeof(KeywordField), (fields, name) => fields.Keywords(name).Select(k => k.Title).FirstOrDefault() },
                { typeof(ComponentLinkField), (fields, name) => fields.Components(name).Select<Component, TcmUri>(c => c.Id).FirstOrDefault() },
                { typeof(ExternalLinkField), (fields, name) => fields.ExternalLinks(name).FirstOrDefault() },
                { typeof(MultimediaLinkField), (fields, name) => fields.Multimedias(name).Select(mc => mc.Title).FirstOrDefault() },
                { typeof(DateField), (fields, name) => fields.Dates(name).FirstOrDefault().ToString("yyyy-MM-dd HH:mm:ss") },
                { typeof(NumberField), (fields, name) => Convert.ToString(fields.Numbers(name).FirstOrDefault()) }
            };

        /// <summary>
        /// Gets a sensible string represntation of a field.
        /// </summary>
        public static string AsText(this ItemFields fields, string fieldName)
        {
            ItemField field;
            Type fieldType;

            return
                null == fields
                || !fields.Contains(fieldName)
                    ? string.Empty
                    : valueResolver.ContainsKey((fieldType = (field = fields[fieldName]).GetType()))
                        ? valueResolver[fieldType](fields, fieldName) ?? string.Empty
                        : field.ToString();
        }

        public static string ToRFC822Date(this ItemField inputDate)
        {
            string formattedDate = inputDate.ToString();
            const string RFC822DateFormat = "yyyyMMddHHmmss";
            var dt = new DateTime();
            if (DateTime.TryParse(formattedDate, out dt))
            {
                formattedDate = dt.ToString(RFC822DateFormat);
            }
            return formattedDate;
        }
    }
}