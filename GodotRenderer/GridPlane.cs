using System.Collections.Generic;
using System.Linq;
using Godot;
using mtw3dviewer.FileFormats;

namespace mtw3dviewer.GodotRenderer
{
    public partial class GridPlane : MeshInstance3D
    {
        private readonly int MIDPOINT = 2048 / 2;
        public override void _Ready()
        {
            base._Ready();
        }

        public void Create(GridNode node, Jjm map, Texture2D texture)
        {
            this.Mesh = new ArrayMesh();
            AddSurface(node, map, this.Mesh as ArrayMesh, texture);
        }
        private List<Vertex> Quad(GridNode node, Jjm map, ArrayMesh mesh, out List<int> indices, out List<Vector2> uvs)
        {
            indices = new();
            List<Vertex> vs = new();
            var v = map.Nodes[0, node.Column, node.Row].AsVertex(); // Top left
            var right = map.Nodes[0, node.Column + 1, node.Row].AsVertex(); // Top right
            var diagonal = map.Nodes[0, node.Column + 1, node.Row + 1].AsVertex(); // Bottom right
            var down = map.Nodes[0, node.Column, node.Row + 1].AsVertex(); // Bottom left

            vs.Add(v);
            vs.Add(new Vertex { X = v.X + MIDPOINT, Y = (v.Y + right.Y) * 0.5f, Z = v.Z }); // avg of top left and top right
            vs.Add(right);

            vs.Add(new Vertex { X = right.X, Y = (diagonal.Y + right.Y) * 0.5f, Z = right.Z - MIDPOINT });  // avg of top right and bottom right
            vs.Add(diagonal);

            vs.Add(new Vertex { X = diagonal.X - MIDPOINT, Y = (diagonal.Y + down.Y) * 0.5f, Z = diagonal.Z }); // avg of bottom right and bottom left
            vs.Add(down);
            vs.Add(new Vertex { X = down.X, Y = (v.Y + down.Y) * 0.5f, Z = down.Z + MIDPOINT }); // avg of bottom right and top left

            // Build inner vertices from control nodes
            v = map.Nodes[0, node.Column, node.Row].AsVertex();
            var controlNode = map.Nodes[4, node.Column, node.Row].AsVertex(); // 4th control node of current main node
            vs.Add(new Vertex { X = v.X + MIDPOINT / 2, Y = controlNode.Y, Z = v.Z - MIDPOINT / 2 });
            controlNode = map.Nodes[3, node.Column + 1, node.Row].AsVertex(); // 3rd control node of right neighbour node
            vs.Add(new Vertex { X = v.X + MIDPOINT * 1.5f, Y = controlNode.Y, Z = v.Z - MIDPOINT / 2 });
            controlNode = map.Nodes[1, node.Column + 1, node.Row + 1].AsVertex(); // 1st control node of diagonal neighbour node
            vs.Add(new Vertex { X = v.X + MIDPOINT * 1.5f, Y = controlNode.Y, Z = v.Z - MIDPOINT * 1.5f });
            controlNode = map.Nodes[2, node.Column, node.Row + 1].AsVertex(); // 2nd control node of down neighbour node
            vs.Add(new Vertex { X = v.X + MIDPOINT / 2, Y = controlNode.Y, Z = v.Z - MIDPOINT * 1.5f });

            indices = new()
            {
                0,7,8,
                8,7,11,
                7,6,11,
                6,5,11,
                5,10,11,
                5,4,10,
                4,3,10,
                3,9,10,
                3,2,9,
                2,1,9,
                1,8,9,
                1,0,8,

                8,11,10,
                10,9,8,
            };

            uvs =
            [
                new Vector2(0, 0),
                new Vector2(0.5f, 0),
                new Vector2(1, 0),
                new Vector2(1, 0.5f),
                new Vector2(1, 1),
                new Vector2(0.5f, 1),
                new Vector2(0, 1),
                new Vector2(0, 0.5f),
                new Vector2(0.25f, 0.25f),
                new Vector2(0.75f, 0.25f),
                new Vector2(0.75f,0.75f),
                new Vector2(0.25f, 0.75f),
            ];
            return vs;
        }
        private void AddSurface(GridNode node, Jjm map, ArrayMesh mesh, Texture2D texture)
        {
            // Edge nodes
            if (node.Column >= map.MainNodes.GetLength(0) - 1 || node.Row >= map.MainNodes.GetLength(1) - 1)
                return;

            Godot.Collections.Array surfaceArray = [];
            surfaceArray.Resize((int)Mesh.ArrayType.Max);

            var verts = Quad(node, map, mesh, out var indices, out var uvs).Select(x => new Vector3(x.X, x.Y, x.Z));

            surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
            surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
            //surfaceArray[(int)Mesh.ArrayType.Normal] = normals.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
            surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

            var material = new StandardMaterial3D();
            material.AlbedoTexture = texture;
            mesh.SurfaceSetMaterial(0, material);
            // var m = mesh.SurfaceGetMaterial(0) as StandardMaterial3D;
            // m.PointSize = 8;
            //m.UsePointSize = true;
        }
    }
}