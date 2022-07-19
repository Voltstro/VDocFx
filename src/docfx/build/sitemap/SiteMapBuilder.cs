using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Docs.Build.sitemap;

internal class SiteMapBuilder
{
    private readonly PublishModel _publishModel;
    private readonly DocumentProvider _documentProvider;

    public SiteMapBuilder(PublishModel publishModel, DocumentProvider documentProvider)
    {
        _publishModel = publishModel;
        _documentProvider = documentProvider;
    }

    public XDocument Build()
    {
        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var root = new XElement(xmlns + "urlset");

        foreach (var publishItem in _publishModel.Files)
        {
            if (publishItem.SourceFile == null)
            {
                continue;
            }

            // Only need pages in the sitemap
            var type = _documentProvider.GetContentType(publishItem.SourceFile);
            if (type != ContentType.Page)
            {
                continue;
            }

            // Hack to remove the menu.json
            if (publishItem.Url == null || publishItem.Url.EndsWith(".json"))
            {
                continue;
            }

            var fullUrl = $"{_publishModel.HostName}{publishItem.Url}";

            var urlElement = new XElement(
                xmlns + "url",
                new XElement(xmlns + "loc", fullUrl));

            // TODO: We should specifically add updated at to publish item
            var updatedAt = publishItem.ExtensionData?["updated_at"];
            if (updatedAt != null)
            {
                // Try to get the page time using the extension data
                if (updatedAt.Type == JTokenType.String)
                {
                    var value = (string)updatedAt!;
                    var time = DateTime.Parse(value);
                    var timeElement = new XElement(
                        xmlns + "lastmod",
                        time.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz"));
                    urlElement.Add(timeElement);
                }
            }

            // We give give priority to index
            if (publishItem.Url == $"{_publishModel.BasePath}/")
            {
                urlElement.Add(new XElement(xmlns + "priority", 1.ToString("F1")));
            }

            root.Add(urlElement);
        }

        return new XDocument(root);
    }
}
