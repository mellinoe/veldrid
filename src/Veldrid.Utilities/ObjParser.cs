using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Veldrid.Utilities
{
    /// <summary>
    /// A parser for Wavefront OBJ files.
    /// </summary>
    public class ObjParser
    {
        private static readonly string[] s_newline = new string[] { "\n" };
        private static readonly char[] s_whitespaceChars = new char[] { ' ' };
        private static readonly char[] s_slashChar = new char[] { '/' };

        private readonly ParseContext _pc = new ParseContext();

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given raw text lines.
        /// </summary>
        /// <param name="lines">The text lines of the OBJ file.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(string[] lines)
        {
            foreach (string line in lines)
            {
                _pc.Process(line);
            }
            _pc.EndOfFileReached();

            return _pc.FinalizeFile();
        }

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given text stream.
        /// </summary>
        /// <param name="s">The <see cref="Stream"/> to read from.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(Stream s)
        {
            string text;
            using (StreamReader sr = new StreamReader(s))
            {
                text = sr.ReadToEnd();
            }

            int lineStart = 0;
            int lineEnd = -1;
            while ((lineEnd = text.IndexOf('\n', lineStart)) != -1)
            {
                string line;

                if (lineEnd != 0 && text[lineEnd - 1] == '\r')
                {
                    line = text.Substring(lineStart, lineEnd - lineStart - 1);
                }
                else
                {
                    line = text.Substring(lineStart, lineEnd - lineStart);
                }

                _pc.Process(line);
                lineStart = lineEnd + 1;
            }

            _pc.EndOfFileReached();
            return _pc.FinalizeFile();
        }

        private class ParseContext
        {
            private List<Vector3> _positions = new List<Vector3>();
            private List<Vector3> _normals = new List<Vector3>();
            private List<Vector2> _texCoords = new List<Vector2>();

            private List<ObjFile.MeshGroup> _groups = new List<ObjFile.MeshGroup>();

            private string _currentGroupName;
            private string _currentMaterial;
            private int _currentSmoothingGroup;
            private List<ObjFile.Face> _currentGroupFaces = new List<ObjFile.Face>();

            private int _currentLine;
            private string _currentLineText;

            private string _materialLibName;

            public void Process(string line)
            {
                _currentLine++;
                _currentLineText = line;

                string[] pieces = line.Split(s_whitespaceChars, StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length == 0 || pieces[0].StartsWith("#"))
                {
                    return;
                }
                switch (pieces[0])
                {
                    case "v":
                        ExpectExactly(pieces, 3, "v");
                        DiscoverPosition(ParseVector3(pieces[1], pieces[2], pieces[3], "position data"));
                        break;
                    case "vn":
                        ExpectExactly(pieces, 3, "vn");
                        DiscoverNormal(ParseVector3(pieces[1], pieces[2], pieces[3], "normal data"));
                        break;
                    case "vt":
                        ExpectAtLeast(pieces, 1, "vt");
                        Vector2 texCoord = ParseVector2(pieces[1], pieces[2], "texture coordinate data");
                        // Flip v coordinate
                        texCoord.Y = 1f - texCoord.Y;
                        DiscoverTexCoord(texCoord);
                        break;
                    case "g":
                        ExpectAtLeast(pieces, 1, "g");
                        FinalizeGroup();
                        _currentGroupName = line.Substring(1, line.Length - 1).Trim();
                        break;
                    case "usemtl":
                        ExpectAtLeast(pieces, 1, "usematl");
                        if (_currentMaterial != null)
                        {
                            string nextGroupName = _currentGroupName + "_Next";
                            FinalizeGroup();
                            _currentGroupName = nextGroupName;
                        }

                        _currentMaterial = pieces[1];
                        break;
                    case "s":
                        ExpectExactly(pieces, 1, "s");
                        if (pieces[1] == "off")
                        {
                            _currentSmoothingGroup = 0;
                        }
                        else
                        {
                            _currentSmoothingGroup = ParseInt(pieces[1], "smoothing group");
                        }
                        break;
                    case "f":
                        ExpectAtLeast(pieces, 3, "f");
                        ProcessFaceLine(pieces);
                        break;
                    case "mtllib":
                        ExpectExactly(pieces, 1, "mtllib");
                        DiscoverMaterialLib(pieces[1]);
                        break;
                    default:
                        throw new ObjParseException(
                            string.Format("An unsupported line-type specifier, '{0}', was used on line {1}, \"{2}\"",
                            pieces[0],
                            _currentLine,
                            _currentLineText));
                }
            }

            private void DiscoverMaterialLib(string libName)
            {
                if (_materialLibName != null)
                {
                    throw new ObjParseException(
                        string.Format("mtllib appeared again in the file. It should only appear once. Line {0}, \"{1}\"", _currentLine, _currentLineText));
                }

                _materialLibName = libName;
            }

            private void ProcessFaceLine(string[] pieces)
            {
                string first = pieces[1];
                ObjFile.FaceVertex faceVertex0 = ParseFaceVertex(first);

                for (int i = 0; i < pieces.Length - 3; i++)
                {
                    string second = pieces[i + 2];
                    ObjFile.FaceVertex faceVertex1 = ParseFaceVertex(second);
                    string third = pieces[i + 3];
                    ObjFile.FaceVertex faceVertex2 = ParseFaceVertex(third);

                    DiscoverFace(new ObjFile.Face(faceVertex0, faceVertex1, faceVertex2, _currentSmoothingGroup));
                }
            }

            private ObjFile.FaceVertex ParseFaceVertex(string faceComponents)
            {
                string[] slashSplit = faceComponents.Split(s_slashChar, StringSplitOptions.None);
                if (slashSplit.Length != 1 && slashSplit.Length != 2 && slashSplit.Length != 3)
                {
                    throw CreateExceptionForWrongFaceCount(slashSplit.Length);
                }

                int pos = ParseInt(slashSplit[0], "the first face position index");

                int texCoord = -1;
                if (slashSplit.Length >= 2 && !string.IsNullOrEmpty(slashSplit[1]))
                {
                    texCoord = ParseInt(slashSplit[1], "the first face texture coordinate index");
                }

                int normal = -1;
                if (slashSplit.Length == 3)
                {
                    normal = ParseInt(slashSplit[2], "the first face normal index");
                }

                return new ObjFile.FaceVertex() { PositionIndex = pos, NormalIndex = normal, TexCoordIndex = texCoord };
            }

            private ObjParseException CreateExceptionForWrongFaceCount(int count)
            {
                return new ObjParseException(
                    string.Format("Expected 1, 2, or 3 face components, but got {0}, on line {1}, \"{2}\"",
                    count,
                    _currentLine,
                    _currentLineText));
            }

            public void DiscoverPosition(Vector3 position)
            {
                _positions.Add(position);
            }

            public void DiscoverNormal(Vector3 normal)
            {
                _normals.Add(normal);
            }

            public void DiscoverTexCoord(Vector2 texCoord)
            {
                _texCoords.Add(texCoord);
            }

            public void DiscoverFace(ObjFile.Face face)
            {
                _currentGroupFaces.Add(face);
            }

            public void FinalizeGroup()
            {
                if (_currentGroupName != null)
                {
                    ObjFile.Face[] faces = _currentGroupFaces.ToArray();
                    _groups.Add(new ObjFile.MeshGroup(_currentGroupName, _currentMaterial, faces));

                    _currentGroupName = null;
                    _currentMaterial = null;
                    _currentSmoothingGroup = -1;
                    _currentGroupFaces.Clear();
                }
            }

            public void EndOfFileReached()
            {
                _currentGroupName = _currentGroupName ?? "GlobalFileGroup";
                _groups.Add(new ObjFile.MeshGroup(_currentGroupName, _currentMaterial, _currentGroupFaces.ToArray()));
            }

            public ObjFile FinalizeFile()
            {
                return new ObjFile(_positions.ToArray(), _normals.ToArray(), _texCoords.ToArray(), _groups.ToArray(), _materialLibName);
            }

            private Vector3 ParseVector3(string xStr, string yStr, string zStr, string location)
            {
                try
                {
                    float x = float.Parse(xStr, CultureInfo.InvariantCulture);
                    float y = float.Parse(yStr, CultureInfo.InvariantCulture);
                    float z = float.Parse(zStr, CultureInfo.InvariantCulture);

                    return new Vector3(x, y, z);
                }
                catch (FormatException fe)
                {
                    throw CreateParseException(location, fe);
                }
            }

            private Vector2 ParseVector2(string xStr, string yStr, string location)
            {
                try
                {
                    float x = float.Parse(xStr, CultureInfo.InvariantCulture);
                    float y = float.Parse(yStr, CultureInfo.InvariantCulture);

                    return new Vector2(x, y);
                }
                catch (FormatException fe)
                {
                    throw CreateParseException(location, fe);
                }
            }

            private int ParseInt(string intStr, string location)
            {
                try
                {
                    int i = int.Parse(intStr, CultureInfo.InvariantCulture);
                    return i;
                }
                catch (FormatException fe)
                {
                    throw CreateParseException(location, fe);
                }
            }

            private void ExpectExactly(string[] pieces, int count, string name)
            {
                if (pieces.Length != count + 1)
                {
                    string message = string.Format(
                        "Expected exactly {0} components to a line starting with {1}, on line {2}, \"{3}\".",
                        count,
                        name,
                        _currentLine,
                        _currentLineText);
                    throw new ObjParseException(message);
                }
            }

            private void ExpectAtLeast(string[] pieces, int count, string name)
            {
                if (pieces.Length < count + 1)
                {
                    string message = string.Format(
                        "Expected at least {0} components to a line starting with {1}, on line {2}, \"{3}\".",
                        count,
                        name,
                        _currentLine,
                        _currentLineText);
                    throw new ObjParseException(message);
                }
            }

            private ObjParseException CreateParseException(string location, FormatException fe)
            {
                string message = string.Format("An error ocurred while parsing {0} on line {1}, \"{2}\"", location, _currentLine, _currentLineText);
                return new ObjParseException(message, fe);
            }
        }
    }

    /// <summary>
    /// An parsing error for Wavefront OBJ files.
    /// </summary>
    public class ObjParseException : Exception
    {
        public ObjParseException(string message) : base(message)
        {
        }

        public ObjParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents a parset Wavefront OBJ file.
    /// </summary>
    public class ObjFile
    {
        public Vector3[] Positions { get; }
        public Vector3[] Normals { get; }
        public Vector2[] TexCoords { get; }
        public MeshGroup[] MeshGroups { get; }
        public string MaterialLibName { get; }

        public ObjFile(Vector3[] positions, Vector3[] normals, Vector2[] texCoords, MeshGroup[] meshGroups, string materialLibName)
        {
            Positions = positions;
            Normals = normals;
            TexCoords = texCoords;
            MeshGroups = meshGroups;
            MaterialLibName = materialLibName;
        }

        /// <summary>
        /// Gets a <see cref="ConstructedMeshInfo"/> for the given OBJ <see cref="MeshGroup"/>.
        /// </summary>
        /// <param name="group">The OBJ <see cref="MeshGroup"/> to construct.</param>
        /// <returns>A new <see cref="ConstructedMeshInfo"/>.</returns>
        public ConstructedMeshInfo GetMesh(MeshGroup group)
        {
            Dictionary<FaceVertex, ushort> vertexMap = new Dictionary<FaceVertex, ushort>();
            ushort[] indices = new ushort[group.Faces.Length * 3];
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();

            for (int i = 0; i < group.Faces.Length; i++)
            {
                Face face = group.Faces[i];
                ushort index0 = GetOrCreate(vertexMap, vertices, face.Vertex0, face.Vertex1, face.Vertex2);
                ushort index1 = GetOrCreate(vertexMap, vertices, face.Vertex1, face.Vertex2, face.Vertex0);
                ushort index2 = GetOrCreate(vertexMap, vertices, face.Vertex2, face.Vertex0, face.Vertex1);

                // Reverse winding order here.
                indices[(i * 3)] = index0;
                indices[(i * 3) + 2] = index1;
                indices[(i * 3) + 1] = index2;
            }

            return new ConstructedMeshInfo(vertices.ToArray(), indices, group.Material);
        }

        /// <summary>
        /// Constructs the first <see cref="MeshGroup"/> in this file.
        /// </summary>
        /// <returns>A new <see cref="ConstructedMeshInfo"/>.</returns>
        public ConstructedMeshInfo GetFirstMesh()
        {
            return GetMesh(MeshGroups[0]);
        }

        private ushort GetOrCreate(
            Dictionary<FaceVertex, ushort> vertexMap,
            List<VertexPositionNormalTexture> vertices,
            FaceVertex key,
            FaceVertex adjacent1,
            FaceVertex adjacent2)
        {
            ushort index;
            if (!vertexMap.TryGetValue(key, out index))
            {
                VertexPositionNormalTexture vertex = ConstructVertex(key, adjacent1, adjacent2);
                vertices.Add(vertex);
                index = checked((ushort)(vertices.Count - 1));
                vertexMap.Add(key, index);
            }

            return index;
        }

        private VertexPositionNormalTexture ConstructVertex(FaceVertex key, FaceVertex adjacent1, FaceVertex adjacent2)
        {
            Vector3 position = Positions[key.PositionIndex - 1];
            Vector3 normal;
            if (key.NormalIndex == -1)
            {
                normal = ComputeNormal(key, adjacent1, adjacent2);
            }
            else
            {
                normal = Normals[key.NormalIndex - 1];
            }


            Vector2 texCoord = key.TexCoordIndex == -1 ? Vector2.Zero : TexCoords[key.TexCoordIndex - 1];

            return new VertexPositionNormalTexture(position, normal, texCoord);
        }

        private Vector3 ComputeNormal(FaceVertex v1, FaceVertex v2, FaceVertex v3)
        {
            Vector3 pos1 = Positions[v1.PositionIndex - 1];
            Vector3 pos2 = Positions[v2.PositionIndex - 1];
            Vector3 pos3 = Positions[v3.PositionIndex - 1];

            return Vector3.Normalize(Vector3.Cross(pos1 - pos2, pos1 - pos3));
        }

        /// <summary>
        /// An OBJ file construct describing an individual mesh group.
        /// </summary>
        public struct MeshGroup
        {
            /// <summary>
            /// The name.
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// The name of the associated <see cref="MaterialDefinition"/>.
            /// </summary>
            public readonly string Material;

            /// <summary>
            /// The set of <see cref="Face"/>s comprising this mesh group.
            /// </summary>
            public readonly Face[] Faces;

            /// <summary>
            /// Constructs a new <see cref="MeshGroup"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="material">The name of the associated <see cref="MaterialDefinition"/>.</param>
            /// <param name="faces">The faces.</param>
            public MeshGroup(string name, string material, Face[] faces)
            {
                Name = name;
                Material = material;
                Faces = faces;
            }
        }

        /// <summary>
        /// An OBJ file construct describing the indices of vertex components.
        /// </summary>
        public struct FaceVertex
        {
            /// <summary>
            /// The index of the position component.
            /// </summary>
            public int PositionIndex;

            /// <summary>
            /// The index of the normal component.
            /// </summary>
            public int NormalIndex;

            /// <summary>
            /// The index of the texture coordinate component.
            /// </summary>
            public int TexCoordIndex;

            public override string ToString()
            {
                return string.Format("Pos:{0}, Normal:{1}, TexCoord:{2}", PositionIndex, NormalIndex, TexCoordIndex);
            }
        }

        /// <summary>
        /// An OBJ file construct describing an individual mesh face.
        /// </summary>
        public struct Face
        {
            /// <summary>
            /// The first vertex.
            /// </summary>
            public readonly FaceVertex Vertex0;

            /// <summary>
            /// The second vertex.
            /// </summary>
            public readonly FaceVertex Vertex1;

            /// <summary>
            /// The third vertex.
            /// </summary>
            public readonly FaceVertex Vertex2;

            /// <summary>
            /// The smoothing group. Describes which kind of vertex smoothing should be applied.
            /// </summary>
            public readonly int SmoothingGroup;

            public Face(FaceVertex v0, FaceVertex v1, FaceVertex v2, int smoothingGroup = -1)
            {
                Vertex0 = v0;
                Vertex1 = v1;
                Vertex2 = v2;

                SmoothingGroup = smoothingGroup;
            }
        }
    }

    /// <summary>
    /// A standalone <see cref="MeshData"/> created from information from an <see cref="ObjFile"/>.
    /// </summary>
    public class ConstructedMeshInfo : MeshData
    {
        /// <summary>
        /// The vertices of the mesh.
        /// </summary>
        public VertexPositionNormalTexture[] Vertices { get; }

        /// <summary>
        /// The indices of the mesh.
        /// </summary>
        public ushort[] Indices { get; }

        /// <summary>
        /// The name of the <see cref="MaterialDefinition"/> associated with this mesh.
        /// </summary>
        public string MaterialName { get; }

        /// <summary>
        /// Constructs a new <see cref="ConstructedMeshInfo"/>.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="indices">The indices.</param>
        /// <param name="materialName">The name of the associated MTL <see cref="MaterialDefinition"/>.</param>
        public ConstructedMeshInfo(VertexPositionNormalTexture[] vertices, ushort[] indices, string materialName)
        {
            Vertices = vertices;
            Indices = indices;
            MaterialName = materialName;
        }

        public DeviceBuffer CreateVertexBuffer(ResourceFactory factory, CommandList cl)
        {
            DeviceBuffer vb = factory.CreateBuffer(
                new BufferDescription((uint)(Vertices.Length * VertexPositionNormalTexture.SizeInBytes), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(vb, 0, Vertices);
            return vb;
        }

        public DeviceBuffer CreateIndexBuffer(ResourceFactory factory, CommandList cl, out int indexCount)
        {
            DeviceBuffer ib = factory.CreateBuffer(new BufferDescription((uint)(Indices.Length * sizeof(int)), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(ib, 0, Indices);
            indexCount = Indices.Length;
            return ib;
        }

        public unsafe BoundingSphere GetBoundingSphere()
        {
            fixed (VertexPositionNormalTexture* ptr = Vertices)
            {
                return BoundingSphere.CreateFromPoints((Vector3*)ptr, Vertices.Length, VertexPositionNormalTexture.SizeInBytes);
            }
        }

        public unsafe BoundingBox GetBoundingBox()
        {
            fixed (VertexPositionNormalTexture* ptr = Vertices)
            {
                return BoundingBox.CreateFromPoints(
                    (Vector3*)ptr,
                    Vertices.Length,
                    VertexPositionNormalTexture.SizeInBytes,
                    Quaternion.Identity,
                    Vector3.Zero,
                    Vector3.One);
            }
        }

        public bool RayCast(Ray ray, out float distance)
        {
            distance = float.MaxValue;
            bool result = false;
            for (int i = 0; i < Indices.Length - 2; i += 3)
            {
                Vector3 v0 = Vertices[Indices[i + 0]].Position;
                Vector3 v1 = Vertices[Indices[i + 1]].Position;
                Vector3 v2 = Vertices[Indices[i + 2]].Position;

                float newDistance;
                if (ray.Intersects(ref v0, ref v1, ref v2, out newDistance))
                {
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                    }

                    result = true;
                }
            }

            return result;
        }

        public int RayCast(Ray ray, List<float> distances)
        {
            int hits = 0;
            for (int i = 0; i < Indices.Length - 2; i += 3)
            {
                Vector3 v0 = Vertices[Indices[i + 0]].Position;
                Vector3 v1 = Vertices[Indices[i + 1]].Position;
                Vector3 v2 = Vertices[Indices[i + 2]].Position;

                if (ray.Intersects(ref v0, ref v1, ref v2, out float newDistance))
                {
                    hits++;
                    distances.Add(newDistance);
                }
            }

            return hits;
        }

        public Vector3[] GetVertexPositions()
        {
            return Vertices.Select(vpnt => vpnt.Position).ToArray();
        }

        public ushort[] GetIndices()
        {
            return Indices;
        }
    }
}
