using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents
{
    public class YoutubePost<TKey> : Content<TKey>
        where TKey : struct
    {
        public string YoutubeUrl { get; set; }
    }
}
