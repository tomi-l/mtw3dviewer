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

        public int Row { get; private set; }
        public int Column { get; private set; }

        public static GridNode Parse(int col, int row, BinaryReader reader)
        {
            return new GridNode
            {
                X = reader.ReadUInt32(),
                Height = reader.ReadInt32(),
                Y = reader.ReadUInt32(),
                Row = row,
                Column = col
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
        public List<GridNode> AllNodes { get; private set; } = new();
        public List<Vertex> Vertices { get; private set; } = new List<Vertex>();
        public List<int> Indices { get; private set; } = new List<int>();
        public GridNode[,,] Nodez { get; private set; }

        public Dictionary<Tuple<uint, uint>, List<GridNode>> Cords { get; private set; } = new Dictionary<Tuple<uint, uint>, List<GridNode>>();
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
                Nodez = new GridNode[Divisions, Dimensions + 1, Dimensions + 1];
                //Nodes = new GridNode[Dimensions + 1, 2];
                var index = 0;
                for (int y = 0; y < MainNodes.GetLength(1); y++)
                {
                    for (int x = 0; x < MainNodes.GetLength(0); x++)
                    {
                        MainNodes[x, y] = GridNode.Parse(x, y, reader);
                        var node = MainNodes[x, y];
                        Vertices.Add(new Vertex { X = node.X, Y = node.Height * -1, Z = node.Y });

                        if (x < MainNodes.GetLength(0) - 1 && y < MainNodes.GetLength(1) - 1)
                        {
                            Indices.Add(index); // kolmio1
                            Indices.Add(index + 1);
                            Indices.Add(index + MainNodes.GetLength(0));

                            Indices.Add(index + 1); // kolmio2
                            Indices.Add(index + MainNodes.GetLength(0) + 1);
                            Indices.Add(index + MainNodes.GetLength(0));
                        }
                        index++;
                        AllNodes.Add(node);

                        if (!Cords.ContainsKey(new Tuple<uint, uint>(node.X, node.Y)))
                        {
                            Cords[new Tuple<uint, uint>(node.X, node.Y)] = new List<GridNode>();
                        }
                        Cords[new Tuple<uint, uint>(node.X, node.Y)].Add(node);
                    }
                }
                for (int y = 0; y < MainNodes.GetLength(1); y++)
                {
                    for (int x = 0; x < MainNodes.GetLength(0); x++)
                    {
                        for (int d = 1; d < Divisions; d++)
                        {
                            var node = GridNode.Parse(x, y, reader);
                            Vertices.Add(new Vertex { X = node.X, Y = node.Height * -1, Z = node.Y });
                            AllNodes.Add(node);
                            Nodez[d, x, y] = node;

                            if (!Cords.ContainsKey(new Tuple<uint, uint>(node.X, node.Y)))
                            {
                                Cords[new Tuple<uint, uint>(node.X, node.Y)] = new List<GridNode>();
                            }
                            Cords[new Tuple<uint, uint>(node.X, node.Y)].Add(node);
                        }


                        /*if (x < MainNodes.GetLength(0) - 1 && y < MainNodes.GetLength(1) - 1)
                        {
                            Indices.Add(index); // kolmio1
                            Indices.Add(index + 1);
                            Indices.Add(index + MainNodes.GetLength(0));

                            Indices.Add(index + 1); // kolmio2
                            Indices.Add(index + MainNodes.GetLength(0) + 1);
                            Indices.Add(index + MainNodes.GetLength(0));
                        }
                        index++;*/
                    }
                }
            }
        }
    }
}