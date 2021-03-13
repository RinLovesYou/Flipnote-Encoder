using FFMpegCore;
using PPMLib;
using Rewrite.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rewrite.Encoder
{
    public class Encoder
    {
        private EncodeConfig Config = MainProgram.FlipnoteConfig;

        public Encoder()
        {
            Config = MainProgram.FlipnoteConfig;
        }

        public bool PrepareAudio()
        {
            var Folder = Config.InputFolder;
            var filename = Config.InputFilename;
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Writing Audio...");

                FFMpegArguments
                .FromFileInput($"{Folder}/{filename}", true)
                .OutputToFile($"{Folder}/{filename}.wav", true, o => o
                .WithCustomArgument("-ac 1 -ar 8192"))
                .ProcessSynchronously();

                Console.CursorLeft = 0;
                Console.WriteLine("Audio Written!");
                return true;
            }
            catch (Exception e)
            {
                Console.CursorLeft = 16;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not write audio!");
                return false;
            }



        }

        public int PrepareFrames()
        {
            switch (Config.ColorMode)
            {
                case 1:
                    {
                        FrameSplitter splitter = new FrameSplitter();
                        splitter.SplitFrames(Config.InputFolder, Config.InputFilename);

                        Ditherer ditherer = new Ditherer();
                        ditherer.TwoColor();
                        break;
                    }
                case 2:
                    {
                        FrameSplitter splitter = new FrameSplitter();
                        splitter.SplitFrames(Config.InputFolder, Config.InputFilename);

                        Ditherer ditherer = new Ditherer();
                        ditherer.ThreeColor(2);
                        break;
                    }
                case 3:
                    {
                        FrameSplitter splitter = new FrameSplitter();
                        splitter.SplitFrames(Config.InputFolder, Config.InputFilename);

                        Ditherer ditherer = new Ditherer();
                        ditherer.ThreeColor(3);
                        break;
                    }
                case 4:
                    {
                        FrameSplitter splitter = new FrameSplitter();
                        splitter.SplitFrames(Config.InputFolder, Config.InputFilename);

                        Ditherer ditherer = new Ditherer();
                        ditherer.FullColor();
                        break;
                    }
                default:
                    {
                        FrameSplitter splitter = new FrameSplitter();
                        splitter.SplitFrames(Config.InputFolder, Config.InputFilename);

                        Ditherer ditherer = new Ditherer();
                        ditherer.TwoColor();
                        break;
                    }

            }


            return Directory.EnumerateFiles(Config.InputFolder, "*.png").ToArray().Length;
        }

        public IEnumerable<PPMFrame> FullColorFrames(string Folder)
        {

            var images = Directory.GetFiles($"{Folder}", "*.png");
            MathUtils.NumericalSort(images);
            var PreviousColor = PenColor.Red;
            PPMFrame Previous = null;
            for (int i = 0; i < images.Length; i++)
            {

                PPMFrame frame = new PPMFrame();


                int black = 0;
                int white = 0;
                Rgba32 ColorBlue = Rgba32.ParseHex("#0000ff");
                Rgba32 ColorRed = Rgba32.ParseHex("#FF0000");
                Rgba32 ColorBlack = Rgba32.ParseHex("#000000");
                Rgba32 ColorWhite = Rgba32.ParseHex("FFFFFF");


                var ColorImage = (Image<Rgba32>)Image.Load($"{Folder}/frame_{i}.png");
                Image<Rgba32> GrayImage = null;
                try
                {
                    GrayImage = (Image<Rgba32>)Image.Load($"tmp/frame_{i}.png");
                }
                catch (Exception e)
                {
                    GrayImage = (Image<Rgba32>)Image.Load($"tmp/frame_{i - 1}.png");
                }

                GrayImage.Mutate(x => x.BinaryThreshold(0.5f));

                //Set all the funny pixels
                for (int x = 0; x < 256; x++)
                {
                    for (int y = 0; y < 192; y++)
                    {
                        frame.Layer1.setLinesEncoding(y, LineEncoding.CodedLine);
                        frame.Layer2.setLinesEncoding(y, LineEncoding.CodedLine);
                        var ColorPixel = ColorImage[x, y];
                        var ColorLuminance = (0.299 * ColorPixel.R + 0.587 * ColorPixel.G + 0.114 * ColorPixel.B) / 255;

                        var GrayPixel = GrayImage[x, y];
                        var GrayLuminance = (0.299 * GrayPixel.R + 0.587 * GrayPixel.G + 0.114 * GrayPixel.B) / 255;

                        var Red = ColorPixel == ColorRed;
                        var Blue = ColorPixel == ColorBlue;
                        var White = ColorPixel == ColorWhite;
                        var Black = ColorPixel == ColorBlack;

                        if (Red)
                        {
                            if (PreviousColor == PenColor.Blue)
                            {
                                if (GrayLuminance < 0.5f)
                                {
                                    frame.Layer1[y, x] = true;
                                    frame.Layer2[y, x] = false;
                                    black++;
                                }
                                else white++;
                            }
                            else
                            {
                                frame.Layer1[y, x] = false;
                                frame.Layer2[y, x] = true;
                            }


                        }
                        if (Blue)
                        {
                            if (PreviousColor == PenColor.Red)
                            {
                                if (GrayLuminance < 0.5f)
                                {
                                    frame.Layer1[y, x] = true;
                                    frame.Layer2[y, x] = false;
                                    black++;
                                }
                                else white++;
                            }
                            else
                            {
                                frame.Layer1[y, x] = false;
                                frame.Layer2[y, x] = true;
                            }


                        }
                        if (White)
                        {
                            frame.Layer1[y, x] = false;
                            frame.Layer2[y, x] = false;
                            white++;
                        }
                        if (Black)
                        {
                            frame.Layer1[y, x] = true;
                            frame.Layer2[y, x] = false;
                            black++;
                        }

                    }
                }

                byte header = 0;

                header |= 1 << 7; // no frame diffing


                if (PreviousColor == PenColor.Red)
                {
                    header |= (byte)((int)PenColor.Red << 3);
                    PreviousColor = PenColor.Blue;
                }
                else
                {
                    header |= (byte)((int)PenColor.Blue << 3);
                    PreviousColor = PenColor.Red;
                }


                header |= (byte)(((int)PenColor.Inverted) << 1);
                header |= (byte)(white > black ? 1 : 0);
                frame._firstByteHeader = header;

                if (black > white)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        for (int y = 0; y < 192; y++)
                        {
                            var ColorPixel = ColorImage[x, y];
                            var ColorLuminance = (0.299 * ColorPixel.R + 0.587 * ColorPixel.G + 0.114 * ColorPixel.B) / 255;

                            var GrayPixel = GrayImage[x, y];
                            var GrayLuminance = (0.299 * GrayPixel.R + 0.587 * GrayPixel.G + 0.114 * GrayPixel.B) / 255;

                            var Red = ColorPixel == ColorRed;
                            var Blue = ColorPixel == ColorBlue;
                            var White = ColorPixel == ColorWhite;
                            var Black = ColorPixel == ColorBlack;

                            if (Red)
                            {
                                if (PreviousColor == PenColor.Red)
                                {
                                    if (GrayLuminance > 0.5f)
                                    {
                                        frame.Layer1[y, x] = true;
                                        frame.Layer2[y, x] = false;
                                        black++;
                                    }
                                    else
                                    {
                                        frame.Layer1[y, x] = false;
                                        frame.Layer2[y, x] = false;
                                    }
                                }
                                else
                                {
                                    frame.Layer1[y, x] = false;
                                    frame.Layer2[y, x] = true;
                                }


                            }
                            if (Blue)
                            {
                                if (PreviousColor == PenColor.Blue)
                                {
                                    if (GrayLuminance > 0.5f)
                                    {
                                        frame.Layer1[y, x] = true;
                                        frame.Layer2[y, x] = false;
                                        black++;
                                    }
                                    else
                                    {
                                        frame.Layer1[y, x] = false;
                                        frame.Layer2[y, x] = false;
                                    }
                                }
                                else
                                {
                                    frame.Layer1[y, x] = false;
                                    frame.Layer2[y, x] = true;
                                }


                            }
                            if (White)
                            {
                                frame.Layer1[y, x] = true;
                                frame.Layer2[y, x] = false;
                            }
                            if (Black)
                            {
                                frame.Layer1[y, x] = false;
                                frame.Layer2[y, x] = false;
                            }
                        }
                    }
                }

                frame.PaperColor = (PaperColor)(frame._firstByteHeader % 2);
                frame.Layer1.PenColor = (PenColor)((frame._firstByteHeader >> 1) & 3);
                frame.Layer2.PenColor = (PenColor)((frame._firstByteHeader >> 3) & 3);
                yield return frame;


            }

        }

        public IEnumerable<PPMFrame> ThreeColorFrames(string Folder, PenColor color)
        {
            var images = Directory.GetFiles($"{Folder}", "*.png");
            MathUtils.NumericalSort(images);

            for (int i = 0; i < images.Length; i++)
            {
                PPMFrame frame = new PPMFrame();


                Rgba32 ColorBlue = Rgba32.ParseHex("#0000ff");
                Rgba32 ColorRed = Rgba32.ParseHex("#FF0000");
                Rgba32 ColorBlack = Rgba32.ParseHex("#000000");
                Rgba32 ColorWhite = Rgba32.ParseHex("FFFFFF");

                var ColorImage = (Image<Rgba32>)Image.Load($"{Folder}/frame_{i}.png");
                int black = 0;
                int white = 0;
                for (int x = 0; x < 256; x++)
                {
                    for (int y = 0; y < 192; y++)
                    {
                        frame.Layer1.setLinesEncoding(y, LineEncoding.CodedLine);
                        frame.Layer2.setLinesEncoding(y, LineEncoding.CodedLine);

                        var ColorPixel = ColorImage[x, y];
                        var ColorLuminance = (0.299 * ColorPixel.R + 0.587 * ColorPixel.G + 0.114 * ColorPixel.B) / 255;

                        var Red = ColorPixel == ColorRed;
                        var Blue = ColorPixel == ColorBlue;
                        var White = ColorPixel == ColorWhite;
                        var Black = ColorPixel == ColorBlack;

                        if (Red)
                        {
                            frame.Layer2[y, x] = true;
                            frame.Layer1[y, x] = false;
                        }
                        if (Blue)
                        {
                            frame.Layer2[y, x] = true;
                            frame.Layer1[y, x] = false;
                        }
                        if (White)
                        {
                            frame.Layer2[y, x] = false;
                            frame.Layer1[y, x] = false;
                            white++;
                        }
                        if (Black)
                        {
                            frame.Layer2[y, x] = false;
                            frame.Layer1[y, x] = true;
                            black++;
                        }

                    }

                }

                if (black > white)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        for (int y = 0; y < 192; y++)
                        {

                            var ColorPixel = ColorImage[x, y];
                            var ColorLuminance = (0.299 * ColorPixel.R + 0.587 * ColorPixel.G + 0.114 * ColorPixel.B) / 255;

                            var Red = ColorPixel == ColorRed;
                            var Blue = ColorPixel == ColorBlue;
                            var White = ColorPixel == ColorWhite;
                            var Black = ColorPixel == ColorBlack;

                            if (Red)
                            {
                                frame.Layer2[y, x] = true;
                                frame.Layer1[y, x] = false;
                            }
                            if (Blue)
                            {
                                frame.Layer2[y, x] = true;
                                frame.Layer1[y, x] = false;
                            }
                            if (White)
                            {
                                frame.Layer2[y, x] = false;
                                frame.Layer1[y, x] = true;

                            }
                            if (Black)
                            {
                                frame.Layer2[y, x] = false;
                                frame.Layer1[y, x] = false;

                            }

                        }

                    }
                }
                byte header = 0;

                header |= 1 << 7; // no frame diffing

                header |= (byte)(((int)color) << 3);
                header |= (byte)(((int)PenColor.Inverted) << 1);
                header |= (byte)(white > black ? 1 : 0);
                frame._firstByteHeader = header;

                frame.PaperColor = (PaperColor)(frame._firstByteHeader % 2);
                frame.Layer1.PenColor = (PenColor)((frame._firstByteHeader >> 1) & 3);
                frame.Layer2.PenColor = (PenColor)((frame._firstByteHeader >> 3) & 3);

                yield return frame;
            }
        }

        public IEnumerable<PPMFrame> BlackWhiteFrames(string Folder)
        {
            var images = Directory.GetFiles($"{Folder}", "*.png");
            MathUtils.NumericalSort(images);
            Rgba32 ColorBlack = Rgba32.ParseHex("#000000");
            Rgba32 ColorWhite = Rgba32.ParseHex("FFFFFF");

            for (int i = 0; i < images.Length; i++)
            {
                PPMFrame frame = new PPMFrame();
                PPMFrame PreviousFrame = null;




                var ColorImage = (Image<Rgba32>)Image.Load($"{Folder}/frame_{i}.png");

                int white = 0;
                int black = 0;
                for (int y = 0; y < 192; y++)
                {
                    frame.Layer1.setLinesEncoding(y, LineEncoding.InvertedCodedLine);
                    frame.Layer2.setLinesEncoding(y, LineEncoding.SkipLine);
                    for (int x = 0; x < 256; x++)
                    {
                        var ColorPixel = ColorImage[x, y];
                        var Black = ColorPixel == ColorBlack;

                        if (Black)
                        {
                            frame.Layer1[y, x] = true;
                            black++;
                        } else
                        {
                            white++;
                        }
                    }

                }

                if(black > white)
                {
                    for (int y = 0; y < 192; y++)
                    {
                        for (int x = 0; x < 256; x++)
                        {
                            var ColorPixel = ColorImage[x, y];
                            var Black = ColorPixel == ColorBlack;

                            if (Black)
                            {
                                frame.Layer1[y, x] = false;
                                
                            }
                            else
                            {
                                frame.Layer1[y, x] = true;
                                
                            }
                        }

                    }
                }

                byte header = 0;

                header |= 1 << 7; // no frame diffing

                header |= (byte)(((int)PenColor.Red) << 3);
                header |= (byte)(((int)PenColor.Inverted) << 1);
                header |= (byte)(white > black ? 1 : 0);
                frame._firstByteHeader = header;

                frame.PaperColor = (PaperColor)(frame._firstByteHeader % 2);
                frame.Layer1.PenColor = (PenColor)((frame._firstByteHeader >> 1) & 3);
                frame.Layer2.PenColor = (PenColor)((frame._firstByteHeader >> 3) & 3);

                yield return frame;


            }
        }


    }
}
