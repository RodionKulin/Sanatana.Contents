using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unidecode.NET;


namespace Sanatana.Contents.Utilities
{
    public class UrlEncoder : IUrlEncoder
    {
        public string Encode(string input)
        {
            input = input.Unidecode();
            input = Regex.Replace(input, " ", "_");
            input = Uri.EscapeDataString(input);
            return input;
        }
    }
}