using Rewrite.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rewrite.Encoder
{
    public class Ditherer
    {
        private EncodeConfig Config { get; set; }

        public Ditherer()
        {
            Config = MainProgram.FlipnoteConfig;
        }

        public bool FullColor()
        {
            var Folder = Config.InputFolder;
            var files = Directory.EnumerateFiles(Folder, "*.png");
            var filenames = files.ToArray();
            MathUtils.NumericalSort(filenames);

            var contrast = Config.Contrast;

            IDither DitheringType = null;
            switch (Config.DitheringMode)
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
                        x.Contrast(contrast + 2);
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
            return true;
        }

        public void ThreeColor(int WhichColor)
        {
            var Folder = Config.InputFolder;
            var files = Directory.EnumerateFiles(Folder, "*.png");
            var filenames = files.ToArray();
            MathUtils.NumericalSort(filenames);

            var contrast = Config.Contrast;

            IDither DitheringType = null;
            switch (Config.DitheringMode)
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
            switch(WhichColor)
            {
                case 2: colors.Add(Color.Red); break;
                case 3: colors.Add(Color.Blue); break;
                default: colors.Add(Color.Red); break;

            }
            colors.Add(Color.Black);
            colors.Add(Color.White);

            for (int i = 0; i < filenames.Length; i++)
            {
                using (Image<Rgba32> image = (Image<Rgba32>)Image.Load(filenames[i]))
                {
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

        public bool TwoColor()
        {
            var Folder = Config.InputFolder;
            var files = Directory.EnumerateFiles(Folder, "*.png");
            var filenames = files.ToArray();
            MathUtils.NumericalSort(filenames);

            var contrast = Config.Contrast;

            IDither DitheringType = null;
            switch (Config.DitheringMode)
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

            for (int i = 0; i < filenames.Length; i++)
            {
                using (Image<Rgba32> image = (Image<Rgba32>)Image.Load(filenames[i]))
                {

                    image.Mutate(x =>
                    {
                        if (contrast != 0)
                        {
                            x.Contrast(contrast);
                        }
                        x.BinaryDither(DitheringType);
                    });
                    image.SaveAsPng($"{Folder}/frame_{i}.png");
                    image.Dispose();
                }
            }
            return true;
        }

    }
}
