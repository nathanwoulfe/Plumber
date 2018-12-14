// ReSharper disable All 

using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Workflow.Models.Content
{
    internal abstract class PublishedPropertyBase : IPublishedProperty
    {
        /// <summary>
        /// The property type.
        /// </summary>
        public readonly PublishedPropertyType PropertyType;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedPropertyBase"/> class.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        protected PublishedPropertyBase(PublishedPropertyType propertyType)
        {
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
        }

        /// <summary>
        /// Gets the property type alias.
        /// </summary>
        public string PropertyTypeAlias => PropertyType.PropertyTypeAlias;

        /// <summary>
        /// Gets a value indicating whether has value.
        /// </summary>
        public abstract bool HasValue { get; }

        /// <summary>
        /// Gets the data value.
        /// </summary>
        public abstract object DataValue { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Gets the x path value.
        /// </summary>
        public abstract object XPathValue { get; }
    }
}
