using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sanatana.Contents;
using Sanatana.Contents.Html.HtmlNodes;

namespace Sanatana.Contents.Html.Media
{    
    public class HtmlMediaExtractor : IHtmlMediaExtractor
    {        

        //replace media tags
        public string ReplaceMedia(string html)
        {
            HtmlElement doc = HtmlParser.Parse(html);
            ReplaceMedia(doc);
            return doc.ToString();
        }

        public void ReplaceMedia(HtmlElement element)
        {
            for (int i = 0; i < element.ChildNodes.Count; i++)
            {
                if (element.ChildNodes[i] is HtmlElement)
                {
                    HtmlElement innerElement = element.ChildNodes[i] as HtmlElement;

                    if (innerElement.Name == "img")
                    {
                        HtmlAttribute srcAttribute = innerElement.Attributes.FirstOrDefault(p => p.Name == "src");
                        Uri address;

                        if (srcAttribute != null &&
                            !string.IsNullOrEmpty(srcAttribute.Value) &&
                            Uri.TryCreate(srcAttribute.Value, UriKind.RelativeOrAbsolute, out address))
                        {
                            if (!address.PathAndQuery.StartsWith("/scripts/ckeditor/plugins/smiley/images/"))
                            {
                                innerElement.Add("data-original", srcAttribute.Value);
                                innerElement.Add("src", "/images/gimp_5834.png");
                                innerElement.Add("style", "");
                                innerElement.Add("width", "");
                                innerElement.Add("height", "");
                                innerElement.Add("class", "art-img-thumb");
                            }
                        }
                    }
                    else if (innerElement.Name == "iframe" || innerElement.Name == "frame")
                    {
                        element.RemoveChildAt(i);

                        HtmlElement newImage = new HtmlElement("img", true);
                        newImage.Add("src", "/images/youtube-icon.png");
                        newImage.Add("class", "art-img-thumb-yt");
                        element.Insert(i, newImage);
                    }
                    else
                    {
                        ReplaceMedia(innerElement);
                    }
                }
            }
        }


        //find images
        public List<HtmlElement> FindElementsOfType(HtmlElement root, string type)
        {
            List<HtmlElement> images = new List<HtmlElement>();

            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                if (root.ChildNodes[i] is HtmlElement)
                {
                    HtmlElement child = root.ChildNodes[i] as HtmlElement;

                    if (child.Name == type)
                    {
                        images.Add(child);
                    }

                    images.AddRange(FindElementsOfType(child, type));
                }
            }

            return images;
        }

        public List<HtmlImageInfo> FindImages(HtmlElement root)
        {
            return FindElementsOfType(root, "img")
                .Select(p => new HtmlImageInfo(p))
                .ToList();
        }
    }
}