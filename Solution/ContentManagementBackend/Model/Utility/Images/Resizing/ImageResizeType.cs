using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public enum ImageResizeType
    {
        /// <summary>
        /// No resizing
        /// </summary>
        None,
        /// <summary>
        /// Save image propotions and resize to fit width and height.
        /// </summary>
        FitRatio,
        /// <summary>
        /// Save image propotions and resize to fit width and height. Add space to match exactly desired width and height.
        /// </summary>
        FitAndFill,
        /// <summary>
        /// Save image propotions and fit height. Then crop image width if it's larger than desized width.
        /// </summary>
        FitHeightAndCropWidth,
        /// <summary>
        /// Save image propotions and fit width. Then crop image height if it's larger than desized height.
        /// </summary>
        FitWidthAndCropHeight,
        /// <summary>
        /// Fit width. If image height is larger than desized height then squeeze it.
        /// </summary>
        FitWidthAndSqueezeHeight
    }

}
