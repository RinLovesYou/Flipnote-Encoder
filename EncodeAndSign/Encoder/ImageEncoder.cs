using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using System.Collections.Generic;
using System.Drawing;

namespace EncodeAndSign.Encoder
{
    public class ImageEncoder
    {
        public ImageEncoder()
        {

        }

        public void DoRinDithering(string[] filenames, int type)
        {
            List<Bitmap> bitmaps = new List<Bitmap>();
            IDither DitheringType = null;
            switch (type)
            {
                case 1:
                    DitheringType = KnownDitherings.Bayer8x8;
                    break;
                case 2:
                    DitheringType = KnownDitherings.Bayer4x4;
                    break;
                case 3:
                    DitheringType = KnownDitherings.Bayer2x2;
                    break;
                case 4:
                    DitheringType = KnownDitherings.FloydSteinberg;
                    break;
                case 5:
                    DitheringType = KnownDitherings.Atkinson;
                    break;
                case 6:
                    DitheringType = KnownDitherings.Burks;
                    break;
                case 7:
                    DitheringType = KnownDitherings.JarvisJudiceNinke;
                    break;
                case 8:
                    DitheringType = KnownDitherings.Sierra3;
                    break;
                case 9:
                    DitheringType = KnownDitherings.StevensonArce;
                    break;
                case 10:
                    DitheringType = KnownDitherings.Sierra2;
                    break;
                case 11:
                    DitheringType = KnownDitherings.Sierra3;
                    break;
                case 12:
                    DitheringType = KnownDitherings.SierraLite;
                    break;
                case 13:
                    DitheringType = KnownDitherings.Stucki;
                    break;
                case 14:
                    DitheringType = KnownDitherings.Ordered3x3;
                    break;
                default:
                    //this one is my favorite :)
                    DitheringType = KnownDitherings.Bayer8x8;
                    break;
            }
            for (int i = 0; i < filenames.Length; i++)
            {
                using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(filenames[i]))
                {
                    image.Mutate(x =>
                    {
                        x.BinaryDither(DitheringType);
                    });
                    image.SaveAsPng($"frames/frame_{i}.png");
                    image.Dispose();
                }
            }
        }
    }
}
