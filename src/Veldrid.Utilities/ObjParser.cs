using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Veldrid.Utilities
{
    /// <summary>
    /// A parser for Wavefront OBJ files.
    /// </summary>
    public class ObjParser
    {
        private const int InitialReadBufferSize = 2048;

        private static readonly char[] s_whitespaceChar = new[] { ' ' };
        private static readonly char s_slashChar = '/';

        private readonly ParseContext _pc = new ParseContext();
        private char[] _readBuffer;

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given raw text lines.
        /// </summary>
        /// <param name="lines">The text lines of the OBJ file.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(string[] lines)
        {
            return Parse((IEnumerable<string>)lines);
        }

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given raw text lines.
        /// </summary>
        /// <param name="lines">The text lines of the OBJ file.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(IEnumerable<string> lines)
        {
            _pc.Reset();

            foreach (string line in lines)
                _pc.Process(line.AsSpan());

            _pc.EndOfFileReached();
            return _pc.FinalizeFile();
        }

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given raw text lines.
        /// </summary>
        /// <param name="lines">The text lines of the OBJ file.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(IEnumerable<ReadOnlyMemory<char>> lines)
        {
            _pc.Reset();

            foreach (ReadOnlyMemory<char> line in lines)
                _pc.Process(line.Span);

            _pc.EndOfFileReached();
            return _pc.FinalizeFile();
        }

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given text stream.
        /// </summary>
        /// <param name="reader">The <see cref="Stream"/> to read from.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
                return Parse(reader);
        }

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given text stream.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read from.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(TextReader reader)
        {
            _pc.Reset();

            if (_readBuffer == null)
                _readBuffer = new char[InitialReadBufferSize];

            int readIndex = 0;

            // Tries to process one or more lines inside the read buffer.
            void TryProcessLines()
            {
                Span<char> text = _readBuffer.AsSpan(0, readIndex);
                int lineEnd;
                while ((lineEnd = text.IndexOf('\n')) != -1)
                {
                    Span<char> line;
                    if (lineEnd != 0 && text[lineEnd - 1] == '\r')
                    {
                        line = text.Slice(0, lineEnd - 1);
                    }
                    else
                    {
                        line = text.Slice(0, lineEnd);
                    }

                    _pc.Process(line);
                    text = text.Slice(lineEnd + 1);
                }

                // Shift back remaining data.
                int consumed = readIndex - text.Length;
                readIndex -= consumed;
                Array.Copy(_readBuffer, consumed, _readBuffer, 0, readIndex);
            }

            TryRead:
            int read;
            while ((read = reader.ReadBlock(_readBuffer, readIndex, _readBuffer.Length - readIndex)) > 0)
            {
                readIndex += read;
                TryProcessLines();
            }

            if (readIndex > 0)
            {
                if (readIndex == _readBuffer.Length)
                {
                    // The buffer couldn't contain a whole line so resize it.
                    Array.Resize(ref _readBuffer, _readBuffer.Length * 2);
                    goto TryRead;
                }

                TryProcessLines();

                // Try to parse the rest that doesn't have a line ending.
                if (readIndex > 0)
                    _pc.Process(_readBuffer.AsSpan(0, readIndex));
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
            private string _materialLibName;

            public void Process(ReadOnlySpan<char> line)
            {
                _currentLine++;

                ReadOnlySpanSplitter<char> splitter = new ReadOnlySpanSplitter<char>(line, s_whitespaceChar, StringSplitOptions.RemoveEmptyEntries);
                if (!splitter.MoveNext())
                    return;

                ReadOnlySpan<char> piece0 = splitter.Current;
                if (piece0.StartsWith("#".AsSpan()))
                    return;

                if (piece0.SequenceEqual("v".AsSpan()))
                {
                    ExpectPieces(ref splitter, "v", true, out ReadOnlySpan<char> piece1, out ReadOnlySpan<char> piece2, out ReadOnlySpan<char> piece3);
                    DiscoverPosition(ParseVector3(piece1, piece2, piece3, "position data"));
                }
                else if (piece0.SequenceEqual("vn".AsSpan()))
                {
                    ExpectPieces(ref splitter, "vn", true, out ReadOnlySpan<char> piece1, out ReadOnlySpan<char> piece2, out ReadOnlySpan<char> piece3);
                    DiscoverNormal(ParseVector3(piece1, piece2, piece3, "normal data"));
                }
                else if (piece0.SequenceEqual("vt".AsSpan()))
                {
                    const string pieceName = "texture coordinate data";

                    ReadOnlySpan<char> x;
                    ReadOnlySpan<char> y = "0".AsSpan();

                    if (!splitter.MoveNext())
                        throw CreateExpectPiecesException("one", pieceName, false);
                    x = splitter.Current;

                    if (splitter.MoveNext())
                        y = splitter.Current;

                    Vector2 texCoord = ParseVector2(x, y, pieceName);
                    // Flip v coordinate
                    texCoord.Y = 1f - texCoord.Y;
                    DiscoverTexCoord(texCoord);
                }
                else if (piece0.SequenceEqual("g".AsSpan()))
                {
                    ExpectPieces(ref splitter, "g", false, out ReadOnlySpan<char> piece1);
                    FinalizeGroup();
                    _currentGroupName = piece1.ToString();
                }
                else if (piece0.SequenceEqual("usemtl".AsSpan()))
                {
                    ExpectPieces(ref splitter, "usematl", true, out ReadOnlySpan<char> piece1);
                    if (_currentMaterial != null)
                    {
                        string nextGroupName = _currentGroupName + "_Next";
                        FinalizeGroup();
                        _currentGroupName = nextGroupName;
                    }
                    _currentMaterial = piece1.ToString();
                }
                else if (piece0.SequenceEqual("s".AsSpan()))
                {
                    ExpectPieces(ref splitter, "s", true, out ReadOnlySpan<char> piece1);
                    if (piece1.SequenceEqual("off".AsSpan()))
                        _currentSmoothingGroup = 0;
                    else
                        _currentSmoothingGroup = ParseInt(piece1, "smoothing group");
                }
                else if (piece0.SequenceEqual("f".AsSpan()))
                {
                    ExpectPieces(ref splitter, "f", false, out ReadOnlySpan<char> piece1, out ReadOnlySpan<char> piece2);
                    ProcessFaceLine(ref splitter, piece1, piece2);
                }
                else if (piece0.SequenceEqual("mtllib".AsSpan()))
                {
                    ExpectPieces(ref splitter, "mtllib", true, out ReadOnlySpan<char> piece1);
                    DiscoverMaterialLib(piece1);
                }
                else
                {
                    throw new ObjParseException(string.Format(
                        "An unsupported line-type specifier, '{0}', was used on line {1}.",
                        piece0.ToString(),
                        _currentLine));
                }
            }

            private void DiscoverMaterialLib(ReadOnlySpan<char> libName)
            {
                if (_materialLibName != null)
                {
                    throw new ObjParseException(
                        $"mtllib appeared again in the file. It should only appear once. Line {_currentLine}.");
                }

                _materialLibName = libName.ToString();
            }

            private void ProcessFaceLine(
                ref ReadOnlySpanSplitter<char> splitter,
                ReadOnlySpan<char> piece1, ReadOnlySpan<char> piece2)
            {
                ObjFile.FaceVertex faceVertex0 = ParseFaceVertex(piece1);

                while (splitter.MoveNext())
                {
                    ReadOnlySpan<char> piece3 = splitter.Current;
                    ObjFile.FaceVertex faceVertex1 = ParseFaceVertex(piece2);
                    ObjFile.FaceVertex faceVertex2 = ParseFaceVertex(piece3);

                    DiscoverFace(new ObjFile.Face(faceVertex0, faceVertex1, faceVertex2, _currentSmoothingGroup));
                    piece2 = piece3;
                }
            }

            private ObjFile.FaceVertex ParseFaceVertex(ReadOnlySpan<char> faceComponents)
            {
                if (faceComponents.IsEmpty)
                    throw CreateExceptionForWrongFaceCount("There must be at least one face component");

                int firstSlash = faceComponents.IndexOf(s_slashChar);
                ReadOnlySpan<char> firstSlice = firstSlash == -1
                    ? faceComponents
                    : faceComponents.Slice(0, firstSlash);

                ReadOnlySpan<char> afterFirstSlash = faceComponents.Slice(firstSlash + 1);
                int secondSlash = afterFirstSlash.IndexOf(s_slashChar);
                ReadOnlySpan<char> secondSlice = secondSlash == -1
                    ? afterFirstSlash
                    : afterFirstSlash.Slice(0, secondSlash);

                ReadOnlySpan<char> afterSecondSlash = afterFirstSlash.Slice(secondSlash + 1);
                int thirdSlash = afterSecondSlash.IndexOf(s_slashChar);
                ReadOnlySpan<char> thirdSlice = thirdSlash == -1
                    ? afterSecondSlash
                    : throw CreateExceptionForWrongFaceCount("No more than three face components are allowed");

                int position = ParseInt(firstSlice, "the first face position index");
                int texCoord = firstSlash == -1 ? -1 : ParseInt(secondSlice, "the first face texture coordinate index");
                int normal = secondSlash == -1 ? -1 : ParseInt(thirdSlice, "the first face normal index");

                return new ObjFile.FaceVertex(position, normal, texCoord);
            }

            private ObjParseException CreateExceptionForWrongFaceCount(string message)
            {
                return new ObjParseException($"{message}, on line {_currentLine}.");
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ObjFile.FaceVertex FinalizeFaceVertex(
                int positionOffset, int normalOffset, int texCoordOffset,
                ObjFile.FaceVertex vertex)
            {
                if (vertex.PositionIndex < 0)
                {
                    vertex.PositionIndex += positionOffset;
                }

                if (vertex.NormalIndex < 0)
                {
                    vertex.NormalIndex += normalOffset;
                }

                if (vertex.TexCoordIndex < 0)
                {
                    vertex.TexCoordIndex += texCoordOffset;
                }

                return vertex;
            }

            public void FinalizeGroup()
            {
                if (_currentGroupName != null)
                {
                    int positionOffset = _positions.Count + 1;
                    int normalOffset = _normals.Count + 1;
                    int texCoordOffset = _texCoords.Count + 1;

                    ObjFile.Face[] faces = _currentGroupFaces.ToArray();
                    for (int i = 0; i < faces.Length; i++)
                    {
                        ref ObjFile.Face face = ref faces[i];
                        var vertex0 = FinalizeFaceVertex(positionOffset, normalOffset, texCoordOffset, face.Vertex0);
                        var vertex1 = FinalizeFaceVertex(positionOffset, normalOffset, texCoordOffset, face.Vertex1);
                        var vertex2 = FinalizeFaceVertex(positionOffset, normalOffset, texCoordOffset, face.Vertex2);
                        face = new ObjFile.Face(vertex0, vertex1, vertex2, face.SmoothingGroup);
                    }
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
                ObjFile file = new ObjFile(_positions.ToArray(), _normals.ToArray(), _texCoords.ToArray(), _groups.ToArray(), _materialLibName);

                Reset();

                return file;
            }

            public void Reset()
            {
                _positions.Clear();
                _normals.Clear();
                _texCoords.Clear();
                _groups.Clear();
                _currentGroupFaces.Clear();
                _currentGroupName = null;
                _currentMaterial = null;
                _materialLibName = null;
                _currentSmoothingGroup = -1;
                _currentLine = 0;
            }

            private float ParseFloat(ReadOnlySpan<char> span)
            {
#if NETSTANDARD2_1 || NETCOREAPP
                return float.Parse(span, NumberStyles.Float, CultureInfo.InvariantCulture);
#else
                return float.Parse(span.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
#endif
            }

            private Vector3 ParseVector3(ReadOnlySpan<char> xStr, ReadOnlySpan<char> yStr, ReadOnlySpan<char> zStr, string location)
            {
                try
                {
                    float x = ParseFloat(xStr);
                    float y = ParseFloat(yStr);
                    float z = ParseFloat(zStr);
                    return new Vector3(x, y, z);
                }
                catch (FormatException ex)
                {
                    throw CreateParseException(location, ex);
                }
            }

            private Vector2 ParseVector2(ReadOnlySpan<char> xStr, ReadOnlySpan<char> yStr, string location)
            {
                try
                {
                    float x = ParseFloat(xStr);
                    float y = ParseFloat(yStr);
                    return new Vector2(x, y);
                }
                catch (FormatException ex)
                {
                    throw CreateParseException(location, ex);
                }
            }

            protected int ParseInt(ReadOnlySpan<char> intStr, string location)
            {
                try
                {
#if NETSTANDARD2_1 || NETCOREAPP
                    return int.Parse(intStr, NumberStyles.None, CultureInfo.InvariantCulture);
#else
                    return int.Parse(intStr.ToString(), NumberStyles.None, CultureInfo.InvariantCulture);
#endif
                }
                catch (FormatException ex)
                {
                    throw CreateParseException(location, ex);
                }
            }

            private void ExpectPieces(
                ref ReadOnlySpanSplitter<char> pieces, string name, bool exact,
                out ReadOnlySpan<char> piece0, out ReadOnlySpan<char> piece1, out ReadOnlySpan<char> piece2)
            {
                if (pieces.MoveNext())
                {
                    piece0 = pieces.Current;

                    if (pieces.MoveNext())
                    {
                        piece1 = pieces.Current;

                        if (pieces.MoveNext())
                        {
                            piece2 = pieces.Current;

                            if (!exact || !pieces.MoveNext())
                                return;
                        }
                    }
                }
                throw CreateExpectPiecesException("three", name, exact);
            }

            private void ExpectPieces(
                ref ReadOnlySpanSplitter<char> pieces, string name, bool exact,
                out ReadOnlySpan<char> piece0, out ReadOnlySpan<char> piece1)
            {
                if (pieces.MoveNext())
                {
                    piece0 = pieces.Current;

                    if (pieces.MoveNext())
                    {
                        piece1 = pieces.Current;

                        if (!exact || !pieces.MoveNext())
                            return;
                    }
                }
                throw CreateExpectPiecesException("two", name, exact);
            }

            private void ExpectPieces(
                ref ReadOnlySpanSplitter<char> pieces, string name, bool exact,
                out ReadOnlySpan<char> piece)
            {
                if (pieces.MoveNext())
                {
                    piece = pieces.Current;

                    if (!exact || !pieces.MoveNext())
                        return;
                }
                throw CreateExpectPiecesException("one", name, exact);
            }

            private Exception CreateExpectPiecesException(string amount, string name, bool exact)
            {
                string message = string.Format(
                    "Expected {0} {1} components to a line starting with {2}, on line {3}.",
                    exact ? "exactly" : "at least",
                    amount,
                    name,
                    _currentLine);
                throw new ObjParseException(message);
            }

            private ObjParseException CreateParseException(string location, Exception inner)
            {
                string message = string.Format("An error ocurred while parsing {0} on line {1}", location, _currentLine);
                return new ObjParseException(message, inner);
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
            Positions = positions ?? throw new ArgumentNullException(nameof(positions));
            Normals = normals ?? throw new ArgumentNullException(nameof(normals));
            TexCoords = texCoords ?? throw new ArgumentNullException(nameof(texCoords));
            MeshGroups = meshGroups ?? throw new ArgumentNullException(nameof(meshGroups));
            MaterialLibName = materialLibName;
        }

        /// <summary>
        /// Gets a <see cref="ConstructedMeshInfo"/> for the given OBJ <see cref="MeshGroup"/>.
        /// </summary>
        /// <param name="group">The OBJ <see cref="MeshGroup"/> to construct.</param>
        /// <param name="reduce">Whether to simplify the mesh by sharing identical vertices.</param>
        /// <returns>A new <see cref="ConstructedMeshInfo"/>.</returns>
        public ConstructedMeshInfo GetMesh(MeshGroup group, bool reduce)
        {
            ushort[] indices = new ushort[group.Faces.Length * 3];
            Dictionary<FaceVertex, ushort> vertexMap = new Dictionary<FaceVertex, ushort>();
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();

            for (int i = 0; i < group.Faces.Length; i++)
            {
                Face face = group.Faces[i];
                ushort index0;
                ushort index1;
                ushort index2;

                if (reduce)
                {
                    index0 = GetOrCreate(vertexMap, vertices, face.Vertex0, face.Vertex1, face.Vertex2);
                    index1 = GetOrCreate(vertexMap, vertices, face.Vertex1, face.Vertex2, face.Vertex0);
                    index2 = GetOrCreate(vertexMap, vertices, face.Vertex2, face.Vertex0, face.Vertex1);
                }
                else
                {
                    index0 = checked((ushort)(i * 3 + 0));
                    index1 = checked((ushort)(i * 3 + 1));
                    index2 = checked((ushort)(i * 3 + 2));
                    vertices.Add(ConstructVertex(face.Vertex0, face.Vertex1, face.Vertex2));
                    vertices.Add(ConstructVertex(face.Vertex1, face.Vertex2, face.Vertex0));
                    vertices.Add(ConstructVertex(face.Vertex2, face.Vertex0, face.Vertex1));
                }

                // Reverse winding order here.
                indices[(i * 3) + 0] = index0;
                indices[(i * 3) + 2] = index1;
                indices[(i * 3) + 1] = index2;
            }

            return new ConstructedMeshInfo(vertices.ToArray(), indices, group.Material);
        }

        /// <summary>
        /// Gets a <see cref="ConstructedMeshInfo"/> for the given OBJ <see cref="MeshGroup"/>.
        /// The mesh is simplified by sharing identical vertices.
        /// </summary>
        /// <param name="group">The OBJ <see cref="MeshGroup"/> to construct.</param>
        /// <returns>A new <see cref="ConstructedMeshInfo"/>.</returns>
        public ConstructedMeshInfo GetMesh(MeshGroup group)
        {
            return GetMesh(group, true);
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
            if (!vertexMap.TryGetValue(key, out ushort index))
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
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Material = material;
                Faces = faces;
            }
        }

        /// <summary>
        /// An OBJ file construct describing the indices of vertex components.
        /// </summary>
        public struct FaceVertex : IEquatable<FaceVertex>
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

            public FaceVertex(int positionIndex, int normalIndex, int texCoordIndex)
            {
                PositionIndex = positionIndex;
                NormalIndex = normalIndex;
                TexCoordIndex = texCoordIndex;
            }

            public bool Equals(FaceVertex other)
            {
                return PositionIndex == other.PositionIndex
                    && NormalIndex == other.NormalIndex
                    && TexCoordIndex == other.TexCoordIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is FaceVertex value && Equals(value);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int code = 17;
                    code = code * 31 + PositionIndex;
                    code = code * 31 + NormalIndex;
                    code = code * 31 + TexCoordIndex;
                    return code;
                }
            }

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
            Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            Indices = indices ?? throw new ArgumentNullException(nameof(indices));
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
            DeviceBuffer ib = factory.CreateBuffer(new BufferDescription((uint)(Indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
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

                if (ray.Intersects(ref v0, ref v1, ref v2, out float newDistance))
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
            Vector3[] array = new Vector3[Vertices.Length];
            Span<VertexPositionNormalTexture> src = Vertices.AsSpan();
            Span<Vector3> dst = array.AsSpan(0, src.Length);
            for (int i = 0; i < src.Length; i++)
                dst[i] = src[i].Position;
            return array;
        }

        public ushort[] GetIndices()
        {
            return Indices;
        }
    }
}
