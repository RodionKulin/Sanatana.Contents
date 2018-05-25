using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Selectors.Contents
{
    public class ContinuationContentPageVM<TKey, TContent>
        where TKey : struct
        where TContent : Content<TKey>
    {
        //properties
        public List<TContent> Contents { get; set; }
        public string LastPublishTimeUtcIso8601 { get; set; }
        public bool CanContinue { get; set; }


        //init
        public ContinuationContentPageVM()
        {

        }
    }
}
