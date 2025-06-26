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

        public void Create(GridNode node, Jjm map, Texture2D texture, MapTexture mapTexture)
        {
            this.Mesh = new ArrayMesh();
            AddSurface(node, map, this.Mesh as ArrayMesh, texture, mapTexture);
        }
        private List<Vertex> Quad(GridNode node, Jjm map, MapTexture mapTexture, out List<int> indices, out List<Vector2> uvs)
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

            var controlNode1 = map.Nodes[4, node.Column, node.Row].AsVertex(); // 4th control node of current main node
            var controlNode2 = map.Nodes[3, node.Column + 1, node.Row].AsVertex(); // 3rd control node of right neighbour node
            var controlNode3 = map.Nodes[1, node.Column + 1, node.Row + 1].AsVertex(); // 1st control node of diagonal neighbour node
            var controlNode4 = map.Nodes[2, node.Column, node.Row + 1].AsVertex(); // 2nd control node of down neighbour node


            /*var vx1 = new Vertex { X = v.X + MIDPOINT / 2, Y = v.Y, Z = v.Z - MIDPOINT / 2 };
            var vx2 = new Vertex { X = v.X + MIDPOINT * 1.5f, Y = controlNode2.Y, Z = v.Z - MIDPOINT / 2 };
            var vx3 = new Vertex { X = v.X + MIDPOINT * 1.5f, Y = controlNode3.Y, Z = v.Z - MIDPOINT * 1.5f };
            var vx4 = new Vertex { X = v.X + MIDPOINT / 2, Y = controlNode4.Y, Z = v.Z - MIDPOINT * 1.5f };*/

            vs.Add(CatmullRom(controlNode1, v, diagonal, controlNode3, 0.25f));
            vs.Add(CatmullRom(controlNode2, right, down, controlNode4, 0.25f));
            vs.Add(CatmullRom(controlNode1, v, diagonal, controlNode3, 0.75f));
            vs.Add(CatmullRom(controlNode2, right, down, controlNode4, 0.75f));

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

            var flip = mapTexture.Flip ? new Vector2(1f, 1f) : Vector2.Zero;
            var rot = mapTexture.Rotation * Mathf.Pi * -0.5f;
            var off = new Vector2(0.5f, 0.5f);
            uvs =
            [
                (new Vector2(0,0f) - off).Rotated(rot) + off,
                (new Vector2(0.5f, 0) - off).Rotated(rot)  + off,
                (new Vector2(1, 0) - off).Rotated(rot)  + off,
                (new Vector2(1, 0.5f) - off).Rotated(rot)  + off,
                (new Vector2(1, 1) - off).Rotated(rot) + off,
                (new Vector2(0.5f, 1) - off).Rotated(rot) + off,
                (new Vector2(0, 1) - off).Rotated(rot)  + off,
                (new Vector2(0, 0.5f) - off).Rotated(rot) + off,
                (new Vector2(0.25f, 0.25f) - off).Rotated(rot)  + off,
                (new Vector2(0.75f, 0.25f) - off).Rotated(rot) + off,
                (new Vector2(0.75f, 0.75f) - off).Rotated(rot) + off,
                (new Vector2(0.25f, 0.75f) - off).Rotated(rot)  + off,
            ];

            if (flip != Vector2.Zero)
            {
                for (int i = 0; i < uvs.Count(); i++)
                    uvs[i] = new Vector2(flip.X - uvs[i].X, uvs[i].Y);
            }
            return vs;
        }
        private void AddSurface(GridNode node, Jjm map, ArrayMesh mesh, Texture2D texture, MapTexture mapTexture)
        {
            // Edge nodes
            if (node.Column >= map.MainNodes.GetLength(0) - 1 || node.Row >= map.MainNodes.GetLength(1) - 1)
                return;

            Godot.Collections.Array surfaceArray = [];
            surfaceArray.Resize((int)Mesh.ArrayType.Max);

            var verts = Quad(node, map, mapTexture, out var indices, out var uvs).Select(x => new Vector3(x.X, x.Y, x.Z));

            surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
            surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
            //surfaceArray[(int)Mesh.ArrayType.Normal] = normals.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
            surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

            var material = new StandardMaterial3D();
            material.AlbedoTexture = texture;
            material.TextureRepeat = false;

            mesh.SurfaceSetMaterial(0, material);
            // var m = mesh.SurfaceGetMaterial(0) as StandardMaterial3D;
            // m.PointSize = 8;
            //m.UsePointSize = true;
        }

        /// <summary>
        /// Returns a point on a Catmull-Rom spline defined by four control points.
        /// </summary>
        /// <param name="p0">The first control point (before the segment).</param>
        /// <param name="p1">The start point of the segment.</param>
        /// <param name="p2">The end point of the segment.</param>
        /// <param name="p3">The control point after the segment.</param>
        /// <param name="t">The interpolation parameter (0 to 1).</param>
        /// <returns>The interpolated point on the spline.</returns>
        public static Vertex CatmullRom(Vertex p0, Vertex p1, Vertex p2, Vertex p3, float t)
        {
            // Ensure t is clamped between 0 and 1
            t = Mathf.Clamp(t, 0, 1f);

            // Calculate powers of t
            float t2 = t * t;
            float t3 = t2 * t;

            // Catmull-Rom formula
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
    }
}