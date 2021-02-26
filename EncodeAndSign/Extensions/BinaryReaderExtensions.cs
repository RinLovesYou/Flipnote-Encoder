using EncodeAndSign.Data;
using System;
using System.IO;
using System.Text;

namespace EncodeAndSign.Extensions
{
    internal static class BinaryReaderExtensions
    {
        public static string ReadWChars(this BinaryReader r, int count)
        {
            return Encoding.Unicode.GetString(r.ReadBytes(2 * count));
        }

        public static Flipnote._FrameData ReadPPMFrameData(this BinaryReader r, int cnt)
        {
            var fd = new Flipnote._FrameData();
            fd.StreamPosition = r.BaseStream.Position;
            try
            {
                fd.FirstByteHeader = r.ReadByte();
            }
            catch (EndOfStreamException)
            {
                if (fd.StreamPosition == 4288480943)
                    throw new Exception("Critical data corruption found. Are you trying a memory pit?");
                else
                    throw new Exception("Flipnote file is broken");
            }
            if ((fd.FirstByteHeader & 0b01100000) != 0)
            {
                fd.TranslateX = r.ReadSByte();
                fd.TranslateY = r.ReadSByte();
            }
            fd.Layer1LineEncoding = r.ReadBytes(48);
            fd.Layer2LineEncoding = r.ReadBytes(48);
            string enc1 = "";
            for (int line = 0; line < 192; line++)
            {
                switch (fd.GetLineEncoding1(line))
                {
                    case Flipnote.LineEncoding.SkipLine:
                        {
                            enc1 += "0";
                            break;
                        }
                    case Flipnote.LineEncoding.CodedLine:
                        {
                            enc1 += "1";
                            PPMLineEncDealWith4Bytes(r, fd, 1, line);
                            break;
                        }
                    case Flipnote.LineEncoding.InvertedCodedLine:
                        {
                            enc1 += "2";
                            PPMLineEncDealWith4Bytes(r, fd, 1, line, true);
                            break;
                        }
                    case Flipnote.LineEncoding.RawLineData:
                        {
                            enc1 += "3";
                            PPMLineEncDealWithRawData(r, fd, 1, line);
                            break;
                        }
                }
            }

            for (int line = 0; line < 192; line++)
            {
                switch (fd.GetLineEncoding2(line))
                {
                    case Flipnote.LineEncoding.SkipLine: break;
                    case Flipnote.LineEncoding.CodedLine:
                        {
                            PPMLineEncDealWith4Bytes(r, fd, 2, line);
                            break;
                        }
                    case Flipnote.LineEncoding.InvertedCodedLine:
                        {
                            PPMLineEncDealWith4Bytes(r, fd, 2, line, true);
                            break;
                        }
                    case Flipnote.LineEncoding.RawLineData:
                        {
                            PPMLineEncDealWithRawData(r, fd, 2, line);
                            break;
                        }
                }
            }

            return fd;
        }

        private static void PPMLineEncDealWith4Bytes(BinaryReader r, Flipnote._FrameData fd, int layer, int line, bool inv = false)
        {
            int y = 0;
            if (inv)
            {
                for (int i = 0; i < 256; i++)
                    if (layer == 1)
                        fd.Layer1[line, i] = true;
                    else
                        fd.Layer2[line, i] = true;
            }
            byte b1 = r.ReadByte(),
                b2 = r.ReadByte(),
                b3 = r.ReadByte(),
                b4 = r.ReadByte();

            uint bytes = ((uint)(b1 << 24)) + ((uint)(b2 << 16)) + ((uint)(b3 << 8)) + b4;
            while (bytes != 0)
            {
                if ((bytes & 0x80000000) != 0)
                {
                    var pixels = r.ReadByte();
                    for (int i = 0; i < 8; i++)
                    {
                        if (layer == 1)
                            fd.Layer1[line, y++] = ((pixels >> i) & 1) == 1;
                        else
                            fd.Layer2[line, y++] = ((pixels >> i) & 1) == 1;
                    }
                }
                else y += 8;
                bytes <<= 1;
            }
        }

        private static void PPMLineEncDealWithRawData(BinaryReader r, Flipnote._FrameData fd, int layer, int line)
        {
            int y = 0;
            for (int i = 0; i < 32; i++)
            {
                byte val = r.ReadByte();
                for (int b = 0; b < 8; b++)
                    if (layer == 1)
                        fd.Layer1[line, y++] = ((val >> b) & 1) == 1;
                    else
                        fd.Layer2[line, y++] = ((val >> b) & 1) == 1;
            }
        }
    }
}
