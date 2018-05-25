using Sanatana.Contents.Html.HtmlNodes;
using System.Collections.Generic;

namespace Sanatana.Contents.Html.Media
{
    public interface IHtmlMediaExtractor
    {
        List<HtmlElement> FindElementsOfType(HtmlElement root, string type);
        List<HtmlImageInfo> FindImages(HtmlElement root);
        void ReplaceMedia(HtmlElement element);
        string ReplaceMedia(string html);
    }
}