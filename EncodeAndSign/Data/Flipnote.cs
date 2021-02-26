using EncodeAndSign.Encoder;
using EncodeAndSign.Extensions;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;

//credit: miso-xyz
//https://github.com/miso-xyz/FlipnoteDesktop
//Edit of miso's flipnote class to extend functionality needed for audio injection.

namespace EncodeAndSign.Data
{
    public class Flipnote
    {
        public Flipnote()
        {

        }

        //used to read flipnotes, might be useful in the future for one-off signing 
        public Flipnote(string filename)
        {
            using (BinaryReader r = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                /// #0000
                if (r.ReadChars(4).Equals(FileMagic))
                    throw new FileFormatException("Unexpected file format.");
                /// #0004
                AnimationDataSize = r.ReadUInt32();
                /// #0008
                SoundDataSize = r.ReadUInt32();
                /// #000C
                FrameCount = r.ReadUInt16();
                /// #000E
                FormatVersion = r.ReadUInt16();
                if (FormatVersion != 0x24)
                    throw new FileFormatException("Wrong format version. It should be 0x24.");
                /// #0010
                Metadata.Lock = r.ReadUInt16();
                /// #0012
                Metadata.ThumbnailFrameIndex = r.ReadUInt16();
                /// #0014
                Metadata.RootAuthorName = r.ReadWChars(11);
                /// #002A
                Metadata.ParentAuthorName = r.ReadWChars(11);
                /// #0040
                Metadata.CurrentAuthorName = r.ReadWChars(11);
                /// #0056
                Metadata.ParentAuthorId = r.ReadBytes(8);
                /// #005E
                Metadata.CurrentAuthorId = r.ReadBytes(8);
                /// #0066
                Metadata.ParentFilename = r.ReadBytes(18);
                /// #0078
                Metadata.CurrentFilename = r.ReadBytes(18);
                /// #008A
                Metadata.RootAuthorId = r.ReadBytes(8);
                /// #0092
                Metadata.RootFileFragment = r.ReadBytes(8);
                /// #009A
                Metadata.Timestamp = r.ReadUInt32();
                /// #009E
                Metadata._0x9E = r.ReadUInt16();
                /// #00A0
                RawThumbnail = r.ReadBytes(1536);
                /// #06A0
                AnimationHeader.FrameOffsetTableSize = r.ReadUInt16();
                /// #06A2
                AnimationHeader._06A2 = r.ReadUInt16();
                /// #06A6
                AnimationHeader.Flags = r.ReadUInt32();
                /// #06A8
                AnimationHeader.Offsets = new uint[AnimationHeader.FrameOffsetTableSize / 4];
                int len = AnimationHeader.FrameOffsetTableSize / 4;
                for (int i = 0; i < len; i++)
                    AnimationHeader.Offsets[i] = r.ReadUInt32();

                long framesPos0 = r.BaseStream.Position;
                Frames = new _FrameData[len];
                long offset = 0x06A8 + AnimationHeader.FrameOffsetTableSize;
                for (int i = 0; i < len; i++)
                {
                    r.BaseStream.Seek(offset + AnimationHeader.Offsets[i], SeekOrigin.Begin);
                    Frames[i] = r.ReadPPMFrameData(0x06A8 + AnimationHeader.FrameOffsetTableSize);
                    Frames[i].AnimationIndex = Array.IndexOf(AnimationHeader.Offsets, Frames[i].StreamPosition);
                    if (i > 0)
                    {
                        Frames[i].Overwrite(Frames[i - 1]);
                    }
                }

                if (SoundDataSize == 0) return;

                offset = 0x6A0 + AnimationDataSize;
                r.BaseStream.Seek(offset, SeekOrigin.Begin);
                SoundEffectFlags = new byte[Frames.Length];
                for (int i = 0; i < Frames.Length; i++)
                {
                    SoundEffectFlags[i] = r.ReadByte();
                }
                offset += Frames.Length;

                if (SoundDataSize == 0) return;

                // make the next offset dividable by 4
                r.ReadBytes((int)((4 - offset % 4) % 4));

                SoundHeader.BGMTrackSize = r.ReadUInt32();
                SoundHeader.SE1TrackSize = r.ReadUInt32();
                SoundHeader.SE2TrackSize = r.ReadUInt32();
                SoundHeader.SE3TrackSize = r.ReadUInt32();
                SoundHeader.CurrentFramespeed = r.ReadByte();
                SoundHeader.RecordingBGMFramespeed = r.ReadByte();
                r.ReadBytes(14);

                SoundData.RawBGM = r.ReadBytes((int)SoundHeader.BGMTrackSize);
                SoundData.RawSE1 = r.ReadBytes((int)SoundHeader.SE1TrackSize);
                SoundData.RawSE2 = r.ReadBytes((int)SoundHeader.SE2TrackSize);
                SoundData.RawSE3 = r.ReadBytes((int)SoundHeader.SE3TrackSize);

                if (r.BaseStream.Position == r.BaseStream.Length)
                {
                    // file is RSA unsigned -> do something...
                }
                else
                {
                    // Next 0x80 bytes = RSA-1024 SHA-1 signature
                    Signature = r.ReadBytes(0x80);
                    // Next 0x10 bytes are filled with 0
                }
            }
        }



        public readonly char[] FileMagic = new char[4] { 'P', 'A', 'R', 'A' };
        public uint AnimationDataSize;
        public uint SoundDataSize;
        public ushort FrameCount;
        public ushort FormatVersion;
        public _Metadata Metadata = new _Metadata();
        public byte[] RawThumbnail;
        public _AnimationHeader AnimationHeader = new _AnimationHeader();
        public _FrameData[] Frames;
        public byte[] SoundEffectFlags;
        public _SoundHeader SoundHeader = new _SoundHeader();
        public _SoundData SoundData = new _SoundData();
        public byte[] Signature;

        public string Filename;

        public static byte[] WavFileStreamByteArray;


        public static Flipnote New(string authorName, byte[] authorId, List<DecodedFrame> frames, bool ignoreMetadata = false)
        {
            var f = new Flipnote();
            f.FrameCount = (ushort)(frames.Count - 1);
            f.FormatVersion = 0x24;

            if (!ignoreMetadata)
            {
                f.Metadata.RootAuthorId = new byte[8];
                f.Metadata.ParentAuthorId = new byte[8];
                f.Metadata.CurrentAuthorId = new byte[8];
                Array.Copy(authorId, f.Metadata.RootAuthorId, 8);
                Array.Copy(authorId, f.Metadata.ParentAuthorId, 8);
                Array.Copy(authorId, f.Metadata.CurrentAuthorId, 8);
                f.Metadata.RootAuthorName = authorName;
                f.Metadata.ParentAuthorName = authorName;
                f.Metadata.CurrentAuthorName = authorName;

                string mac6 = string.Join("", authorId.Take(3).Reverse().Select(t => t.ToString("X2")));
                var asm = Assembly.GetEntryAssembly().GetName().Version;
                var dt = DateTime.UtcNow;
                var fnVM = ((byte)asm.Major).ToString("X2");
                var fnVm = ((byte)asm.Minor).ToString("X2");
                var fnYY = (byte)(dt.Year - 2009);
                var fnMD = dt.Month * 32 + dt.Day;
                var fnTi = (((dt.Hour * 3600 + dt.Minute * 60 + dt.Second) % 4096) >> 1) + (fnMD > 255 ? 1 : 0);
                fnMD = (byte)fnMD;
                var fnYMD = (fnYY << 9) + fnMD;
                var H6_9 = fnYMD.ToString("X4");
                var H89 = ((byte)fnMD).ToString("X2");
                var HABC = fnTi.ToString("X3");

                string _13str = $"80{fnVM}{fnVm}{H6_9}{HABC}";
                string nEdited = 0.ToString().PadLeft(3, '0');
                var filename = $"{mac6}_{_13str}_{nEdited}.ppm";
                f.Filename = FilenameChecksumDigit(filename) + filename.Remove(0, 1);

                var rawfn = new byte[18];
                for (int i = 0; i < 3; i++)
                {
                    rawfn[i] = byte.Parse("" + mac6[2 * i] + mac6[2 * i + 1], System.Globalization.NumberStyles.HexNumber);
                }
                for (int i = 3; i < 16; i++)
                {
                    rawfn[i] = (byte)_13str[i - 3];
                }
                rawfn[16] = rawfn[17] = 0;

                f.Metadata.ParentFilename = new byte[18];
                f.Metadata.CurrentFilename = new byte[18];

                Array.Copy(rawfn, f.Metadata.ParentFilename, 18);
                Array.Copy(rawfn, f.Metadata.CurrentFilename, 18);

                f.Metadata.RootFileFragment = new byte[8];
                for (int i = 0; i < 3; i++)
                {
                    f.Metadata.RootFileFragment[i] =
                        byte.Parse("" + mac6[2 * i] + mac6[2 * i + 1], System.Globalization.NumberStyles.HexNumber);
                }
                for (int i = 3; i < 8; i++)
                {
                    f.Metadata.RootFileFragment[i] =
                        (byte)((byte.Parse("" + _13str[2 * (i - 3)], System.Globalization.NumberStyles.HexNumber) << 4)
                              + byte.Parse("" + _13str[2 * (i - 3) + 1], System.Globalization.NumberStyles.HexNumber));
                }
                f.Metadata.Timestamp = (uint)((dt - new DateTime(2000, 1, 1, 0, 0, 0)).TotalSeconds);
                f.RawThumbnail = new DecodedFrame().CreateThumbnailW64();
            }
            // write the animation data

            uint animDataSize = 0;

            using (WaveFileReader reader = new WaveFileReader("frames/audio.wav"))
            {

                byte[] buffer = new byte[reader.Length];
                int read = reader.Read(buffer, 0, buffer.Length);
                short[] sampleBuffer = new short[read / 2];
                Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);

                List<byte> bgm = new List<byte>();
                AdpcmEncoder encoder = new AdpcmEncoder();
                for (int i = 0; i < sampleBuffer.Length; i += 2)
                {
                    try
                    {
                        bgm.Add((byte)(encoder.Encode(sampleBuffer[i]) | encoder.Encode(sampleBuffer[i + 1]) << 4));
                    }
                    catch (Exception e)
                    {

                    }

                }

                f.SoundData.RawBGM = bgm.ToArray();
            }



            f.SoundDataSize = (uint)f.SoundData.RawBGM.Length;

            f.AnimationHeader.FrameOffsetTableSize = (ushort)(4 * frames.Count);
            f.AnimationHeader.Flags = 0x430000; // ???

            f.Frames = new _FrameData[frames.Count];

            for (int i = 0; i < frames.Count; i++)
            {

                f.Frames[i] = frames[i].ToFrameData();


                animDataSize += (uint)f.Frames[i].ToByteArray().Length;
            }
            f.AnimationDataSize = animDataSize;

            f.SoundHeader.CurrentFramespeed = 0;
            f.SoundHeader.RecordingBGMFramespeed = 0;
            return f;
        }

        public bool Signed = false;

        public void Save(string fn)
        {
            using (var w = new BinaryWriter(new FileStream(fn, FileMode.Create)))
            {
                AnimationDataSize = (uint)(AnimationDataSize + 8 + Frames.Count() * 4);
                var AllignSize = (uint)(4 - ((0x6A0 + AnimationDataSize + Frames.Count()) % 4));
                if (AllignSize != 4)
                    AnimationDataSize += AllignSize;
                w.Write(FileMagic);
                w.Write(AnimationDataSize);
                w.Write(SoundDataSize);
                w.Write(FrameCount);
                w.Write((ushort)0x0024);
                w.Write(Metadata.Lock);
                w.Write(Metadata.ThumbnailFrameIndex);
                w.Write(Encoding.Unicode.GetBytes(Metadata.RootAuthorName.PadRight(11, '\0')));
                w.Write(Encoding.Unicode.GetBytes(Metadata.ParentAuthorName.PadRight(11, '\0')));
                w.Write(Encoding.Unicode.GetBytes(Metadata.CurrentAuthorName.PadRight(11, '\0')));
                w.Write(Metadata.ParentAuthorId);
                w.Write(Metadata.CurrentAuthorId);
                w.Write(Metadata.ParentFilename);
                w.Write(Metadata.CurrentFilename);
                w.Write(Metadata.RootAuthorId);
                w.Write(Metadata.RootFileFragment);
                w.Write(Metadata.Timestamp);
                w.Write((ushort)0); //0x009E
                w.Write(RawThumbnail);

                w.Write(AnimationHeader.FrameOffsetTableSize);
                w.Write((ushort)0); // 0x06A2
                w.Write(AnimationHeader.Flags);

                // Calculate frame offsets & write frame data
                List<byte[]> lst = new List<byte[]>();
                uint offset = 0;
                for (int i = 0; i < Frames.Length; i++)
                {
                    lst.Add(Frames[i].ToByteArray());
                    w.Write(offset);
                    offset += (uint)lst[i].Length;
                }

                for (int i = 0; i < Frames.Length; i++)
                {
                    w.Write(lst[i]);
                }

                //w.Write(new byte[(4 - w.BaseStream.Position % 4) % 4]);

                // Write sound data
                for (int i = 0; i < Frames.Length; i++)
                    w.Write((byte)0);

                if (AllignSize != 4)
                    w.Write(new byte[AllignSize]);

                // make the next offset dividable by 4;
                w.Write(SoundData.RawBGM.Length); // BGM
                w.Write((uint)0); // SE1
                w.Write((uint)0); // SE2
                w.Write((uint)0); // SE3
                //w.Write(new byte[(4 - w.BaseStream.Position % 4) % 4]);
                w.Write(SoundHeader.CurrentFramespeed); // Frame speed
                w.Write(SoundHeader.RecordingBGMFramespeed); //BGM speed
                w.Write(new byte[14]);


                //write the actual BGM
                w.Write(SoundData.RawBGM);


                using (var ms = new MemoryStream())
                {
                    var p = w.BaseStream.Position;
                    w.BaseStream.Seek(0, SeekOrigin.Begin);
                    w.BaseStream.CopyTo(ms);
                    w.BaseStream.Seek(p, SeekOrigin.Begin);
                    //sign if you can
                    if (File.Exists("fnkey.pem"))
                    {
                        try
                        {
                            w.Write(ComputeSignature(ms.ToArray()));
                            Signed = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message + "\n" + e.StackTrace);
                            Signed = false;
                        }


                    }
                    else
                    {
                        //placeholder key
                        w.Write(new byte[0x80]);
                        Signed = false;
                    }
                }
                w.Write(new byte[0x10]);

            }
        }

        static readonly string checksumDict = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static char FilenameChecksumDigit(string filename)
        {
            int sumc = Convert.ToInt32("" + filename[0] + filename[1], 16);
            for (int i = 1; i < 16; i++)
            {
                sumc = (sumc + (int)filename[i]) % 256;
            }
            return checksumDict[sumc % 36];
        }

        /// <summary>
        /// Generates the RSA SHA-1 signature for the file data passed as parameter
        /// </summary>
        /// <remarks>
        /// The private key is not contained in this package. Good luck in googling 
        /// it by yourself. Once you have it, place it in a file named "fnkey.pem"
        /// in the root directory.
        /// </remarks>
        /// <param name="data">The PPM binary data</param>      
        /// <returns>a 144-sized byte array.</returns>
        public static byte[] ComputeSignature(byte[] data)
        {
            var privkey = File.ReadAllText("fnkey.pem")
                .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                .Replace("-----END RSA PRIVATE KEY-----", "")
                .Replace(System.Environment.NewLine, "");
            var rsa = CreateRsaProviderFromPrivateKey(privkey);
            var hash = new SHA1CryptoServiceProvider().ComputeHash(data);
            return rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
        }

        // https://stackoverflow.com/questions/14644926/use-pem-encoded-rsa-private-key-in-net                     
        private static RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privkeybytes = Convert.FromBase64String(privateKey);
            var rsa = new RSACryptoServiceProvider();
            var RSAparams = new RSAParameters();
            using (BinaryReader r = new BinaryReader(new MemoryStream(privkeybytes)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = r.ReadUInt16();
                if (twobytes == 0x8130)
                    r.ReadByte();
                else if (twobytes == 0x8230)
                    r.ReadInt16();
                else
                    throw new Exception("Unexpected format");

                twobytes = r.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = r.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected format");

                RSAparams.Modulus = r.ReadBytes(GetIntegerSize(r));
                RSAparams.Exponent = r.ReadBytes(GetIntegerSize(r));
                RSAparams.D = r.ReadBytes(GetIntegerSize(r));
                RSAparams.P = r.ReadBytes(GetIntegerSize(r));
                RSAparams.Q = r.ReadBytes(GetIntegerSize(r));
                RSAparams.DP = r.ReadBytes(GetIntegerSize(r));
                RSAparams.DQ = r.ReadBytes(GetIntegerSize(r));
                RSAparams.InverseQ = r.ReadBytes(GetIntegerSize(r));
            }

            rsa.ImportParameters(RSAparams);
            return rsa;
        }

        private static int GetIntegerSize(BinaryReader r)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = r.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = r.ReadByte();
            if (bt == 0x81)
                count = r.ReadByte();
            else
                if (bt == 0x82)
            {
                highbyte = r.ReadByte();
                lowbyte = r.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
                count = bt;
            while (r.ReadByte() == 0x00)
                count--;
            r.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        /// <summary>
        /// Flipnote file metadata.
        /// </summary>
        public class _Metadata
        {
            public ushort Lock;
            public ushort ThumbnailFrameIndex;
            public string RootAuthorName;
            public string ParentAuthorName;
            public string CurrentAuthorName;
            public byte[] ParentAuthorId;
            public byte[] CurrentAuthorId;
            public byte[] ParentFilename;
            public byte[] CurrentFilename;
            public byte[] RootAuthorId;
            public byte[] RootFileFragment;
            public uint Timestamp; // aka # of seconds passed since 2020/01/01
            public ushort _0x9E;   // unused                     

            /// <summary>
            /// Get the actual date from Timestamp.
            /// </summary>
            public DateTime Date
            {
                get => new DateTime(2000, 1, 1).AddSeconds(Timestamp);
            }
        }

        public class _AnimationHeader
        {
            public ushort FrameOffsetTableSize;
            public ushort _06A2; // 0
            public uint Flags;  // ???
            public uint[] Offsets;
        }

        /// <summary>
        /// Flipnote animation frame data. This is a low-level data linker
        /// for filesystem read/write operations. At application level, 
        /// DecodedFrame is used while editing the flipnotes.
        /// </summary>        
        public class _FrameData
        {
            public long StreamPosition;
            public long AnimationIndex;
            public byte FirstByteHeader;
            public byte[] Layer1LineEncoding = new byte[48];
            public byte[] Layer2LineEncoding = new byte[48];

            public bool[,] Layer1 = new bool[192, 256];
            public bool[,] Layer2 = new bool[192, 256];

            public int TranslateX = 0;
            public int TranslateY = 0;

            public System.Windows.Media.Color PaperColor
            {
                get => (FirstByteHeader & 1) == 1 ? Colors.White : Colors.Black;
            }

            public System.Windows.Media.Color Frame1Color
            {
                get
                {
                    int flag1 = (FirstByteHeader & 0b00000110) >> 1;
                    if (flag1 == 2) return Colors.Red;
                    if (flag1 == 3) return Colors.Blue;
                    if (flag1 == 1)
                        return (FirstByteHeader & 1) == 1 ? Colors.Black : Colors.White;
                    return Colors.Transparent;
                }
            }

            public System.Windows.Media.Color Frame2Color
            {
                get
                {
                    int flag2 = (FirstByteHeader & 0b00011000) >> 3;
                    if (flag2 == 2) return Colors.Red;
                    if (flag2 == 3) return Colors.Blue;
                    if (flag2 == 1)
                        return (FirstByteHeader & 1) == 1 ? Colors.Black : Colors.White;
                    return Colors.Transparent;
                }
            }

            public LineEncoding GetLineEncoding1(int index)
            {
                int _byte = Layer1LineEncoding[index >> 2];
                int pos = (index & 0x3) * 2;
                return (LineEncoding)((_byte >> pos) & 0x3);
            }

            public LineEncoding GetLineEncoding2(int index)
            {
                int _byte = Layer2LineEncoding[index >> 2];
                int pos = (index & 0x3) * 2;
                return (LineEncoding)((_byte >> pos) & 0x3);
            }

            /// <summary>
            /// Helper for editing the Layer`X`LineEncoding individual line compression data.
            /// </summary>
            /// <param name="target">Could be Layer1LineEncoding or Layer2LineEncoding</param>
            /// <param name="index">Index of line whose compression to be set</param>
            /// <param name="value">Compression type (could be 0, 1, 2 or 3)</param>
            private void SetLineEncoding(byte[] target, int index, int value)
            {
                int o = index >> 2;
                int pos = (index & 0x3) * 2;
                var b = target[o];
                b &= (byte)~(0x3 << pos);
                b |= (byte)(value << pos);
                target[o] = b;
            }

            /// <summary>
            /// Builds the current frame by writing the current data over the previous frame data.           
            /// </summary>
            /// <remarks>Used if frame diffing is enabled.</remarks>
            /// <param name="frame">The previous frame</param>
            public void Overwrite(_FrameData frame)
            {
                if ((FirstByteHeader & 0b10000000) != 0)
                {
                    return;
                }
                for (int y = 0; y < 192; y++)
                {
                    if (y - TranslateY < 0) continue;
                    if (y - TranslateY >= 192) break;
                    for (int x = 0; x < 256; x++)
                    {
                        if (x - TranslateX < 0) continue;
                        if (x - TranslateX >= 256) break;
                        Layer1[y, x] ^= frame.Layer1[y - TranslateY, x - TranslateX];
                        Layer2[y, x] ^= frame.Layer2[y - TranslateY, x - TranslateX];
                    }
                }
            }

            /// <summary>
            /// Converts frame data to flipnote PPM-format binary
            /// </summary>
            /// <returns>
            /// A byte array containing animation frame data as should
            /// be existent in a PPM file
            /// </returns>
            public byte[] ToByteArray()
            {
                var res = new List<byte>();
                res.Add(FirstByteHeader);
                for (int l = 0; l < 192; l++)
                {
                    SetLineEncoding(Layer1LineEncoding, l, L1ChooseLineEncoding(l));
                    SetLineEncoding(Layer2LineEncoding, l, L2ChooseLineEncoding(l));
                }
                res.AddRange(Layer1LineEncoding);
                res.AddRange(Layer2LineEncoding);
                for (int l = 0; l < 192; L1PutLine(res, l++)) ;
                for (int l = 0; l < 192; L2PutLine(res, l++)) ;
                return res.ToArray();
            }

            /// <summary>
            /// Choose the encoding for a Layer 1 line
            /// </summary>
            /// <param name="l">Line number</param>
            /// <returns></returns>
            private int L1ChooseLineEncoding(int l)
            {
                // count the 0-filled & 1-filled 8-bit chuncks
                int _0chks = 0, _1chks = 0;
                for (int i = 0; i < 32; i++)
                {
                    int c = 8 * i, n0 = 0, n1 = 0;
                    for (int j = 0; j < 8; j++)
                        if (Layer1[l, c + j])
                            n1++;
                        else
                            n0++;
                    _0chks += n0 == 8 ? 1 : 0;
                    _1chks += n1 == 8 ? 1 : 0;
                }
                // no line data => compression type 0
                if (_0chks == 32)
                    return 0;
                // no chuncks of any type => compression type 3
                if (_0chks == 0 && _1chks == 0)
                    return 3;
                // choose between compression types 1 and 2
                return _0chks > _1chks ? 1 : 2;
            }

            /// <summary>
            /// Choose the encoding for a Layer 2 line
            /// </summary>
            /// <param name="l">Line number</param>
            /// <returns></returns>
            private int L2ChooseLineEncoding(int l)
            {
                // count the 0-filled & 1-filled 8-bit chuncks
                int _0chks = 0, _1chks = 0;
                for (int i = 0; i < 32; i++)
                {
                    int c = 8 * i, n0 = 0, n1 = 0;
                    for (int j = 0; j < 8; j++)
                        if (Layer2[l, c + j])
                            n1++;
                        else
                            n0++;
                    _0chks += n0 == 8 ? 1 : 0;
                    _1chks += n1 == 8 ? 1 : 0;
                }
                // no line data => compression type 0
                if (_0chks == 32)
                    return 0;
                // no chuncks of any type => compression type 3
                if (_0chks == 0 && _1chks == 0)
                    return 3;
                // choose between compression types 1 and 2
                return _0chks > _1chks ? 1 : 2;
            }

            private void L1PutLine(List<byte> lst, int ln)
            {
                int compr = (int)GetLineEncoding1(ln);
                if (compr == 0) return;
                if (compr == 1)
                {
                    var chks = new List<byte>();
                    uint flag = 0;
                    for (int i = 0; i < 32; i++)
                    {
                        byte chunk = 0;
                        for (int j = 0; j < 8; j++)
                            if (Layer1[ln, 8 * i + j])
                                chunk |= (byte)(1 << j);
                        if (chunk != 0x00)
                        {
                            flag |= (1u << (31 - i));
                            chks.Add(chunk);
                        }
                    }
                    lst.Add((byte)((flag & 0xFF000000u) >> 24));
                    lst.Add((byte)((flag & 0x00FF0000u) >> 16));
                    lst.Add((byte)((flag & 0x0000FF00u) >> 8));
                    lst.Add((byte)(flag & 0x000000FFu));
                    lst.AddRange(chks);
                    return;
                }
                if (compr == 2)
                {
                    var chks = new List<byte>();
                    uint flag = 0;
                    for (int i = 0; i < 32; i++)
                    {
                        byte chunk = 0;
                        for (int j = 0; j < 8; j++)
                            if (Layer1[ln, 8 * i + j])
                                chunk |= (byte)(1 << j);
                        if (chunk != 0xFF)
                        {
                            flag |= (1u << (31 - i));
                            chks.Add(chunk);
                        }
                    }
                    lst.Add((byte)((flag & 0xFF000000u) >> 24));
                    lst.Add((byte)((flag & 0x00FF0000u) >> 16));
                    lst.Add((byte)((flag & 0x0000FF00u) >> 8));
                    lst.Add((byte)(flag & 0x000000FFu));
                    lst.AddRange(chks);
                    return;
                }
                if (compr == 3)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        byte chunk = 0;
                        for (int j = 0; j < 8; j++)
                        {
                            if (Layer1[ln, 8 * i + j])
                                chunk |= (byte)(1 << j);
                        }
                        lst.Add(chunk);
                    }
                    return;
                }
            }

            private void L2PutLine(List<byte> lst, int ln)
            {
                int compr = (int)GetLineEncoding2(ln);
                if (compr == 0) return;
                if (compr == 1)
                {
                    var chks = new List<byte>();
                    uint flag = 0;
                    for (int i = 0; i < 32; i++)
                    {
                        byte chunk = 0;
                        for (int j = 0; j < 8; j++)
                            if (Layer2[ln, 8 * i + j])
                                chunk |= (byte)(1 << j);
                        if (chunk != 0x00)
                        {
                            flag |= (1u << (31 - i));
                            chks.Add(chunk);
                        }
                    }
                    lst.Add((byte)((flag & 0xFF000000u) >> 24));
                    lst.Add((byte)((flag & 0x00FF0000u) >> 16));
                    lst.Add((byte)((flag & 0x0000FF00u) >> 8));
                    lst.Add((byte)(flag & 0x000000FFu));
                    lst.AddRange(chks);
                    return;
                }
                if (compr == 2)
                {
                    var chks = new List<byte>();
                    uint flag = 0;
                    for (int i = 0; i < 32; i++)
                    {
                        byte chunk = 0;
                        for (int j = 0; j < 8; j++)
                            if (Layer2[ln, 8 * i + j])
                                chunk |= (byte)(1 << j);
                        if (chunk != 0xFF)
                        {
                            flag |= (1u << (31 - i));
                            chks.Add(chunk);
                        }
                    }
                    lst.Add((byte)((flag & 0xFF000000u) >> 24));
                    lst.Add((byte)((flag & 0x00FF0000u) >> 16));
                    lst.Add((byte)((flag & 0x0000FF00u) >> 8));
                    lst.Add((byte)(flag & 0x000000FFu));
                    lst.AddRange(chks);
                    return;
                }
                if (compr == 3)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        byte chunk = 0;
                        for (int j = 0; j < 8; j++)
                        {
                            if (Layer2[ln, 8 * i + j])
                                chunk |= (byte)(1 << j);
                        }
                        lst.Add(chunk);
                    }
                    return;
                }
            }
        } // _FrameData

        public class _SoundHeader
        {
            public uint BGMTrackSize;
            public uint SE1TrackSize;
            public uint SE2TrackSize;
            public uint SE3TrackSize;
            public byte CurrentFramespeed;
            public byte RecordingBGMFramespeed;
        }

        public class _SoundData
        {
            public byte[] RawBGM;
            public byte[] RawSE1;
            public byte[] RawSE2;
            public byte[] RawSE3;
        }

        public enum LineEncoding
        {
            SkipLine = 0,
            CodedLine = 1,
            InvertedCodedLine = 2,
            RawLineData = 3
        }

        public int FrameSpeed
        {
            get => FrameSpeedToMs[SoundHeader.CurrentFramespeed];
        }

        /// <summary>
        /// Class helper to convert the 1-8 frame speed numbering system to milliseconds
        /// </summary>
        public static _PlaybackSpeed FrameSpeedToMs = new _PlaybackSpeed();
        public class _PlaybackSpeed
        {
            public int this[int i]
            {
                get
                {
                    switch (8 - i)
                    {
                        case 1: return 2000;
                        case 2: return 1000;
                        case 3: return 500;
                        case 4: return 250;
                        case 5: return 166;
                        case 6: return 83;
                        case 7: return 50;
                        case 8: return 33;
                        default: return 33;
                    }
                }
            }
        }
    }
}
