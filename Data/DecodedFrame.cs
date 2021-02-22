using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


//Credit: miso-xyz
//https://github.com/miso-xyz/FlipnoteDesktop

namespace EncodeAndSign.Data
{
    public class DecodedFrame
    {
        /// <summary>
        /// Layer 1 pixels as double array. Usage: Layer1Data[x,y]= ~true|false~;
        /// </summary>
        public bool[,] Layer1Data = new bool[256, 192];
        /// <summary>
        /// Layer 2 pixels as double array. Usage: Layer2Data[x,y]= ~true|false~;
        /// </summary>
        public bool[,] Layer2Data = new bool[256, 192];

        public DecodedFrame()
        {
            IsPaperWhite = true;
            Layer1Color = LayerColor.BlackWhite;
            Layer2Color = LayerColor.Red;
        }

        public DecodedFrame(DecodedFrame src)
        {
            Array.Copy(src.Layer1Data, Layer1Data, 256 * 192);
            Array.Copy(src.Layer2Data, Layer2Data, 256 * 192);
            IsPaperWhite = src.IsPaperWhite;
            Layer1Color = src.Layer1Color;
            Layer2Color = src.Layer2Color;
            SetImage(null, true);
        }

        private LayerColor _Layer1Color;
        public LayerColor Layer1Color
        {
            get => _Layer1Color;
            set
            {
                _Layer1Color = value;
                switch (value)
                {
                    case LayerColor.BlackWhite: Layer1ColorInt = IsPaperWhite ? 1 : 0; break;
                    case LayerColor.Red: Layer1ColorInt = 2; break;
                    case LayerColor.Blue: Layer1ColorInt = 3; break;
                }
            }
        }

        private LayerColor _Layer2Color;
        public LayerColor Layer2Color
        {
            get => _Layer2Color;
            set
            {
                _Layer2Color = value;
                switch (value)
                {
                    case LayerColor.BlackWhite: Layer2ColorInt = IsPaperWhite ? 1 : 0; break;
                    case LayerColor.Red: Layer2ColorInt = 2; break;
                    case LayerColor.Blue: Layer2ColorInt = 3; break;
                }
            }
        }

        public bool[,] GetLayerPixels(int layer)
        {
            return layer == 1 ? Layer1Data : Layer2Data;
        }

        public void SetLayerPixels(int layer, bool[,] pixels)
        {
            if (layer == 1)
            {
                Array.Copy(pixels, Layer1Data, 256 * 192);
            }
            else
            {
                Array.Copy(pixels, Layer2Data, 256 * 192);
            }
            SetImage(null, true);
        }

        byte[] pixels = new byte[64 * 192];

        public int Layer1ColorInt;
        public int Layer2ColorInt;

        public void SetImagePixel(int x, int y, int val)
        {
            int b = 256 * y + x;
            int p = 3 - b % 4;
            b /= 4;
            pixels[b] &= (byte)(~(0b11 << (2 * p)));
            pixels[b] |= (byte)(val << (2 * p));
        }

        public byte GetImagePixel(int x, int y)
        {
            int b = 256 * y + x;
            int p = 3 - b % 4;
            b /= 4;
            return (byte)((pixels[b] >> (2 * p)) & 0b11);
        }

        public void SetPixel(int layer, int x, int y)
        {
            if (!(0 <= x && x <= 255 && 0 <= y && y <= 191))
                return;
            if (layer == 1)
            {
                Layer1Data[x, y] = true;
                SetImagePixel(x, y, Layer1ColorInt);
            }
            else
            {
                Layer2Data[x, y] = true;
                if (!Layer1Data[x, y])
                {
                    SetImagePixel(x, y, Layer2ColorInt);
                }
            }
        }

        public void ErasePixel(int layer, int x, int y)
        {
            if (!(0 <= x && x <= 255 && 0 <= y && y <= 191))
                return;
            if (layer == 1)
            {
                Layer1Data[x, y] = false;
                SetImagePixel(x, y, Layer2Data[x, y] ? Layer2ColorInt : IsPaperWhite ? 0 : 1);
            }
            else
            {
                Layer2Data[x, y] = false;
                if (!Layer1Data[x, y])
                {
                    SetImagePixel(x, y, IsPaperWhite ? 0 : 1);
                }
            }
        }

        public void SetImage(WriteableBitmap bmp, bool forceRedraw = false)
        {
            if (forceRedraw)
            {
                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < 192; y++)
                    {
                        if (Layer1Data[x, y])
                            SetImagePixel(x, y, Layer1ColorInt);
                        else if (Layer2Data[x, y])
                            SetImagePixel(x, y, Layer2ColorInt);
                        else SetImagePixel(x, y, IsPaperWhite ? 0 : 1);
                    }
            }
            bmp?.WritePixels(new Int32Rect(0, 0, 256, 192), pixels, 64, 0);
        }

        private bool _IsPaperWhite = true;
        public bool IsPaperWhite
        {
            get => _IsPaperWhite;
            set
            {
                _IsPaperWhite = value;
                if (value)
                {
                    if (Layer1ColorInt == 0) Layer1ColorInt = 1;
                    if (Layer2ColorInt == 0) Layer2ColorInt = 1;
                }
                else
                {
                    if (Layer1ColorInt == 1) Layer1ColorInt = 0;
                    if (Layer2ColorInt == 1) Layer2ColorInt = 0;
                }
            }
        }

        public static BitmapPalette Palette = new BitmapPalette(new List<Color>
        {
            Colors.White,
            Colors.Black,
            Color.FromRgb(255, 42, 42), // Red
            Color.FromRgb( 10, 57,255)  // Blue
        });

        public WriteableBitmap Thumbnail { get; } = new WriteableBitmap(128, 96, 96, 96, PixelFormats.Indexed8,
            new BitmapPalette(new List<Color>
            {
                // 0-9
                Color.FromRgb(0,0,0),
                Color.FromRgb(  0,  0,255), /// 0b00000001 - 4B
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(255,  0,  0), /// 0b00000100 - 4R
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb( 85,  0,170), /// 0b00000111 -> R+3B
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 10-19
                Color.FromRgb(255,  0,255), /// 0b00001010 -> 2R+2B
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(255,  0, 128), /// 0b00001101 -> 3R+b
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0), /// 0b00010000 -> 4b
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0, 85), /// 0b00010011 -> b+3B
                // 20-29
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb( 85,  0,  0), /// 0b00011100 -> b+3R
                Color.FromRgb(0,0,0),
                // 30-39
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(  0,  0,127), /// 0b00100010 -> 2b+2B
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 40-49
                Color.FromRgb(127,  0,  0), /// 0b00101000 -> 2b+2R
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(189,189,255), /// 0b01000011 -> w+3B
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 50-59
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 60-69
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(255,255,255), /// 0b01000000 -> 4w
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                // 70-79
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(  0,  0,  0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(255, 63, 63), /// 0b01001100 -> w+3r
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 80-89
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 90-99
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 100-109
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 110-119
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 120-129
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 130-139
                Color.FromRgb(127,127,255), /// 0b10000010 -> 2w+2B
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(255,127,127), /// 0b10001000 -> 2w+2R
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 140-149
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 150-159
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 160-169
                Color.FromRgb(127,127,127), /// 0b101010000 -> 2w+2b
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 170-179
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 180-189
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 190-199
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(189,189,255), //0b11000001 -> 3w+B
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(255,189,189), // 0b11000100 -> 3w+R
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 200-209
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(189,189,189), /// 0b11010000 -> 3w + b
                Color.FromRgb(0,0,0),              
                // 210-219
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 220-229
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 330-239
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 240-249
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                // 250-255
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0),
                Color.FromRgb(0,0,0)
            }));
        private byte[] thumbnailPixels = new byte[128 * 96];

        public void CreateThumbnail()
        {
            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 96; y++)
                {
                    int b = 128 * y + x;
                    thumbnailPixels[b] = GetCompositeThumbnailPixel(x, y);
                }
            }
            Thumbnail.WritePixels(new Int32Rect(0, 0, 128, 96), thumbnailPixels, 128, 0);
        }

        byte GetCompositeThumbnailPixel(int x, int y)
        {
            byte nb = 0, nw = 0, nR = 0, nB = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    byte px = GetImagePixel(2 * x + i, 2 * y + j);

                    nw += (byte)(px == 0 ? 1 : 0);
                    nb += (byte)(px == 1 ? 1 : 0);
                    nR += (byte)(px == 2 ? 1 : 0);
                    nB += (byte)(px == 3 ? 1 : 0);
                }
            }
            if (nw == 4) return 1 << 6;
            if (nb == 4) return 1 << 4;
            if (nR == 4) return 1 << 2;
            if (nB == 4) return 1;
            return (byte)((nw << 6) + (nb << 4) + (nR << 2) + nB);
        }

        public byte[] CreateThumbnailW64()
        {
            var res = new byte[1536];
            /// TO DO : change with the actual frame thumbnail
            for (int x = 0; x < 64; x++)
                for (int y = 0; y < 48; y++)
                    w64SetPixel(res, x, y, (8 * (y / 8) + x / 8) % 16);
            return res;
        }

        private void w64SetPixel(byte[] raw, int x, int y, int val)
        {
            int offset = (8 * (y / 8) + (x / 8)) * 32 + 4 * (y % 8) + (x % 8) / 2;
            int nibble = x & 1;
            raw[offset] &= (byte)~(0x0F << (4 * nibble));
            raw[offset] |= (byte)(val << (4 * nibble));
        }

        public Flipnote._FrameData ToFrameData()
        {
            var fd = new Flipnote._FrameData();
            byte header = 0;
            header |= 1 << 7; // no frame diffing
            header |= (byte)(((int)Layer2Color + 1) << 3);
            header |= (byte)(((int)Layer1Color + 1) << 1);
            header |= (byte)(IsPaperWhite ? 1 : 0);
            fd.FirstByteHeader = header;
            for (int x = 0; x < 256; x++)
                for (int y = 0; y < 192; y++)
                {
                    fd.Layer1[y, x] = Layer1Data[x, y];
                    fd.Layer2[y, x] = Layer2Data[x, y];
                }
            return fd;
        }
    }
}
