// ReSharper disable All 

using System;
using System.Xml.Serialization;
using Umbraco.Core.Models.PublishedContent;

namespace Workflow.Models.Content
{
    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    internal class PublishedProperty : PublishedPropertyBase
    {
        private readonly object _dataValue;
        private readonly bool _isPreviewing;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedProperty"/> class.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <param name="dataValue">
        /// The data value.
        /// </param>
        /// <param name="isPreviewing">
        /// The is previewing.
        /// </param>
        public PublishedProperty(PublishedPropertyType propertyType, object dataValue, bool isPreviewing) : base(propertyType)
        {
            _dataValue = dataValue;
            _isPreviewing = isPreviewing;
        }

        /// <summary>
        /// Gets a value indicating whether has value.
        /// </summary>
        public override bool HasValue => _dataValue != null
                                         && ((_dataValue is string) == false
                                             || string.IsNullOrWhiteSpace((string)_dataValue) == false);

        /// <summary>
        /// Gets the data value.
        /// </summary>
        public override object DataValue => _dataValue;

        /// <summary>
        /// Gets the value.
        /// </summary>
        public override object Value
        {
            get
            {
                object source = PropertyType.ConvertDataToSource(_dataValue, _isPreviewing);

                return PropertyType.ConvertSourceToObject(source, _isPreviewing);
            }
        }

        /// <summary>
        /// Gets the x path value.
        /// </summary>
        public override object XPathValue
        {
            get
            {
                object source = PropertyType.ConvertDataToSource(_dataValue, _isPreviewing);
                return PropertyType.ConvertSourceToXPath(source, _isPreviewing);
            }
        }
    }
}
