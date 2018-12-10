using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Workflow.Extensions;

namespace Workflow.Models.Content
{
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
            this.inner = inner ?? throw new NullReferenceException("inner");

            IUserService userService = ApplicationContext.Current.Services.UserService;
            this.isPreviewing = isPreviewing;

            lazyUrlName = new Lazy<string>(() => this.inner.GetUrlSegment().ToLower());
            lazyCreatorName = new Lazy<string>(() => this.inner.GetCreatorProfile(userService).Name);
            lazyWriterName = new Lazy<string>(() => this.inner.GetWriterProfile(userService).Name);

            contentType = PublishedContentType.Get(PublishedItemType.Content, this.inner.ContentType.Alias);

            properties =
                MapProperties(
                    contentType.PropertyTypes,
                    this.inner.Properties,
                    (t, v) => new PublishedProperty(t, v, this.isPreviewing)).ToArray();
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public override int Id => inner.Id;

        /// <summary>
        /// Gets the key.
        /// </summary>
        public override Guid Key => inner.Key;

        /// <summary>
        /// Gets the document type id.
        /// </summary>
        public override int DocumentTypeId => inner.ContentTypeId;

        /// <summary>
        /// Gets the document type alias.
        /// </summary>
        public override string DocumentTypeAlias => inner.ContentType.Alias;

        /// <summary>
        /// Gets the item type.
        /// </summary>
        public override PublishedItemType ItemType => PublishedItemType.Content;

        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name => inner.Name;

        /// <summary>
        /// Gets the level.
        /// </summary>
        public override int Level => inner.Level;

        /// <summary>
        /// Gets the path.
        /// </summary>
        public override string Path => inner.Path;

        /// <summary>
        /// Gets the sort order.
        /// </summary>
        public override int SortOrder => inner.SortOrder;

        /// <summary>
        /// Gets the version.
        /// </summary>
        public override Guid Version => inner.Version;

        /// <summary>
        /// Gets the template id.
        /// </summary>
        public override int TemplateId => inner.Template?.Id ?? 0;

        /// <summary>
        /// Gets the url name.
        /// </summary>
        public override string UrlName => lazyUrlName.Value;

        /// <summary>
        /// Gets the create date.
        /// </summary>
        public override DateTime CreateDate => inner.CreateDate;

        /// <summary>
        /// Gets the update date.
        /// </summary>
        public override DateTime UpdateDate => inner.UpdateDate;

        /// <summary>
        /// Gets the creator id.
        /// </summary>
        public override int CreatorId => inner.CreatorId;

        /// <summary>
        /// Gets the creator name.
        /// </summary>
        public override string CreatorName => lazyCreatorName.Value;

        /// <summary>
        /// Gets the writer id.
        /// </summary>
        public override int WriterId => inner.WriterId;

        /// <summary>
        /// Gets the writer name.
        /// </summary>
        public override string WriterName => lazyWriterName.Value;

        /// <summary>
        /// Gets a value indicating whether is draft.
        /// </summary>
        public override bool IsDraft => inner.Published == false;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        public override IPublishedContent Parent
        {
            get
            {
                IContent parent = inner.Parent();
                return parent.ToPublishedContent(isPreviewing);
            }
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        public override IEnumerable<IPublishedContent> Children
        {
            get
            {
                List<IContent> children = inner.Children().ToList();

                return
                    children.Select(x => x.ToPublishedContent(isPreviewing))
                        .Where(x => x != null)
                        .OrderBy(x => x.SortOrder);
            }
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public override ICollection<IPublishedProperty> Properties => properties;

        /// <summary>
        /// Gets the content type.
        /// </summary>
        public override PublishedContentType ContentType => contentType;

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
            return properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
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
            PropertyEditorResolver propertyEditorResolver = PropertyEditorResolver.Current;
            IDataTypeService dataTypeService = ApplicationContext.Current.Services.DataTypeService;

            return propertyTypes.Select(
                x =>
                {
                    Property p = properties.SingleOrDefault(xx => xx.Alias == x.PropertyTypeAlias);
                    object v = p?.Value;

                    if (v == null) return map(x, v);

                    PropertyEditor e = propertyEditorResolver.GetByAlias(x.PropertyEditorAlias);

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

                    return map(x, v);
                });
        }
    }
}
