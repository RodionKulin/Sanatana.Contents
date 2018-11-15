using NUnit.Framework;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Files.Resizer;
using Sanatana.Patterns.Pipelines;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using Sanatana.Contents.Files.Services;

namespace Sanatana.ContentsSpecs.Files.Queries
{
    public class ImageSharpResizerSpecs
    {
        [TestFixture]
        public class when_resizing : SpecsFor<ImageSharpResizer>
        {
            ImageSharpResizer _resizer;

            public when_resizing()
            {
                _resizer = new ImageSharpResizer();
            }

            [Test]
            public void then_does_image_processing_using_imagesharp()
            {
                System.Diagnostics.Debugger.Launch();
                System.Diagnostics.Debugger.Break();

                string source = "SampleContent/source.jpg";
                string destination = "SampleContent/destination.jpg";

                byte[] image = File.ReadAllBytes(source);
                PipelineResult<byte[]> result = _resizer.Resize(image
                    , ImageFormat.Png, ImageResizeType.FitAndFill, 1000, 590, false);

                result.Completed.ShouldBeTrue();
                File.WriteAllBytes(destination, result.Data);
            }
        }

    }
}
