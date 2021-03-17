using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using System;
using System.Collections.Generic;
using System.IO;

namespace EncodeAndSign.Encoder
{
    public class ImageEncoder
    {
        public ImageEncoder()
        {

        }



        public void DoRinDithering(string[] filenames, int type, float contrast)
        {
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
                    DitheringType = KnownDitherings.StevensonArce;
                    break;
                case 9:
                    DitheringType = KnownDitherings.Sierra2;
                    break;
                case 10:
                    DitheringType = KnownDitherings.Sierra3;
                    break;
                case 11:
                    DitheringType = KnownDitherings.SierraLite;
                    break;
                case 12:
                    DitheringType = KnownDitherings.Stucki;
                    break;
                case 13:
                    DitheringType = KnownDitherings.Ordered3x3;
                    break;
                default:
                    //this one is my favorite :)
                    DitheringType = KnownDitherings.Bayer8x8;
                    break;
            }

            List<Color> colors = new List<Color>();
            colors.Add(Color.Red);
            //colors.Add(Color.Blue);
            colors.Add(Color.Black);
            colors.Add(Color.White);
            colors.Add(Color.Blue);

            for (int i = 0; i < filenames.Length; i++)
            {
                Directory.CreateDirectory("tmp");
                using (Image<Rgba32> image = (Image<Rgba32>)Image.Load(filenames[i]))
                {
                    Image<Rgba32> bw = image.Clone();
                    bw.Mutate(x =>
                    {
                        x.Contrast(3f);
                        x.BinaryDither(DitheringType, Color.Black, Color.White);
                    });
                    bw.Save($"tmp/frame_{i}.png");
                    bw.Dispose();

                    image.Mutate(x =>
                    {
                        if (contrast != 0)
                        {
                            x.Contrast(contrast);
                        }
                        //x.BinaryDither(DitheringType);
                        var Palette = new ReadOnlyMemory<Color>(colors.ToArray());


                        x.Dither(DitheringType, Palette);
                    });
                    image.SaveAsPng($"frames/frame_{i}.png");
                    image.Dispose();
                }
            }
        }
    }
}
