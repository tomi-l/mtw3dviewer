using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mtw3dviewer.FileFormats
{
    public struct Vertex
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Vertex Parse(BinaryReader reader)
        {
            return new Vertex
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),
            };
        }
        public override string ToString()
        {
            return $"X:{X} Y:{Y} Z:{Z}";
        }
    }

    /// <summary>
    /// .3xx File format
    /// </summary>
    public class Model3xx
    {
        public struct Uv
        {
            public int VertexIndex { get; set; }
            public int NormalIndex { get; set; }
            public float X { get; set; }
            public float Y { get; set; }

            public static Uv Parse(BinaryReader reader)
            {
                return new Uv
                {
                    VertexIndex = reader.ReadInt32(),
                    NormalIndex = reader.ReadInt32(),
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle()
                };
            }
        }
        public struct Triangle
        {
            public int TextureIndex { get; set; }
            public Uv[] Uvs => _uvs.Reverse().ToArray();
            private Uv[] _uvs;
            public List<int> Indices => Uvs.Select(x => x.VertexIndex).ToList();

            public Triangle()
            {
                _uvs = new Uv[3];
            }
            public static Triangle Parse(BinaryReader reader)
            {
                return new Triangle
                {
                    TextureIndex = reader.ReadInt32(),
                    _uvs =
                    [
                        Uv.Parse(reader),
                        Uv.Parse(reader),
                        Uv.Parse(reader),
                    ]
                };
            }
        }
        private byte[] _data;
        private List<Vertex> _verticesTable = new();
        private List<Vertex> _normalsTable = new();
        public string FileName { get; private set; }
        public int NumVertices { get; private set; }
        public int NumTriangles { get; private set; }
        public int NumNormals { get; private set; }
        public List<Triangle> Triangles { get; private set; } = new();
        /// <summary>
        /// Texture Ids used in the model
        /// </summary>
        public List<int> TextureIds => Triangles.Select(x => x.TextureIndex).Distinct().ToList();
        /// <summary>
        /// Texture names used in a model defined in .txx file
        /// </summary>
        public List<string> Textures { get; private set; }
        /// <summary>
        /// All vertices of the model
        /// </summary>
        public IEnumerable<Vertex> Vertices
        {
            get
            {
                return VerticesBy(x => true);
            }
        }
        /// <summary>
        /// All indices of the model
        /// </summary>
        public IEnumerable<int> Indices
        {
            get
            {
                return IndicesBy(x => true);
            }
        }
        /// <summary>
        /// All normals of the model
        /// </summary>
        public IEnumerable<Vertex> Normals
        {
            get
            {
                return NormalsBy(x => true);
            }
        }
        /// <summary>
        /// Loads .3xx file
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadModel(string fileName)
        {
            FileName = fileName;
            _data = File.ReadAllBytes(fileName);
            using (var stream = new MemoryStream(_data))
            {
                BinaryReader reader = new BinaryReader(stream);
                // Read number of vertices
                NumVertices = reader.ReadInt32();
                // Parse vertices
                for (int i = 0; i < NumVertices; i++)
                {
                    _verticesTable.Add(Vertex.Parse(reader));
                }
                // Read number of normals
                NumNormals = reader.ReadInt32();
                // Parse normals
                for (int i = 0; i < NumNormals; i++)
                {
                    _normalsTable.Add(Vertex.Parse(reader));
                }
                // Read number of triangles
                NumTriangles = reader.ReadInt32();
                // Parse triangles
                for (int i = 0; i < NumTriangles; i++)
                {
                    Triangles.Add(Triangle.Parse(reader));
                }
            }
            // Read textures from .txx file
            LoadTextures();
        }
        /// <summary>
        /// Filter vertices by triangle
        /// </summary>
        /// <param name="where">Predicate</param>
        public IEnumerable<Vertex> VerticesBy(Func<Triangle, bool> where)
        {
            foreach (var t in Triangles.Where(where))
            {
                foreach (var u in t.Uvs)
                {
                    yield return _verticesTable[u.VertexIndex];
                }
            }
        }
        /// <summary>
        /// Filter normals by triangle
        /// </summary>
        /// <param name="where">Predicate</param>
        public IEnumerable<Vertex> NormalsBy(Func<Triangle, bool> where)
        {
            foreach (var t in Triangles.Where(where))
            {
                foreach (var u in t.Uvs)
                {
                    yield return _normalsTable[u.NormalIndex];
                }
            }
        }
        /// <summary>
        /// Filter indices by triangle
        /// </summary>
        /// <param name="where">Predicate</param>
        public IEnumerable<int> IndicesBy(Func<Triangle, bool> where)
        {
            int i = 0;
            foreach (var t in Triangles.Where(where))
            {
                foreach (var u in t.Indices)
                {
                    yield return i++; // we could return u but sequential indices work here
                }
            }
        }

        private void LoadTextures()
        {
            string txx = FileName.Replace(".3xx", ".txx", System.StringComparison.CurrentCultureIgnoreCase);
            string textures = File.ReadAllText(txx);
            Textures = textures.Split(Environment.NewLine).ToList();
        }
    }
}