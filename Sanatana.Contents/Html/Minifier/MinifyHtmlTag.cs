using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Html.Minifier
{
    public class MinifyHtmlTag
    {
        public string Name { get; private set; }
        public MinifyTagType MinifyTagType { get; private set; }


        //init
        public MinifyHtmlTag(string input)
        {
            input = input.Trim(new char[] { '<', '>', ' ' });

            if (input.StartsWith("/"))
                MinifyTagType = MinifyTagType.Closing;
            else if (input.EndsWith("/"))
                MinifyTagType = MinifyTagType.Single;
            else
                MinifyTagType = MinifyTagType.Opening;

            string[] tagParts = input.Trim(new char[] { '/', ' ' }).Split(new char[] { ' ' });
            if (tagParts.Length > 0)
                Name = tagParts[0];
        }

        //methods
        public string CreateClosingTag()
        {
            if (MinifyTagType == MinifyTagType.Opening && !string.IsNullOrEmpty(Name))
            {
                return string.Format("</{0}>", Name);
            }
            else
            {
                return null;
            }
        }
    }
}
