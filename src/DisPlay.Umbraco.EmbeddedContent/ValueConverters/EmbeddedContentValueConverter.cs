namespace DisPlay.Umbraco.EmbeddedContent.ValueConverters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using global::Umbraco.Core;
    using global::Umbraco.Core.Logging;
    using global::Umbraco.Core.Models;
    using global::Umbraco.Core.Models.PublishedContent;
    using global::Umbraco.Core.PropertyEditors;
    using global::Umbraco.Core.Services;
    using global::Umbraco.Web;

    using Models;

    public class EmbeddedContentValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IUserService _userService;
        private readonly IPublishedContentModelFactory _publishedContentModelFactory;
        private readonly ProfilingLogger _profilingLogger;
        private readonly Func<UmbracoContext> _umbracoContextFactory;

        public EmbeddedContentValueConverter(
            IDataTypeService dataTypeService,
            IUserService userService,
            IPublishedContentModelFactory publishedContentModelFactory,
            ProfilingLogger profilingLogger,
            Func<UmbracoContext> umbracoContextFactory)
        {
            _dataTypeService = dataTypeService;
            _userService = userService;
            _publishedContentModelFactory = publishedContentModelFactory;
            _profilingLogger = profilingLogger;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public EmbeddedContentValueConverter() : this(
            ApplicationContext.Current.Services.DataTypeService,
            ApplicationContext.Current.Services.UserService,
            PublishedContentModelFactoryResolver.HasCurrent
                ? PublishedContentModelFactoryResolver.Current.Factory
                : null,
            ApplicationContext.Current.ProfilingLogger,
            () => UmbracoContext.Current)
        {
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            EmbeddedContentConfig config = GetConfig(propertyType.DataTypeId);

            if(config.MaxItems == 1)
            {
                return typeof(IPublishedContent);
            }

            return typeof(IEnumerable<IPublishedContent>);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if(string.IsNullOrEmpty(source?.ToString()))
            {
                return null;
            }

            return NestedContentHelper.ConvertFromNestedContent(JArray.Parse(source.ToString()));
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            EmbeddedContentConfig config = GetConfig(propertyType.DataTypeId);

            using (_profilingLogger.DebugDuration<EmbeddedContentValueConverter>($"ConvertSourceToObject({propertyType.PropertyTypeAlias})"))
            {
                if (source == null)
                {
                    if (config.MaxItems == 1)
                    {
                        return null;
                    }

                    return Enumerable.Empty<IPublishedContent>();
                }
;
                var items = ((JArray)source).ToObject<EmbeddedContentItem[]>();
                var result = new List<IPublishedContent>(items.Length);
                PublishedContentSet<IPublishedContent> contentSet = result.ToContentSet();

                for (var i = 0; i < items.Length; i++)
                {
                    EmbeddedContentItem item = items[i];

                    if (!item.Published)
                    {
                        continue;
                    }

                    if (config.DocumentTypes.FirstOrDefault(x => x.DocumentTypeAlias == item.ContentTypeAlias) == null)
                    {
                        continue;
                    }

                    PublishedContentType contentType = null;
                    try
                    {
                        contentType = PublishedContentType.Get(PublishedItemType.Content, item.ContentTypeAlias);
                    }
                    catch (Exception ex)
                    {
                        _profilingLogger.Logger.Error<EmbeddedContentValueConverter>($"Error getting content type {item.ContentTypeAlias}.", ex);
                    }

                    if (contentType == null)
                    {
                        continue;
                    }

                    IPublishedContent content =
                        new PublishedEmbeddedContent(_userService, item, contentType, contentSet, i, preview);

                    if (_publishedContentModelFactory != null)
                    {
                        content = _publishedContentModelFactory.CreateModel(content);
                    }

                    result.Add(content);
                }

                if (config.MaxItems == 1)
                {
                    return result.FirstOrDefault();
                }

                return contentSet;
            }
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias == EmbeddedContent.Constants.PropertyEditorAlias;
        }

        private EmbeddedContentConfig GetConfig(int dataTypeId)
        {
            using (_profilingLogger.DebugDuration<EmbeddedContentValueConverter>($"GetConfig({dataTypeId})"))
            {
                PreValueCollection preValues = _dataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeId);
                PreValue configPreValue = preValues.PreValuesAsDictionary["embeddedContentConfig"];
                return JsonConvert.DeserializeObject<EmbeddedContentConfig>(configPreValue.Value);
            }
        }
    }
}
