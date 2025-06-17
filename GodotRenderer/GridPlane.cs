using System.Collections.Generic;
using Godot;
using mtw3dviewer.FileFormats;

namespace mtw3dviewer.GodotRenderer
{
    public partial class GridPlane : MeshInstance3D
    {
        public override void _Ready()
        {
            base._Ready();
        }

        public void Create(GridNode node, Jjm map)
        {
            this.Mesh = new ArrayMesh();
            AddSurface(node, map, this.Mesh as ArrayMesh);
        }
        private void AddSurface(GridNode node, Jjm map, ArrayMesh mesh)
        {
            // Edge nodes
            if (node.Column >= map.MainNodes.GetLength(0) - 1 || node.Row >= map.MainNodes.GetLength(1) - 1)
                return;

            Godot.Collections.Array surfaceArray = [];
            surfaceArray.Resize((int)Mesh.ArrayType.Max);
            var verts = new List<Vector3>();
            var indices = new List<int> { 2, 1, 0, 2, 3, 1 };
            var index = 0;

            for (int y = node.Row; y <= node.Row + 1; y++)
            {
                for (int x = node.Column; x <= node.Column + 1; x++)
                {
                    var v = map.Vertices[y * map.MainNodes.GetLength(1) + x];
                    verts.Add(new Vector3(v.X, v.Y, v.Z));
                    indices.AddRange(new List<int>
                    {

                    });

                    //for (int d = 4; d < map.Divisions; d++)

                    // if (node.Column < map.Nodes.GetLength(0) - 1 && node.Row < map.Nodes.GetLength(1) - 1)
                    {
                        // var v = map.Nodes[x, y];
                        //verts.Add(new Vector3(v.X, v.Height * -1, v.Y));


                        /*indices.Add(index); // kolmio1
                        indices.Add(index + 1);
                        indices.Add(index + map.Nodes.GetLength(0));

                        indices.Add(index + 1); // kolmio2
                        indices.Add(index + map.Nodes.GetLength(0) + 1);
                        indices.Add(index + map.Nodes.GetLength(0));*/
                    }
                    //index++;
                }

            }
            // 2048 
            for (int d = 1; d < map.Divisions; d++)
            {
                var offset = map.MainNodes.GetLength(0) * map.MainNodes.GetLength(1) + d - 1;
                var nd = map.AllNodes[offset];
                var ndd = map.Nodez[d, node.Column, node.Row];
                var offf = node.Row * map.MainNodes.GetLength(1) + node.Column * (map.Divisions - 1) + offset;
                var v = map.Vertices[offf];
                var vv = new Vertex { X = ndd.X, Y = ndd.Height * -1, Z = ndd.Y };


                var c = map.Cords[new System.Tuple<uint, uint>(ndd.X, ndd.Y)];
                if (c == null)
                    throw new System.Exception();
                //verts.Add(new Vector3(v.X, v.Y, v.Z));
                //verts.Add(new Vector3(vv.X + 2048*0.5f, vv.Y, vv.Z+ 2048*0.5f));
                verts.Add(new Vector3(vv.X, vv.Y, vv.Z));
                // verts.Add(new Vector3(vv.X + vv.X * 0.5f, vv.Y, vv.Z + vv.Y * 0.5f));
                indices.AddRange(new List<int>
                {

                });
            }
            surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
            //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
            //surfaceArray[(int)Mesh.ArrayType.Normal] = normals.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
            //surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Points, surfaceArray);
            mesh.SurfaceSetMaterial(0, new StandardMaterial3D());
            var m = mesh.SurfaceGetMaterial(0) as StandardMaterial3D;
            m.PointSize = 8;
            m.UsePointSize = true;

        }
    }
}