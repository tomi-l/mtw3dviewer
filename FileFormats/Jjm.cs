using System;
using System.Collections.Generic;
using System.IO;

namespace mtw3dviewer.FileFormats
{
    public struct GridNode
    {
        public uint X { get; set; }
        public int Height { get; set; }
        public uint Y { get; set; }

        public readonly int Row => (int)Y / 2048;
        public readonly int Column => (int)X / 2048;

        public Vertex AsVertex()
        {
            return new Vertex { X = X, Y = Height *-1, Z = Y };
        }

        public static GridNode Parse(BinaryReader reader)
        {
            return new GridNode
            {
                X = reader.ReadUInt32(),
                Height = reader.ReadInt32(),
                Y = reader.ReadUInt32(),
            };
        }
    }
    public class Jjm
    {
        public ushort Dimensions { get; private set; }
        public ushort Divisions { get; private set; }
        public uint TerrainSizeInBytes { get; private set; }
        private byte[] _data;
        public GridNode[,] MainNodes;
        public List<Vertex> Vertices { get; private set; } = new List<Vertex>();
        public GridNode[,,] Nodes { get; private set; }
        public void LoadMap(string path)
        {
            _data = File.ReadAllBytes(path);
            using (var stream = new MemoryStream(_data))
            {
                BinaryReader reader = new BinaryReader(stream);
                Dimensions = reader.ReadUInt16();
                Divisions = reader.ReadUInt16();
                TerrainSizeInBytes = reader.ReadUInt32();
                reader.ReadByte(); // Empty
                MainNodes = new GridNode[Dimensions + 1, Dimensions + 1];
                Nodes = new GridNode[Divisions, Dimensions + 1, Dimensions + 1];

                for (int y = 0; y < MainNodes.GetLength(1); y++)
                {
                    for (int x = 0; x < MainNodes.GetLength(0); x++)
                    {
                        MainNodes[x, y] = GridNode.Parse(reader);
                        var node = MainNodes[x, y];
                        Vertices.Add(new Vertex { X = node.X, Y = node.Height * -1, Z = node.Y });
                        Nodes[0, x, y] = node;
                    }
                }
                for (int y = 0; y < MainNodes.GetLength(1); y++)
                {
                    for (int x = 0; x < MainNodes.GetLength(0); x++)
                    {
                        for (int d = 1; d < Divisions; d++)
                        {
                            var node = GridNode.Parse(reader);
                            Vertices.Add(new Vertex { X = node.X, Y = node.Height * -1, Z = node.Y });
                            Nodes[d, x, y] = node;
                        }
                    }
                }
            }
        }
    }
}