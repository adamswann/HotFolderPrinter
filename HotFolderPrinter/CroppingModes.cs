using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V2Lib.Imaging {

    public enum CroppingModes {
        /// <summary>Scale the image to fit the bound box, but maintain the aspect ratio.  This may result in empty space on one axis.</summary>
        Scale = 0,
        /// <summary>Stretch the image on both axes, if needed, to fit the bounding box.  This may distort the image.</summary>
        Stretch,
        /// <summary>Crop enough of both sides (or top and bottom) to fit within the bounding box.</summary>
        CropCenter,
        /// <summary>For an H image, same as CenterCrop.  For a V image, crop from the bottom up.</summary>
        CropTop
    }

}
