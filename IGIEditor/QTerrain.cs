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

            using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    HMPItem header = new HMPItem();
                    header.contents = reader.ReadUInt32();
                    header.unk = reader.ReadByte();
                    header.pad = reader.ReadBytes(3);
                    header.size = reader.ReadUInt32();

                    data.headers.Add(header);

                    int numElements = (int)((header.size + 1) * (header.size + 1));
                    float[] hmpArray = new float[numElements];
                    for (int i = 0; i < numElements; i++)
                    {
                        hmpArray[i] = reader.ReadSingle();
                    }
                    data.hmpArrays.Add(hmpArray);
                }
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

            using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    uint size = reader.ReadUInt32();
                    data.sizes.Add(size);
                    data.pixelData.Add(reader.ReadBytes((int)(size * size)));
                }
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

            using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    BITItem header = new BITItem();
                    header.contents = reader.ReadUInt32();
                    header.unk = reader.ReadByte();
                    header.pad = reader.ReadBytes(3);
                    header.size = reader.ReadUInt32();

                    data.headers.Add(header);

                    int numElements = (int)(header.size * header.size);
                    data.bitArrays.Add(reader.ReadBytes(numElements));
                }
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

        public static CMDData LoadCMD(string filepath)
        {
            CMDData data = new CMDData();
            if (!File.Exists(filepath)) return data;

            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath)))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        // Check if we have enough bytes for header (8 bytes)
                        if (reader.BaseStream.Length - reader.BaseStream.Position < 8)
                        {
                            QLog.AddLog("LoadCMD", "Not enough bytes for CMD header at position " + reader.BaseStream.Position);
                            break;
                        }

                        CMDItem header = new CMDItem();
                        header.numTriangle = reader.ReadUInt16();
                        header.vertexOffset = reader.ReadUInt16();
                        header.numParentVertex = reader.ReadUInt16();
                        header.numChildVertex = reader.ReadUInt16();

                        // Validate values - check for sentinel value and reasonable limits
                        if (header.numTriangle == 65535 || header.numParentVertex == 65535 || header.numChildVertex == 65535)
                        {
                            QLog.AddLog("LoadCMD", "Invalid CMD entry (sentinel value 65535 found), skipping entry");
                            continue;
                        }

                        // Validate vertexOffset consistency
                        if (header.vertexOffset != header.numTriangle * 4)
                        {
                            QLog.AddLog("LoadCMD", "Invalid CMD entry (vertexOffset mismatch), skipping entry");
                            continue;
                        }

                        data.headers.Add(header);

                        // Calculate expected data size
                        // vertexOffset should equal numTriangle * sizeof(uint32_t)
                        int vertexDataSize = header.numParentVertex * 12; // 12 bytes per vertex (3 floats)
                        int triangleDataSize = header.numTriangle * 12; // 12 bytes per triangle (3 uint32)

                        // Read vertex data (parent vertices + child vertices)
                        int totalVertices = header.numParentVertex + header.numChildVertex;
                        if (totalVertices > 0)
                        {
                            int expectedVertexBytes = totalVertices * 12;
                            if (reader.BaseStream.Length - reader.BaseStream.Position >= expectedVertexBytes)
                            {
                                byte[] vertexBytes = reader.ReadBytes(expectedVertexBytes);
                                data.vertexData.Add(vertexBytes);
                            }
                            else
                            {
                                QLog.AddLog("LoadCMD", "Not enough bytes for vertex data. Expected: " + expectedVertexBytes + ", Available: " + (reader.BaseStream.Length - reader.BaseStream.Position));
                                data.vertexData.Add(new byte[0]); // Add empty to maintain index alignment
                            }
                        }
                        else
                        {
                            data.vertexData.Add(new byte[0]);
                        }

                        // Read triangle data
                        if (header.numTriangle > 0)
                        {
                            int expectedTriangleBytes = header.numTriangle * 12;
                            if (reader.BaseStream.Length - reader.BaseStream.Position >= expectedTriangleBytes)
                            {
                                byte[] triangleBytes = reader.ReadBytes(expectedTriangleBytes);
                                // Convert to uint array
                                uint[] triangles = new uint[header.numTriangle * 3];
                                for (int i = 0; i < triangles.Length; i++)
                                {
                                    triangles[i] = BitConverter.ToUInt32(triangleBytes, i * 4);
                                }
                                data.triangleData.Add(triangles);
                            }
                            else
                            {
                                QLog.AddLog("LoadCMD", "Not enough bytes for triangle data. Expected: " + expectedTriangleBytes + ", Available: " + (reader.BaseStream.Length - reader.BaseStream.Position));
                                data.triangleData.Add(new uint[0]); // Add empty to maintain index alignment
                            }
                        }
                        else
                        {
                            data.triangleData.Add(new uint[0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                QLog.LogException("LoadCMD", ex);
                QLog.AddLog("LoadCMD", "Error loading CMD file: " + filepath);
            }
            return data;
        }

        public static void SaveCMD(string filepath, CMDData data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filepath)))
            {
                for (int i = 0; i < data.headers.Count; i++)
                {
                    CMDItem header = data.headers[i];
                    writer.Write(header.numTriangle);
                    writer.Write(header.vertexOffset);
                    writer.Write(header.numParentVertex);
                    writer.Write(header.numChildVertex);

                    // Write vertex data
                    if (i < data.vertexData.Count && data.vertexData[i] != null)
                    {
                        writer.Write(data.vertexData[i]);
                    }

                    // Write triangle data
                    if (i < data.triangleData.Count && data.triangleData[i] != null)
                    {
                        foreach (uint triangleIndex in data.triangleData[i])
                        {
                            writer.Write(triangleIndex);
                        }
                    }
                }
            }
        }

        public static Bitmap RenderCMD(byte[] vertexData, uint[] triangleData, int numVertices)
        {
            // Create a simple wireframe rendering of the CMD mesh
            int width = 512;
            int height = 512;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                if (vertexData == null || vertexData.Length == 0 || triangleData == null || triangleData.Length == 0)
                {
                    // Draw placeholder text
                    using (Font font = new Font("Arial", 12))
                    using (Brush brush = new SolidBrush(Color.White))
                    {
                        g.DrawString("No CMD data to render", font, brush, 10, 10);
                    }
                    return bitmap;
                }

                // Parse vertices (assuming 3 floats per vertex: x, y, z)
                int vertexCount = vertexData.Length / 12; // 12 bytes per vertex (3 floats)
                float[] vertices = new float[vertexCount * 3];
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i * 3] = BitConverter.ToSingle(vertexData, i * 12);
                    vertices[i * 3 + 1] = BitConverter.ToSingle(vertexData, i * 12 + 4);
                    vertices[i * 3 + 2] = BitConverter.ToSingle(vertexData, i * 12 + 8);
                }

                // Find bounds to normalize coordinates
                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;
                for (int i = 0; i < vertexCount; i++)
                {
                    minX = Math.Min(minX, vertices[i * 3]);
                    maxX = Math.Max(maxX, vertices[i * 3]);
                    minY = Math.Min(minY, vertices[i * 3 + 1]);
                    maxY = Math.Max(maxY, vertices[i * 3 + 1]);
                }

                float rangeX = maxX - minX;
                float rangeY = maxY - minY;
                if (rangeX == 0) rangeX = 1;
                if (rangeY == 0) rangeY = 1;

                // Draw triangles
                using (Pen pen = new Pen(Color.LimeGreen, 1))
                {
                    for (int i = 0; i < triangleData.Length; i += 3)
                    {
                        if (i + 2 < triangleData.Length)
                        {
                            uint idx1 = triangleData[i];
                            uint idx2 = triangleData[i + 1];
                            uint idx3 = triangleData[i + 2];

                            if (idx1 < vertexCount && idx2 < vertexCount && idx3 < vertexCount)
                            {
                                PointF p1 = new PointF(
                                    (vertices[idx1 * 3] - minX) / rangeX * (width - 40) + 20,
                                    (vertices[idx1 * 3 + 1] - minY) / rangeY * (height - 40) + 20
                                );
                                PointF p2 = new PointF(
                                    (vertices[idx2 * 3] - minX) / rangeX * (width - 40) + 20,
                                    (vertices[idx2 * 3 + 1] - minY) / rangeY * (height - 40) + 20
                                );
                                PointF p3 = new PointF(
                                    (vertices[idx3 * 3] - minX) / rangeX * (width - 40) + 20,
                                    (vertices[idx3 * 3 + 1] - minY) / rangeY * (height - 40) + 20
                                );

                                g.DrawLine(pen, p1, p2);
                                g.DrawLine(pen, p2, p3);
                                g.DrawLine(pen, p3, p1);
                            }
                        }
                    }
                }

                // Draw info text
                using (Font font = new Font("Arial", 10))
                using (Brush brush = new SolidBrush(Color.White))
                {
                    g.DrawString($"Vertices: {vertexCount}", font, brush, 10, 10);
                    g.DrawString($"Triangles: {triangleData.Length / 3}", font, brush, 10, 25);
                }
            }
            return bitmap;
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
