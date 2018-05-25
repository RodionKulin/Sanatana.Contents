using Sanatana.Patterns.Pipelines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Sanatana.Contents.Files.Resizer
{
    public interface IImageResizer
    {
        PipelineResult<byte[]> Resize(byte[] inputBytes, ImageFormat targetFormat, ImageResizeType resizeType
            , int width, int height, bool roundCorners);


    }
}