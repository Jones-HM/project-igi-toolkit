using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

namespace IGIEditor
{
    public class QTerrain
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CTRItem
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public short[] children; // int16_t children_[8]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public sbyte[] cmdTransform; // int8_t cmd_transform_[8]
            public byte childrenMask; // uint8_t children_mask_
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] pad; // uint8_t pad_[3]
            public uint cmdOffset; // uint32_t cmd_offset_
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CMDItem
        {
            public ushort numTriangle;
            public ushort vertexOffset;
            public ushort numParentVertex;
            public ushort numChildVertex;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BITItem
        {
            public uint contents; // runtime pointer in 32bit platform, but in file it might be offset or placeholder
            public byte unk;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] pad;
            public uint size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HMPItem
        {
            public uint contents; // runtime pointer in 32bit platform
            public byte unk;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] pad;
            public uint size;
        }

        public class CTRData
        {
            public List<CTRItem> items = new List<CTRItem>();
        }

        public class BITData
        {
            public List<BITItem> headers = new List<BITItem>();
            public List<byte[]> bitArrays = new List<byte[]>();
        }

        public class HMPData
        {
            public List<HMPItem> headers = new List<HMPItem>();
            public List<float[]> hmpArrays = new List<float[]>();
        }

        public class LMPData
        {
            public List<uint> sizes = new List<uint>();
            public List<byte[]> pixelData = new List<byte[]>();
        }

        public class CMDData
        {
            public List<CMDItem> headers = new List<CMDItem>();
            public List<byte[]> vertexData = new List<byte[]>(); // Raw vertex data
            public List<uint[]> triangleData = new List<uint[]>(); // Triangle indices
        }

        public static CTRData LoadCTR(string filepath)
        {
            CTRData data = new CTRData();
            if (!File.Exists(filepath)) return data;

            byte[] buffer = File.ReadAllBytes(filepath);
            int itemSize = Marshal.SizeOf(typeof(CTRItem));
            int numItems = buffer.Length / itemSize;

            for (int i = 0; i < numItems; i++)
            {
                byte[] itemBuffer = new byte[itemSize];
                Array.Copy(buffer, i * itemSize, itemBuffer, 0, itemSize);
                data.items.Add(ByteArrayToStruct<CTRItem>(itemBuffer));
            }
            return data;
        }

        public static void SaveCTR(string filepath, CTRData data)
        {
            int itemSize = Marshal.SizeOf(typeof(CTRItem));
            byte[] buffer = new byte[data.items.Count * itemSize];

            for (int i = 0; i < data.items.Count; i++)
            {
                byte[] itemBuffer = StructToByteArray(data.items[i]);
                Array.Copy(itemBuffer, 0, buffer, i * itemSize, itemSize);
            }
            File.WriteAllBytes(filepath, buffer);
        }

        public static HMPData LoadHMP(string filepath)
        {
            HMPData data = new HMPData();
            if (!File.Exists(filepath)) return data;

            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        // Check if we have enough bytes for header (4 + 1 + 3 + 4 = 12 bytes)
                        if (reader.BaseStream.Length - reader.BaseStream.Position < 12)
                        {
                            QLog.AddLog("LoadHMP", "Not enough bytes for HMP header at position " + reader.BaseStream.Position);
                            break;
                        }

                        HMPItem header = new HMPItem();
                        header.contents = reader.ReadUInt32();
                        header.unk = reader.ReadByte();
                        header.pad = reader.ReadBytes(3);
                        header.size = reader.ReadUInt32();

                        // Validate size to prevent overflow
                        if (header.size > 65535) // Reasonable maximum size
                        {
                            QLog.AddLog("LoadHMP", "Invalid HMP size: " + header.size + ", skipping entry");
                            continue;
                        }

                        data.headers.Add(header);

                        int numElements = (int)((header.size + 1) * (header.size + 1));
                        int expectedBytes = numElements * 4; // 4 bytes per float

                        // Check if we have enough bytes for the array
                        if (reader.BaseStream.Length - reader.BaseStream.Position < expectedBytes)
                        {
                            QLog.AddLog("LoadHMP", "Not enough bytes for HMP array. Expected: " + expectedBytes + ", Available: " + (reader.BaseStream.Length - reader.BaseStream.Position));
                            break;
                        }

                        float[] hmpArray = new float[numElements];
                        for (int i = 0; i < numElements; i++)
                        {
                            hmpArray[i] = reader.ReadSingle();
                        }
                        data.hmpArrays.Add(hmpArray);
                    }
                }
            }
            catch (Exception ex)
            {
                QLog.LogException("LoadHMP", ex);
                QLog.AddLog("LoadHMP", "Error loading HMP file: " + filepath);
            }
            return data;
        }

        public static void SaveHMP(string filepath, HMPData data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filepath)))
            {
                for (int i = 0; i < data.headers.Count; i++)
                {
                    writer.Write(data.headers[i].contents);
                    writer.Write(data.headers[i].unk);
                    writer.Write(data.headers[i].pad);
                    writer.Write(data.headers[i].size);

                    float[] hmpArray = data.hmpArrays[i];
                    for (int j = 0; j < hmpArray.Length; j++)
                    {
                        writer.Write(hmpArray[j]);
                    }
                }
            }
        }

        public static Bitmap RenderHMP(float[] hmpArray, uint size)
        {
            int imgSize = (int)(size + 1);
            Bitmap bmp = new Bitmap(imgSize, imgSize);
            float min = float.MaxValue;
            float max = float.MinValue;

            foreach (float f in hmpArray)
            {
                if (f < min) min = f;
                if (f > max) max = f;
            }

            float range = max - min;
            if (range == 0) range = 1.0f;

            for (int y = 0; y < imgSize; y++)
            {
                for (int x = 0; x < imgSize; x++)
                {
                    float val = hmpArray[y * imgSize + x];
                    int colorVal = (int)((val - min) / range * 255);
                    colorVal = Math.Max(0, Math.Min(255, colorVal));
                    bmp.SetPixel(x, y, Color.FromArgb(colorVal, colorVal, colorVal));
                }
            }
            return bmp;
        }

        public static Bitmap Render3DWireframe(float[] hmpArray, uint size)
        {
            int gridSize = (int)(size + 1);
            int width = 400, height = 400;
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
                Pen pen = new Pen(Color.Lime, 1);

                float scale = 300.0f / gridSize;
                float heightScale = 0.5f;

                // Simple Isometric Projection
                Func<float, float, float, PointF> project = (x, y, z) => {
                    float px = (x - y) * 0.707f * scale + width / 2;
                    float py = (x + y) * 0.354f * scale - z * heightScale + height / 2;
                    return new PointF(px, py);
                };

                for (int y = 0; y < gridSize; y++)
                {
                    for (int x = 0; x < gridSize; x++)
                    {
                        PointF p1 = project(x, y, hmpArray[y * gridSize + x]);
                        if (x + 1 < gridSize)
                        {
                            PointF p2 = project(x + 1, y, hmpArray[y * gridSize + (x + 1)]);
                            g.DrawLine(pen, p1, p2);
                        }
                        if (y + 1 < gridSize)
                        {
                            PointF p3 = project(x, y + 1, hmpArray[(y + 1) * gridSize + x]);
                            g.DrawLine(pen, p1, p3);
                        }
                    }
                }
            }
            return bmp;
        }

        public static Bitmap RenderLMP(byte[] lmpArray, uint size)
        {
            int imgSize = (int)size;
            if (imgSize == 0 || lmpArray.Length == 0) return null;
            Bitmap bmp = new Bitmap(imgSize, imgSize);
            for (int y = 0; y < imgSize; y++)
            {
                for (int x = 0; x < imgSize; x++)
                {
                    int idx = y * imgSize + x;
                    if (idx < lmpArray.Length)
                    {
                        byte val = lmpArray[idx];
                        bmp.SetPixel(x, y, Color.FromArgb(val, val, val));
                    }
                }
            }
            return bmp;
        }

        public static Bitmap RenderBIT(byte[] bitArray, uint size)
        {
            int imgSize = (int)size;
            Bitmap bmp = new Bitmap(imgSize, imgSize);
            for (int y = 0; y < imgSize; y++)
            {
                for (int x = 0; x < imgSize; x++)
                {
                    byte val = bitArray[y * imgSize + x];
                    bmp.SetPixel(x, y, Color.FromArgb(val, val, val));
                }
            }
            return bmp;
        }

        public static LMPData LoadLMP(string filepath)
        {
            LMPData data = new LMPData();
            if (!File.Exists(filepath)) return data;

            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        // Check if we have enough bytes for size field (4 bytes)
                        if (reader.BaseStream.Length - reader.BaseStream.Position < 4)
                        {
                            QLog.AddLog("LoadLMP", "Not enough bytes for LMP size at position " + reader.BaseStream.Position);
                            break;
                        }

                        uint size = reader.ReadUInt32();

                        // Validate size to prevent overflow
                        if (size > 65535) // Reasonable maximum size
                        {
                            QLog.AddLog("LoadLMP", "Invalid LMP size: " + size + ", skipping entry");
                            continue;
                        }

                        data.sizes.Add(size);

                        // Calculate pixel data size with overflow protection
                        long pixelDataSizeLong = (long)size * (long)size;
                        if (pixelDataSizeLong > int.MaxValue)
                        {
                            QLog.AddLog("LoadLMP", "LMP size too large: " + pixelDataSizeLong + ", skipping entry");
                            continue;
                        }

                        int pixelDataSize = (int)pixelDataSizeLong;

                        // Check if we have enough bytes for the pixel data
                        if (reader.BaseStream.Length - reader.BaseStream.Position < pixelDataSize)
                        {
                            QLog.AddLog("LoadLMP", "Not enough bytes for LMP pixel data. Expected: " + pixelDataSize + ", Available: " + (reader.BaseStream.Length - reader.BaseStream.Position));
                            break;
                        }

                        data.pixelData.Add(reader.ReadBytes(pixelDataSize));
                    }
                }
            }
            catch (Exception ex)
            {
                QLog.LogException("LoadLMP", ex);
                QLog.AddLog("LoadLMP", "Error loading LMP file: " + filepath);
            }
            return data;
        }

        public static void SaveLMP(string filepath, LMPData data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filepath)))
            {
                for (int i = 0; i < data.sizes.Count; i++)
                {
                    writer.Write(data.sizes[i]);
                    writer.Write(data.pixelData[i]);
                }
            }
        }

        public static BITData LoadBIT(string filepath)
        {
            BITData data = new BITData();
            if (!File.Exists(filepath)) return data;

            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        // Check if we have enough bytes for header (4 + 1 + 3 + 4 = 12 bytes)
                        if (reader.BaseStream.Length - reader.BaseStream.Position < 12)
                        {
                            QLog.AddLog("LoadBIT", "Not enough bytes for BIT header at position " + reader.BaseStream.Position);
                            break;
                        }

                        BITItem header = new BITItem();
                        header.contents = reader.ReadUInt32();
                        header.unk = reader.ReadByte();
                        header.pad = reader.ReadBytes(3);
                        header.size = reader.ReadUInt32();

                        // Validate size to prevent overflow
                        if (header.size > 65535) // Reasonable maximum size
                        {
                            QLog.AddLog("LoadBIT", "Invalid BIT size: " + header.size + ", skipping entry");
                            continue;
                        }

                        data.headers.Add(header);

                        // Calculate numElements with overflow protection
                        long numElementsLong = (long)header.size * (long)header.size;
                        if (numElementsLong > int.MaxValue)
                        {
                            QLog.AddLog("LoadBIT", "BIT size too large: " + numElementsLong + ", skipping entry");
                            continue;
                        }

                        int numElements = (int)numElementsLong;

                        // Check if we have enough bytes for the array
                        if (reader.BaseStream.Length - reader.BaseStream.Position < numElements)
                        {
                            QLog.AddLog("LoadBIT", "Not enough bytes for BIT array. Expected: " + numElements + ", Available: " + (reader.BaseStream.Length - reader.BaseStream.Position));
                            break;
                        }

                        data.bitArrays.Add(reader.ReadBytes(numElements));
                    }
                }
            }
            catch (Exception ex)
            {
                QLog.LogException("LoadBIT", ex);
                QLog.AddLog("LoadBIT", "Error loading BIT file: " + filepath);
            }
            return data;
        }

        public static void SaveBIT(string filepath, BITData data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filepath)))
            {
                for (int i = 0; i < data.headers.Count; i++)
                {
                    writer.Write(data.headers[i].contents);
                    writer.Write(data.headers[i].unk);
                    writer.Write(data.headers[i].pad);
                    writer.Write(data.headers[i].size);
                    writer.Write(data.bitArrays[i]);
                }
            }
        }

        private static T ByteArrayToStruct<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        private static byte[] StructToByteArray<T>(T str) where T : struct
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(str, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }
    }
}