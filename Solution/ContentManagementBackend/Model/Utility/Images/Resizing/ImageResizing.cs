using Common.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace ContentManagementBackend
{
    public class ImageResizing
    {
        //методы 
        public static QueryResult<byte[]> FillSize(byte[] inputBytes, ImageTargetParameters parameters)
        {
            bool hasExceptions = false;
            MemoryStream outputStream = new MemoryStream();

            try
            {
                MemoryStream inputStream = new MemoryStream(inputBytes);
                using (Image inputImage = Image.FromStream(inputStream))
                {
                    ImageNewSizes sizes = new ImageNewSizes(inputImage.Width, inputImage.Height);
                    if (sizes.IsEmpty)
                    {
                        return new QueryResult<byte[]>(null, true);
                    }

                    PickResizeType(parameters, sizes);

                    if (sizes.IsEmpty)
                    {
                        return new QueryResult<byte[]>(null, true);
                    }

                    SaveToStream(inputImage, outputStream, parameters, sizes);
                }
            }
            catch(Exception exception)
            {
                hasExceptions = true;
            }


            return new QueryResult<byte[]>(outputStream.ToArray(), hasExceptions);
        }

        private static void PickResizeType(ImageTargetParameters parameters, ImageNewSizes sizes)
        {
            if (parameters.ResizeType == ImageResizeType.FitRatio)
            {
                LowerAllProportionally(parameters.Width, parameters.Height, sizes);
                RaiseToLowestProportionally(parameters.Width, parameters.Height, sizes);
            }
            else if (parameters.ResizeType == ImageResizeType.FitAndFill)
            {
                LowerAllProportionally(parameters.Width, parameters.Height, sizes);
                RaiseToLowestProportionally(parameters.Width, parameters.Height, sizes);
                AddSpaceOnBorders(parameters.Width, parameters.Height, sizes);
            }
            else if (parameters.ResizeType == ImageResizeType.FitHeightAndCropWidth)
            {
                LowerAllProportionally(parameters.Width, parameters.Height, sizes);
                RaiseHeightProportionally(parameters.Height, sizes);
                CutAll(parameters.Width, parameters.Height, sizes);
            }
            else if (parameters.ResizeType == ImageResizeType.FitWidthAndCropHeight)
            {
                LowerAllProportionally(parameters.Width, parameters.Height, sizes);
                RaiseWidthProportionally(parameters.Width, sizes);
                CutAll(parameters.Width, parameters.Height, sizes);
            }
            else if (parameters.ResizeType == ImageResizeType.FitWidthAndSqueezeHeight)
            {
                LowerAllProportionally(parameters.Width, parameters.Height, sizes);
                RaiseWidthProportionally(parameters.Width, sizes);
                FillHeightIfBigger(parameters.Height, sizes);
            }
        }

        private static void SaveToStream(Image inputImage, Stream outputStream
            , ImageTargetParameters parameters, ImageNewSizes sizes)
        {
            using (Bitmap newImage = new Bitmap(sizes.Width, sizes.Height))
            {
                using (Graphics g = Graphics.FromImage(newImage))
                {
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    g.InterpolationMode = InterpolationMode.Bilinear;
                    g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    g.Clear(Color.White);

                    if (parameters.RoundCorners)
                    {
                        GraphicsPath roundPath = CreateRoundPath(sizes.DrawingRegion, 6);
                        g.SetClip(roundPath);
                    }

                    g.DrawImage(inputImage,sizes.DrawingRegion, sizes.CropRegion, GraphicsUnit.Pixel);
                }
                newImage.Save(outputStream, parameters.TargetFormat);
            }
        }



        //изменение размеров
        /// <summary>
        /// Уменьшить ширину и высоту до размеров не больше заданных, сохраняя пропорции
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="sizes.Width"></param>
        /// <param name="новаяВысота"></param>
        private static void LowerAllProportionally(int maxWidth, int maxHeight, ImageNewSizes sizes)
        {
            // если надо уменьшить
            // то ширину и высоту делаем меньше или равные заданным
            if (sizes.DrawingRegion.Width > maxWidth || sizes.DrawingRegion.Height > maxHeight)
            {
                int новШирина = sizes.Width;
                int новВысота = sizes.Height;

                if (новВысота > maxHeight)
                {
                    новШирина = (int)(новШирина * ((double)maxHeight / новВысота));
                    новВысота = maxHeight;
                }
                if (новШирина > maxWidth)
                {
                    новВысота = (int)(новВысота * ((double)maxWidth / новШирина));
                    новШирина = maxWidth;
                }

                sizes.DrawingRegion = new Rectangle(sizes.DrawingRegion.X, sizes.DrawingRegion.Y,
                    новШирина, новВысота);
                sizes.Width = новШирина;
                sizes.Height = новВысота;
            }
        }

        /// <summary>
        /// Увеличить ширину или высоту до минимального размера, сохраняя пропорции
        /// </summary>
        /// <param name="minWidth"></param>
        /// <param name="minHeight"></param>
        /// <param name="sizes.Width"></param>
        /// <param name="sizes.Height"></param>
        private static void RaiseToLowestProportionally(int minWidth, int minHeight, ImageNewSizes sizes)
        {
            // если надо увеличить
            // то делаем или высоту или ширику равными заданным
            if (sizes.Width < minWidth && sizes.Height < minHeight)
            {
                double коэфШир = (double)minWidth / sizes.Width;
                double коэфВыс = (double)minHeight / sizes.Height;
                int новШирина = sizes.Width;
                int новВысота = sizes.Height;
                
                if (коэфВыс < коэфШир)
                {
                    новШирина = (int)(новШирина * ((double)minHeight / новВысота));
                    новВысота = minHeight;
                }
                else
                {
                    новВысота = (int)(новВысота * ((double)minWidth / новШирина));
                    новШирина = minWidth;
                }
                
                sizes.DrawingRegion = new Rectangle(sizes.DrawingRegion.X, sizes.DrawingRegion.Y,
                    новШирина, новВысота);
                sizes.Width = новШирина;
                sizes.Height = новВысота;
            }
        }

        /// <summary>
        /// Увеличить ширину до минимального размера, сохраняя пропорции
        /// </summary>
        /// <param name="minWidth"></param>
        /// <param name="sizes.Width"></param>
        /// <param name="sizes.Height"></param>
        private static void RaiseWidthProportionally(int minWidth, ImageNewSizes sizes)
        {
            if (sizes.Width < minWidth)
            {
                int новВысота = (int)(sizes.Height * ((double)minWidth / sizes.Width));
                int новШирина = minWidth;

                sizes.DrawingRegion = new Rectangle(
                    sizes.DrawingRegion.X,
                    sizes.DrawingRegion.Y,
                    новШирина,
                    новВысота);
                sizes.Width = новШирина;
                sizes.Height = новВысота;
            }
        }

        /// <summary>
        /// Увеличить ширину до минимального размера, сохраняя пропорции
        /// </summary>
        /// <param name="minWidth"></param>
        /// <param name="sizes.Width"></param>
        /// <param name="sizes.Height"></param>
        private static void RaiseHeightProportionally(int minHeight, ImageNewSizes sizes)
        {
            if (sizes.Height < minHeight)
            {
                int новШирина = (int)(sizes.Width * ((double)minHeight / sizes.Height));
                int новВысота = minHeight;

                sizes.DrawingRegion = new Rectangle(sizes.DrawingRegion.X, sizes.DrawingRegion.Y,
                   новШирина, новВысота);
                sizes.Width = новШирина;
                sizes.Height = новВысота;
            }
        }

        /// <summary>
        /// Дополняет пустым пространством недостающую высоту и ширину изображения
        /// </summary>
        /// <param name="minWidth"></param>
        /// <param name="minHeight"></param>
        /// <param name="sizes"></param>
        private static void AddSpaceOnBorders(int width, int height, ImageNewSizes sizes)
        {
            if (sizes.Width < width)
            {
                int половинаРазницы = (width - sizes.Width) / 2;
                sizes.DrawingRegion = new Rectangle(
                    sizes.DrawingRegion.X + половинаРазницы,
                    sizes.DrawingRegion.Y,
                    sizes.DrawingRegion.Width,
                    sizes.DrawingRegion.Height);
                sizes.Width = width;
            }
            if (sizes.Height < height)
            {
                int половинаРазницы = (height - sizes.Height) / 2;
                sizes.DrawingRegion = new Rectangle(
                    sizes.DrawingRegion.X,
                    sizes.DrawingRegion.Y + половинаРазницы,
                    sizes.DrawingRegion.Width,
                    sizes.DrawingRegion.Height);
                sizes.Height = height;
            }
        }

        /// <summary>
        /// Уменьшить изображение до нужной высоты обрезав по краям
        /// </summary>
        /// <param name="maxHeight"></param>
        /// <param name="sizes"></param>
        private static void CutHeight(int maxHeight, ImageNewSizes sizes)
        {
            if (sizes.Height > maxHeight)
            {
                int лишнее = sizes.Height - maxHeight;
                double коэфРазницы = (double)sizes.DrawingRegion.Height / sizes.CropRegion.Height;
                int половинаЛишнегоОригинал = (int)(лишнее / коэфРазницы) / 2;

                sizes.CropRegion = new Rectangle(
                    sizes.CropRegion.X,
                    sizes.CropRegion.Y + половинаЛишнегоОригинал,
                    sizes.CropRegion.Width,
                    sizes.CropRegion.Height - половинаЛишнегоОригинал);

                sizes.DrawingRegion = new Rectangle(
                    sizes.DrawingRegion.X,
                    sizes.DrawingRegion.Y,
                    sizes.DrawingRegion.Width,
                    maxHeight);

                sizes.Height = maxHeight;
            }
        }
        
        /// <summary>
        /// Обрезать лишнюю ширину и высоту по краям
        /// </summary>
        private static void CutAll(int width, int height, ImageNewSizes sizes)
        {
            if (sizes.Height > height)
            {
                int лишнее = sizes.Height - height;
                double коэфРазницы = (double)sizes.DrawingRegion.Height / sizes.CropRegion.Height;
                int половинаЛишнегоОригинал = (int)(лишнее / коэфРазницы) / 2;

                sizes.DrawingRegion = new Rectangle(
                    sizes.DrawingRegion.X,
                    sizes.DrawingRegion.Y,
                    sizes.DrawingRegion.Width,
                    sizes.DrawingRegion.Height - лишнее);

                sizes.CropRegion = new Rectangle(
                    sizes.CropRegion.X,
                    sizes.CropRegion.Y + половинаЛишнегоОригинал,
                    sizes.CropRegion.Width,
                    sizes.CropRegion.Height - половинаЛишнегоОригинал * 2);

                sizes.Height = height;
            }
            if (sizes.Width > width)
            {
                int лишнее = sizes.Width - width;
                double коэфРазницы = (double)sizes.DrawingRegion.Width / sizes.CropRegion.Width;
                int половинаЛишнегоОригинал = (int)(лишнее / коэфРазницы) / 2;

                sizes.DrawingRegion = new Rectangle(
                    sizes.DrawingRegion.X,
                    sizes.DrawingRegion.Y,
                    sizes.DrawingRegion.Width - лишнее,
                    sizes.DrawingRegion.Height);

                sizes.CropRegion = new Rectangle(
                    sizes.CropRegion.X + половинаЛишнегоОригинал,
                    sizes.CropRegion.Y ,
                    sizes.CropRegion.Width - половинаЛишнегоОригинал * 2,
                    sizes.CropRegion.Height );

                sizes.Width = width;
            }
        }
        
        /// <summary>
        /// Сжимает изображение по высоте, если превышает максимальную высоту
        /// </summary>
        /// <param name="maxHeight"></param>
        /// <param name="sizes"></param>
        private static void FillHeightIfBigger(int maxHeight, ImageNewSizes sizes)
        {
            if (sizes.Height > maxHeight)
            {
                sizes.DrawingRegion = new Rectangle(
                    sizes.DrawingRegion.X,
                    sizes.DrawingRegion.Y,
                    sizes.DrawingRegion.Width,
                    maxHeight);

                sizes.Height = maxHeight;
            }
        }



        //закругление
        private static GraphicsPath CreateRoundPath(int x, int y, int width, int height, int radius)
        {
            //http://tech.pro/tutorial/656/csharp-creating-rounded-rectangles-using-a-graphics-path
            int xw = x + width;
            int yh = y + height;
            int xwr = xw - radius;
            int yhr = yh - radius;
            int xr = x + radius;
            int yr = y + radius;
            int r2 = radius * 2;
            int xwr2 = xw - r2;
            int yhr2 = yh - r2;

            GraphicsPath p = new GraphicsPath();
            p.StartFigure();

            //Top Left Corner
            p.AddArc(x, y, r2, r2, 180, 90);
          

            //Top Edge
            p.AddLine(xr, y, xwr, y);

            //Top Right Corner
            p.AddArc(xwr2, y, r2, r2, 270, 90);

            //Right Edge
            p.AddLine(xw, yr, xw, yhr);

            //Bottom Right Corner
            p.AddArc(xwr2, yhr2, r2, r2, 0, 90);
          

            //Bottom Edge
            p.AddLine(xwr, yh, xr, yh);

            //Bottom Left Corner
            p.AddArc(x, yhr2, r2, r2, 90, 90);
         
            //Left Edge
            p.AddLine(x, yhr, x, yr);

            p.CloseFigure();
            return p;
        }

        private static GraphicsPath CreateRoundPath(Rectangle rect, int radius)
        {
            return CreateRoundPath(rect.X, rect.Y, rect.Width, rect.Height, radius); 
        }


    }
}