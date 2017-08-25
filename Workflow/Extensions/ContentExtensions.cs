/* https://gist.githubusercontent.com/jbreuer/dde3605035179c34b7287850c45cb8c9/raw/570cbaa30365653dbcf4142e988eba4fc692ecad/ContentExtensions.cs */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Web.Models;

namespace Workflow.Extensions
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Treat unpublished content as IPublishedContent, to allow full property access
        /// </summary>
        /// <param name="content"></param>
        /// <param name="isPreview"></param>
        /// <returns></returns>
        public static IPublishedContent ToPublishedContent(this IContent content, bool isPreview = false)
        {
            return new PublishedContent(content, isPreview);
        }
    }
    /// <summary>
    /// The published content.
    /// </summary>
    public class PublishedContent : PublishedContentWithKeyBase
    {
        private readonly PublishedContentType contentType;
        private readonly IContent inner;
        private readonly bool isPreviewing;
        private readonly Lazy<string> lazyCreatorName;
        private readonly Lazy<string> lazyUrlName;
        private readonly Lazy<string> lazyWriterName;
        private readonly IPublishedProperty[] properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContent"/> class.
        /// </summary>
        /// <param name="inner">
        /// The inner.
        /// </param>
        /// <param name="isPreviewing">
        /// The is previewing.
        /// </param>
        public PublishedContent(IContent inner, bool isPreviewing)
        {
            if (inner == null)
            {
                throw new NullReferenceException("inner");
            }

            this.inner = inner;
            this.isPreviewing = isPreviewing;

            this.lazyUrlName = new Lazy<string>(() => this.inner.GetUrlSegment().ToLower());
            this.lazyCreatorName = new Lazy<string>(() => this.inner.GetCreatorProfile().Name);
            this.lazyWriterName = new Lazy<string>(() => this.inner.GetWriterProfile().Name);

            this.contentType = PublishedContentType.Get(PublishedItemType.Content, this.inner.ContentType.Alias);

            this.properties =
                MapProperties(
                    this.contentType.PropertyTypes,
                    this.inner.Properties,
                    (t, v) => new PublishedProperty(t, v, this.isPreviewing)).ToArray();
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public override int Id
        {
            get
            {
                return this.inner.Id;
            }
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public override Guid Key
        {
            get
            {
                return this.inner.Key;
            }
        }

        /// <summary>
        /// Gets the document type id.
        /// </summary>
        public override int DocumentTypeId
        {
            get
            {
                return this.inner.ContentTypeId;
            }
        }

        /// <summary>
        /// Gets the document type alias.
        /// </summary>
        public override string DocumentTypeAlias
        {
            get
            {
                return this.inner.ContentType.Alias;
            }
        }

        /// <summary>
        /// Gets the item type.
        /// </summary>
        public override PublishedItemType ItemType
        {
            get
            {
                return PublishedItemType.Content;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name
        {
            get
            {
                return this.inner.Name;
            }
        }

        /// <summary>
        /// Gets the level.
        /// </summary>
        public override int Level
        {
            get
            {
                return this.inner.Level;
            }
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        public override string Path
        {
            get
            {
                return this.inner.Path;
            }
        }

        /// <summary>
        /// Gets the sort order.
        /// </summary>
        public override int SortOrder
        {
            get
            {
                return this.inner.SortOrder;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override Guid Version
        {
            get
            {
                return this.inner.Version;
            }
        }

        /// <summary>
        /// Gets the template id.
        /// </summary>
        public override int TemplateId
        {
            get
            {
                return this.inner.Template == null ? 0 : this.inner.Template.Id;
            }
        }

        /// <summary>
        /// Gets the url name.
        /// </summary>
        public override string UrlName
        {
            get
            {
                return this.lazyUrlName.Value;
            }
        }

        /// <summary>
        /// Gets the create date.
        /// </summary>
        public override DateTime CreateDate
        {
            get
            {
                return this.inner.CreateDate;
            }
        }

        /// <summary>
        /// Gets the update date.
        /// </summary>
        public override DateTime UpdateDate
        {
            get
            {
                return this.inner.UpdateDate;
            }
        }

        /// <summary>
        /// Gets the creator id.
        /// </summary>
        public override int CreatorId
        {
            get
            {
                return this.inner.CreatorId;
            }
        }

        /// <summary>
        /// Gets the creator name.
        /// </summary>
        public override string CreatorName
        {
            get
            {
                return this.lazyCreatorName.Value;
            }
        }

        /// <summary>
        /// Gets the writer id.
        /// </summary>
        public override int WriterId
        {
            get
            {
                return this.inner.WriterId;
            }
        }

        /// <summary>
        /// Gets the writer name.
        /// </summary>
        public override string WriterName
        {
            get
            {
                return this.lazyWriterName.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is draft.
        /// </summary>
        public override bool IsDraft
        {
            get
            {
                return this.inner.Published == false;
            }
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        public override IPublishedContent Parent
        {
            get
            {
                var parent = this.inner.Parent();
                return parent.ToPublishedContent(this.isPreviewing);
            }
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        public override IEnumerable<IPublishedContent> Children
        {
            get
            {
                var children = this.inner.Children().ToList();

                return
                    children.Select(x => x.ToPublishedContent(this.isPreviewing))
                        .Where(x => x != null)
                        .OrderBy(x => x.SortOrder);
            }
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public override ICollection<IPublishedProperty> Properties
        {
            get
            {
                return this.properties;
            }
        }

        /// <summary>
        /// Gets the content type.
        /// </summary>
        public override PublishedContentType ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        /// <summary>
        /// The get property.
        /// </summary>
        /// <param name="alias">
        /// The alias.
        /// </param>
        /// <returns>
        /// The <see cref="IPublishedProperty"/>.
        /// </returns>
        public override IPublishedProperty GetProperty(string alias)
        {
            return this.properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
        }

        /// <summary>
        /// The map properties.
        /// </summary>
        /// <param name="propertyTypes">
        /// The property types.
        /// </param>
        /// <param name="properties">
        /// The properties.
        /// </param>
        /// <param name="map">
        /// The map.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{T}"/>.
        /// </returns>
        internal static IEnumerable<IPublishedProperty> MapProperties(
            IEnumerable<PublishedPropertyType> propertyTypes,
            IEnumerable<Property> properties,
            Func<PublishedPropertyType, object, IPublishedProperty> map)
        {
            var propertyEditorResolver = PropertyEditorResolver.Current;
            var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

            return propertyTypes.Select(
                x =>
                {
                    var p = properties.SingleOrDefault(xx => xx.Alias == x.PropertyTypeAlias);
                    var v = p == null || p.Value == null ? null : p.Value;
                    if (v != null)
                    {
                        var e = propertyEditorResolver.GetByAlias(x.PropertyEditorAlias);

                            // We are converting to string, even for database values which are integer or
                            // DateTime, which is not optimum. Doing differently would require that we have a way to tell
                            // whether the conversion to XML string changes something or not... which we don't, and we
                            // don't want to implement it as PropertyValueEditor.ConvertDbToXml/String should die anyway.

                            // Don't think about improving the situation here: this is a corner case and the real
                            // thing to do is to get rig of PropertyValueEditor.ConvertDbToXml/String.

                            // Use ConvertDbToString to keep it simple, although everywhere we use ConvertDbToXml and
                            // nothing ensures that the two methods are consistent.
                            if (e != null)
                        {
                            v = e.ValueEditor.ConvertDbToString(p, p.PropertyType, dataTypeService);
                        }
                    }

                    return map(x, v);
                });
        }
    }

    internal static class ContentBaseExtensions
    {
        /// <summary>
        /// Gets the url segment providers.
        /// </summary>
        /// <remarks>This is so that unit tests that do not initialize the resolver do not
        /// fail and fall back to defaults. When running the whole Umbraco, CoreBootManager
        /// does initialise the resolver.</remarks>
        private static IEnumerable<IUrlSegmentProvider> UrlSegmentProviders
        {
            get
            {
                return UrlSegmentProviderResolver.HasCurrent
                           ? UrlSegmentProviderResolver.Current.Providers
                           : new IUrlSegmentProvider[] { new DefaultUrlSegmentProvider() };
            }
        }

        /// <summary>
        /// Gets the default url segment for a specified content.
        /// </summary>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <returns>
        /// The url segment.
        /// </returns>
        public static string GetUrlSegment(this IContentBase content)
        {
            var url = UrlSegmentProviders.Select(p => p.GetUrlSegment(content)).First(u => u != null);
            url = url ?? new DefaultUrlSegmentProvider().GetUrlSegment(content); // be safe
            return url;
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    internal class PublishedProperty : PublishedPropertyBase
    {
        private readonly object dataValue;

        private readonly bool isPreviewing;

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
        public PublishedProperty(PublishedPropertyType propertyType, object dataValue, bool isPreviewing)
            : base(propertyType)
        {
            this.dataValue = dataValue;
            this.isPreviewing = isPreviewing;
        }

        /// <summary>
        /// Gets a value indicating whether has value.
        /// </summary>
        public override bool HasValue
        {
            get
            {
                return this.dataValue != null
                       && ((this.dataValue is string) == false
                           || string.IsNullOrWhiteSpace((string)this.dataValue) == false);
            }
        }

        /// <summary>
        /// Gets the data value.
        /// </summary>
        public override object DataValue
        {
            get
            {
                return this.dataValue;
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public override object Value
        {
            get
            {
                var source = this.PropertyType.ConvertDataToSource(this.dataValue, this.isPreviewing);
                return this.PropertyType.ConvertSourceToObject(source, this.isPreviewing);
            }
        }

        /// <summary>
        /// Gets the x path value.
        /// </summary>
        public override object XPathValue
        {
            get
            {
                var source = this.PropertyType.ConvertDataToSource(this.dataValue, this.isPreviewing);
                return this.PropertyType.ConvertSourceToXPath(source, this.isPreviewing);
            }
        }
    }

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
            if (propertyType == null)
            {
                throw new ArgumentNullException("propertyType");
            }

            this.PropertyType = propertyType;
        }

        /// <summary>
        /// Gets the property type alias.
        /// </summary>
        public string PropertyTypeAlias
        {
            get
            {
                return this.PropertyType.PropertyTypeAlias;
            }
        }

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

