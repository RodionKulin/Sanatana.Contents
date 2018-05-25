using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sanatana.Contents.Files.Downloads
{
    public class FileStream
    {
        //properties
        public Stream Stream { get; set; }
        public long ContentLength { get; set; }


        //init
        public FileStream()
        {

        }
        public FileStream(Stream stream)
        {
            Stream = stream;
            ContentLength = stream.Length;
        }
    }
}
