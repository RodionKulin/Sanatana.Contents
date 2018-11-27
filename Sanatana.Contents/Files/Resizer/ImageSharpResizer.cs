using System;
using System.Collections.Generic;
using System.Text;
using Sanatana.Patterns.Pipelines;
using Sanatana.Contents.Resources;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Sanatana.Contents.Files.Resizer
{
    public class ImageSharpResizer : IImageResizer
    {
        //methods
        public virtual PipelineResult<byte[]> Resize(byte[] imageBytes, ImageFormat targetFormat
            , ImageResizeType resizeType, int width, int height, bool roundCorners)
        {
            byte[] outputBytes = null;

            
            using (Image<Rgba32> image = Image.Load(imageBytes))
            {
                ImageNewSizes sizes = new ImageNewSizes(image.Width, image.Height);
                if (sizes.IsEmpty)
                {
                    return PipelineResult<byte[]>.Error(ContentsMessages.Image_FormatException);
                }

                PickResizeType(resizeType, width, height, sizes);
                if (sizes.IsEmpty)
                {
                    return PipelineResult<byte[]>.Error(ContentsMessages.Image_FormatException);
                }

                outputBytes = Save(image, sizes, targetFormat, resizeType, roundCorners);
             
            }

            return PipelineResult<byte[]>.Success(outputBytes);
        }

        protected virtual void PickResizeType(ImageResizeType resizeType
            , int width, int height, ImageNewSizes sizes)
        {
            if (resizeType == ImageResizeType.FitRatio)
            {
                LowerAllProportionally(width, height, sizes);
                RaiseToLowestProportionally(width, height, sizes);
            }
            else if (resizeType == ImageResizeType.FitAndFill)
            {
                LowerAllProportionally(width, height, sizes);
                RaiseToLowestProportionally(width, height, sizes);
                AddSpaceOnBorders(width, height, sizes);
            }
            else if (resizeType == ImageResizeType.FitHeightAndCropWidth)
            {
                LowerAllProportionally(width, height, sizes);
                RaiseHeightProportionally(height, sizes);
                CutAll(width, height, sizes);
            }
            else if (resizeType == ImageResizeType.FitWidthAndCropHeight)
            {
                LowerAllProportionally(width, height, sizes);
                RaiseWidthProportionally(width, sizes);
                CutAll(width, height, sizes);
            }
            else if (resizeType == ImageResizeType.FitWidthAndSqueezeHeight)
            {
                LowerAllProportionally(width, height, sizes);
                RaiseWidthProportionally(width, sizes);
                FillHeightIfBigger(height, sizes);
            }

            if (sizes.Padding.IsEmpty)
            {
                sizes.Padding = sizes.Size;
            }
        }

        protected virtual byte[] Save(Image<Rgba32> image
            , ImageNewSizes sizes, ImageFormat targetFormat, ImageResizeType resizeType, bool roundCorners)
        {
            if(resizeType != ImageResizeType.None)
            {
                image.Mutate(x => x
                    .Crop(sizes.CropRegion)
                    .Resize(new ResizeOptions
                    {
                        Size = sizes.Size,
                        Mode = ResizeMode.Stretch
                    })
                    .Pad(sizes.Padding.Width, sizes.Padding.Height)
                );
            }

            byte[] outputBytes = null;
            using (MemoryStream outputStream = new MemoryStream())
            {
                if (targetFormat == ImageFormat.Gif)
                    image.SaveAsGif(outputStream);
                else if (targetFormat == ImageFormat.Jpeg)
                    image.SaveAsJpeg(outputStream);
                else if (targetFormat == ImageFormat.Png)
                    image.SaveAsPng(outputStream);
                else if (targetFormat == ImageFormat.Bmp)
                    image.SaveAsBmp(outputStream);
                outputBytes = outputStream.ToArray();
            }

            return outputBytes;
        }


        //size changing
        /// <summary>
        /// Shrink width and height to maximum values if it's bigger, saving image propotions
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="sizes.Width"></param>
        /// <param name="новаяВысота"></param>
        protected virtual void LowerAllProportionally(int maxWidth, int maxHeight, ImageNewSizes sizes)
        {
            // if need to shrink
            // then make width and height lower or equal to max values
            if (sizes.Size.Width > maxWidth || sizes.Size.Height > maxHeight)
            {
                int newWidth = sizes.Width;
                int newHeight = sizes.Height;

                if (newHeight > maxHeight)
                {
                    newWidth = (int)(newWidth * ((double)maxHeight / newHeight));
                    newHeight = maxHeight;
                }
                if (newWidth > maxWidth)
                {
                    newHeight = (int)(newHeight * ((double)maxWidth / newWidth));
                    newWidth = maxWidth;
                }

                sizes.Size = new Size(newWidth, newHeight);
                sizes.Width = newWidth;
                sizes.Height = newHeight;
            }
        }

        /// <summary>
        /// Raise width and height to minimum values, saving image propotions
        /// </summary>
        /// <param name="minWidth"></param>
        /// <param name="minHeight"></param>
        /// <param name="sizes.Width"></param>
        /// <param name="sizes.Height"></param>
        protected virtual void RaiseToLowestProportionally(int minWidth, int minHeight, ImageNewSizes sizes)
        {
            // if need to raise
            // then set height and width equal or greater to min values
            if (sizes.Width < minWidth && sizes.Height < minHeight)
            {
                double widthCoefficient = (double)minWidth / sizes.Width;
                double heightCoefficient = (double)minHeight / sizes.Height;
                int newWidth = sizes.Width;
                int newHeight = sizes.Height;

                if (heightCoefficient < widthCoefficient)
                {
                    newWidth = (int)(newWidth * ((double)minHeight / newHeight));
                    newHeight = minHeight;
                }
                else
                {
                    newHeight = (int)(newHeight * ((double)minWidth / newWidth));
                    newWidth = minWidth;
                }

                sizes.Size = new Size(newWidth, newHeight);
                sizes.Width = newWidth;
                sizes.Height = newHeight;
            }
        }

        /// <summary>
        /// Raise width to minimum value, saving image propotions
        /// </summary>
        /// <param name="minWidth"></param>
        /// <param name="sizes.Width"></param>
        /// <param name="sizes.Height"></param>
        protected virtual void RaiseWidthProportionally(int minWidth, ImageNewSizes sizes)
        {
            if (sizes.Width < minWidth)
            {
                int newHeight = (int)(sizes.Height * ((double)minWidth / sizes.Width));
                int newWidth = minWidth;

                sizes.Size = new Size(newWidth, newHeight);
                sizes.Width = newWidth;
                sizes.Height = newHeight;
            }
        }

        /// <summary>
        /// Raise height to minimum value, saving image propotions
        /// </summary>
        /// <param name="minWidth"></param>
        /// <param name="sizes.Width"></param>
        /// <param name="sizes.Height"></param>
        protected virtual void RaiseHeightProportionally(int minHeight, ImageNewSizes sizes)
        {
            if (sizes.Height < minHeight)
            {
                int newWidth = (int)(sizes.Width * ((double)minHeight / sizes.Height));
                int newHeight = minHeight;

                sizes.Size = new Size(newWidth, newHeight);
                sizes.Width = newWidth;
                sizes.Height = newHeight;
            }
        }

        /// <summary>
        /// Fill required width and height with white space
        /// </summary>
        /// <param name="minWidth"></param>
        /// <param name="minHeight"></param>
        /// <param name="sizes"></param>
        protected virtual void AddSpaceOnBorders(int width, int height, ImageNewSizes sizes)
        {
            if (sizes.Width < width)
            {
                sizes.Padding = new Size(width, sizes.Padding.Height);
                sizes.Width = width;
            }
            if (sizes.Height < height)
            {
                sizes.Padding = new Size(sizes.Padding.Width, height);
                sizes.Height = height;
            }
        }

        /// <summary>
        /// Cut extra height on image edges
        /// </summary>
        /// <param name="maxHeight"></param>
        /// <param name="sizes"></param>
        protected virtual void CutHeight(int maxHeight, ImageNewSizes sizes)
        {
            if (sizes.Height > maxHeight)
            {
                int extraHeight = sizes.Height - maxHeight;
                double diffCoefficient = (double)sizes.Size.Height / sizes.CropRegion.Height;
                int extraHeightHalfOriginal = (int)(extraHeight / diffCoefficient) / 2;

                sizes.CropRegion = new Rectangle(
                    sizes.CropRegion.X,
                    sizes.CropRegion.Y + extraHeightHalfOriginal,
                    sizes.CropRegion.Width,
                    sizes.CropRegion.Height - extraHeightHalfOriginal);

                sizes.Size = new Size(sizes.Size.Width, maxHeight);
                sizes.Height = maxHeight;
            }
        }

        /// <summary>
        /// Cut extra width and height on image edges
        /// </summary>
        protected virtual void CutAll(int width, int height, ImageNewSizes sizes)
        {
            if (sizes.Height > height)
            {
                int extraHeight = sizes.Height - height;
                double diffCoefficient = (double)sizes.Size.Height / sizes.CropRegion.Height;
                int extraHeightHalfOriginal = (int)(extraHeight / diffCoefficient) / 2;

                sizes.Size = new Size(sizes.Size.Width, sizes.Size.Height - extraHeight);

                sizes.CropRegion = new Rectangle(
                    sizes.CropRegion.X,
                    sizes.CropRegion.Y + extraHeightHalfOriginal,
                    sizes.CropRegion.Width,
                    sizes.CropRegion.Height - extraHeightHalfOriginal * 2);

                sizes.Height = height;
            }
            if (sizes.Width > width)
            {
                int extraWidth = sizes.Width - width;
                double diffCoefficient = (double)sizes.Size.Width / sizes.CropRegion.Width;
                int extraWidthHalfOriginal = (int)(extraWidth / diffCoefficient) / 2;

                sizes.Size = new Size(sizes.Size.Width - extraWidth, sizes.Size.Height);

                sizes.CropRegion = new Rectangle(
                    sizes.CropRegion.X + extraWidthHalfOriginal,
                    sizes.CropRegion.Y,
                    sizes.CropRegion.Width - extraWidthHalfOriginal * 2,
                    sizes.CropRegion.Height);

                sizes.Width = width;
            }
        }

        /// <summary>
        /// Shrink image height if it's higher then max value
        /// </summary>
        /// <param name="maxHeight"></param>
        /// <param name="sizes"></param>
        protected virtual void FillHeightIfBigger(int maxHeight, ImageNewSizes sizes)
        {
            if (sizes.Height > maxHeight)
            {
                sizes.Size = new Size(sizes.Size.Width, maxHeight);
                sizes.Height = maxHeight;
            }
        }

    }
}
