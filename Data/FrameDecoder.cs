using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace EncodeAndSign.Data
{
    public class FrameDecoder
    {
        public List<bool[,]> BoolFrames { get; set; }

        public List<Bitmap> bitmaps { get; set; }
        public FrameDecoder(string[] filenames)
        {
            //frames to bool arrays
            List<bool[,]> bools = new List<bool[,]>();
            int i = 0;
            filenames.ToList().ForEach(frame =>
            {
                var color = new Bitmap(frame);
                var bw = color.Clone(new Rectangle(0, 0, color.Width, color.Height), PixelFormat.Format1bppIndexed);
                bools.Add(BitmapToBoolArray(bw));

            });
            BoolFrames = bools;

            //List<Bitmap> bits = new List<Bitmap>();
            ////bool arrays to bitmaps
            //BoolFrames.ForEach(frame =>
            //{
            //    bits.Add(bruhe(frame));
            //});
            //bitmaps = bits;
        }


        public bool[,] BitmapToBoolArray(Bitmap PiecesBitmap)
        {
            bool[,] PiecesBoolArray = new bool[256, 192];
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 192; y++)
                {
                    if (PiecesBitmap.GetPixel(x, y).GetBrightness() > 0.4)
                    {
                        PiecesBoolArray[x, y] = false;
                    }
                    else
                    {
                        PiecesBoolArray[x, y] = true;
                    }

                }
            }
            return PiecesBoolArray;
        }

        //leftover from debugging
        //todo: make a flipnote decoder :')
        public Bitmap BoolArrayToBitmap(bool[,] inp)
        {
            bool[,] boolArray = inp;

            Bitmap b = new Bitmap(256, 192);

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 192; j++)
                {
                    b.SetPixel(i, j, boolArray[i, j] ? Color.Black : Color.White);
                }
            }

            return (b);
        }


    }
}
