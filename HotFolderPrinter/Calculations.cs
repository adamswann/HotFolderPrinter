using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace V2Lib.Imaging {
    public class Calculations {

        public class CopyCoordinates {
            public RectangleF Source;
            public RectangleF Destination;

            //public new string ToString() {
            //    return String.Format("Source: X:{0}, Y:{1}, W:{2}, H:{3};  Dest: X:{4}, Y:{5}, W:{6}, H:{7}",
            //        Source.X, Source.Y, Source.Width, Source.Height,
            //        Destination.X, Destination.Y, Destination.Width, Destination.Height
            //        );
            //}

        }


        /// <summary>
        /// Returns the scaling factor needed to fit the source rectangle in the target rectangle while maintaining aspect ratio.  Empty space may be left on one axis.
        /// </summary>
        public static float GetAspectFitScaleFactor(SizeF target, SizeF source) {
            return Math.Min(target.Width / source.Width, target.Height / source.Height);
        }

        /// <summary>
        /// Returns the scaling factor needed to fit the source rectangle in the target rectangle while maintaining aspect ratio.  Empty space may be left on one axis.
        /// </summary>
        public static double GetAspectFitScaleFactor(Size target, Size source) {
            return Math.Min((double)target.Width / source.Width, (double)target.Height / source.Height);
        }

        /// <summary>
        /// Returns the scaling factor needed to fill the source rectangle with the target rectangle while maintaining aspect ratio.  Some portion of the source image may need to be cropped.
        /// </summary>
        public static float GetAspectFillScaleFactor(SizeF target, SizeF source) {
            return Math.Max(target.Width / source.Width, target.Height / source.Height);
        }

        /// <summary>
        /// Returns the scaling factor needed to fill the source rectangle with the target rectangle while maintaining aspect ratio.  Some portion of the source image may need to be cropped.
        /// </summary>
        public static float GetAspectFillScaleFactor(Size target, Size source) {
            return Math.Max((float)target.Width / source.Width, (float)target.Height / source.Height);
        }


        public static CopyCoordinates ComputeCopyCoords(RectangleF target, SizeF size, CroppingModes cropMode) {
            switch (cropMode) {
                case CroppingModes.Scale:
                    return ComputeAspectFit(target, size);
                case CroppingModes.Stretch:
                    return ComputeFill(target, size);
                case CroppingModes.CropCenter:
                    return ComputeAspectFill(target, size, false);
                case CroppingModes.CropTop:
                    return ComputeAspectFill(target, size, true);
                default:
                    throw new Exception("Unsupported CropMode: " + cropMode.ToString());
            }
        }

        private static CopyCoordinates ComputeAspectFit(RectangleF target, SizeF sourceSize) {

            /// For an AspectFit, the entire source rectangle is copied over to the destination
            /// since the desire is to fit the entire source rectange within the target bounding box.
            RectangleF Source = new RectangleF(new PointF(0, 0), sourceSize);

            /// First, we'll compute the size of the destination rectangle -- which is the source rectangle scaled to fit.
            float ScaleFactor = GetAspectFitScaleFactor(target.Size, sourceSize);
            RectangleF DestinationSize = ScaleRectangle(Source, ScaleFactor);

            // Next, we need to position the new rectangle within the target.
            RectangleF Destination;
            float x;
            float y;
            if (DestinationSize.Size.Width < target.Size.Width) {
                // If the source rectangle is "tall" relative to the target, there will be white space on either side and we need to center the image horizontally in the target.

                x = target.X + (target.Size.Width - DestinationSize.Size.Width) / 2; // Amount of horiztonal white space, split in two.
                y = target.Y + 0;


            } else {
                // If the source rectangle is "wide" relative to the target, there will be white space on top and bottom we need to center the image vertically in the target.

                x = target.X + 0;
                y = target.Y + (target.Size.Height - DestinationSize.Size.Height) / 2; // Amount of vertical white space, split in two.

            }
            Destination = new RectangleF(new PointF(x, y), DestinationSize.Size);


            return new CopyCoordinates() { Destination = Destination, Source = Source };
        }

        private static CopyCoordinates ComputeAspectFill(RectangleF target, SizeF sourceSize, bool topCrop) {

            /// 
            /// CropCenter: Crop enough of both sides (or top and bottom) to fit within the bounding box.
            /// CropTop: For an H image, same as CenterCrop.  For a V image, crop from the bottom up.

            /// For an AspectFill, only a portion of the source is copied over, with the remainder cropped off to fit.

            /// First, we'll compute the size of the origin rectangle -- which is the target rectangle scaled to fit.
            float ScaleFactor = GetAspectFitScaleFactor(sourceSize, target.Size);

            SizeF UsableSourceSize = ScaleRectangle(target, ScaleFactor).Size;

            // Next, we need to position the new rectangle within the target.
            float x;
            float y;

            if (UsableSourceSize.Width < sourceSize.Width) {
                // If the usable source rectangle is narrower than the entire source, we need to offset the starting point by half the difference.

                x = (sourceSize.Width - UsableSourceSize.Width) / 2; // Amount of horiztonal white space, split in two.
                y = 0;

            } else {
                // If the usable source rectangle is taller than the entire source, we need to offset the starting point by half the difference.

                x = 0;

                if (topCrop) {
                    y = 0; // Always start at the top.
                } else {
                    y = (sourceSize.Height - UsableSourceSize.Height) / 2; // Amount of vertical white space, split in two.
                }

            }
            RectangleF Source = new RectangleF(new PointF(x, y), UsableSourceSize);


            // For an AspectFill, the entire destination is filled.
            RectangleF Destination = target;

            return new CopyCoordinates() { Destination = Destination, Source = Source };
        }



        private static CopyCoordinates ComputeFill(RectangleF target, SizeF sourceSize) {

            /// For an Stretch, the entire source rectangle is copied over to the destination
            RectangleF Source = new RectangleF(new PointF(0, 0), sourceSize);

            // The destination is the entire target box.
            RectangleF Destination = target;

            return new CopyCoordinates() { Destination = Destination, Source = Source };
        }

        public static RectangleF ScaleRectangle(RectangleF boundingBox, float scaleFactor) {

            float X = boundingBox.X * scaleFactor;
            float Y = boundingBox.Y * scaleFactor;

            float Width = boundingBox.Width * scaleFactor;
            float Height = boundingBox.Height * scaleFactor;

            return new RectangleF(X, Y, Width, Height);
        }


        public static PointF ScalePoint(PointF point, float scaleFactor) {

            float X = point.X * scaleFactor;
            float Y = point.Y * scaleFactor;

            return new PointF(X, Y);
        }

        /// <summary>
        /// Converts an angle measure (in degrees) to a number between 0 and 360.
        /// </summary>
        /// <param name="measure">An angle measure (in degrees).</param>
        /// <returns>An equivalent angle limited between 0 and 360 degrees.</returns>
        public static int NormalizeAngle(int measure) {

            // Brute force way of compensating for the negative.
            //while (measure < 0)
            //    measure += 360;

            // Less loopy method.
            if (measure < 0)
                measure = 360 - (Math.Abs(measure) % 360);

            return measure % 360;

        }

    }
}
