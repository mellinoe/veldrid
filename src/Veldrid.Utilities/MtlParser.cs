using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Veldrid.Utilities
{
    /// <summary>
    /// A parser for Wavefront MTL files.
    /// </summary>
    public class MtlParser
    {
        private static readonly string[] s_newline = new string[] { "\n" };

        private readonly ParseContext _pc = new ParseContext();

        /// <summary>
        /// Parses a <see cref="MtlFile"/> from the given array of text lines.
        /// </summary>
        /// <param name="lines">The raw text lines of the MTL file.</param>
        /// <returns>A new <see cref="MtlFile"/>.</returns>
        public MtlFile Parse(string[] lines)
        {
            foreach (string line in lines)
            {
                _pc.Process(line);
            }
            _pc.EndOfFileReached();

            return _pc.FinalizeFile();
        }

        /// <summary>
        /// Parses a <see cref="MtlFile"/> from the given stream
        /// </summary>
        /// <param name="s">The stream to parse from.</param>
        /// <returns>A new <see cref="MtlFile"/>.</returns>
        public MtlFile Parse(Stream s)
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
            private static readonly char[] s_whitespaceChars = new char[] { ' ' };

            private readonly List<MaterialDefinition> _definitions = new List<MaterialDefinition>();
            private MaterialDefinition _currentDefinition;
            private Dictionary<string, string> DictMatDef = new Dictionary<string, string> { };
            private int _currentLine;
            private string _currentLineText;

            public void Process(string line)
            {
                _currentLine++;
                _currentLineText = line;

                string[] pieces = line.Split(s_whitespaceChars, StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length == 0 || pieces[0].StartsWith("#"))
                {
                    return;
                }
                switch (pieces[0].ToLowerInvariant().Trim())
                {
                    case "newmtl":
                        ExpectAtLeast(pieces, 1, "newmtl");
                        FinalizeCurrentMaterial();
                        if (!DictMatDef.ContainsKey(pieces[1]))
                        {
                            DictMatDef.Add(pieces[1], pieces[1]);
                            _currentDefinition = new MaterialDefinition(pieces[1]);
                        }
                        break;
                    case "ka":
                        ExpectExactly(pieces, 3, "Ka");
                        if (_currentDefinition != null)
                            _currentDefinition.AmbientReflectivity = ParseVector3(pieces[1], pieces[2], pieces[3], "Ka");
                        break;
                    case "kd":
                        ExpectExactly(pieces, 3, "Kd");
                        if (_currentDefinition != null)
                            _currentDefinition.DiffuseReflectivity = ParseVector3(pieces[1], pieces[2], pieces[3], "Kd");
                        break;
                    case "ks":
                        ExpectExactly(pieces, 3, "Ks");
                        if (_currentDefinition != null)
                            _currentDefinition.SpecularReflectivity = ParseVector3(pieces[1], pieces[2], pieces[3], "Ks");
                        break;
                    case "ke": // Non-standard?
                        ExpectExactly(pieces, 3, "Ke");
                        if (_currentDefinition != null)
                            _currentDefinition.EmissiveCoefficient = ParseVector3(pieces[1], pieces[2], pieces[3], "Ks");
                        break;
                    case "tf":
                        ExpectExactly(pieces, 3, "Tf");
                        if (_currentDefinition != null)
                            _currentDefinition.TransmissionFilter = ParseVector3(pieces[1], pieces[2], pieces[3], "Tf");
                        break;
                    case "illum":
                        ExpectExactly(pieces, 1, "illum");
                        if (_currentDefinition != null)
                            _currentDefinition.IlluminationModel = ParseInt(pieces[1], "illum");
                        break;
                    case "d": // "Dissolve", or opacity
                        ExpectExactly(pieces, 1, "d");
                        if (_currentDefinition != null)
                            _currentDefinition.Opacity = ParseFloat(pieces[1], "d");
                        break;
                    case "tr": // Transparency
                        ExpectExactly(pieces, 1, "Tr");
                        if (_currentDefinition != null)
                            _currentDefinition.Opacity = 1 - ParseFloat(pieces[1], "Tr");
                        break;
                    case "ns":
                        ExpectExactly(pieces, 1, "Ns");
                        if (_currentDefinition != null)
                            _currentDefinition.SpecularExponent = ParseFloat(pieces[1], "Ns");
                        break;
                    case "sharpness":
                        ExpectExactly(pieces, 1, "sharpness");
                        if (_currentDefinition != null)
                            _currentDefinition.Sharpness = ParseFloat(pieces[1], "sharpness");
                        break;
                    case "ni": // "Index of refraction"
                        ExpectExactly(pieces, 1, "Ni");
                        if (_currentDefinition != null)
                            _currentDefinition.OpticalDensity = ParseFloat(pieces[1], "Ni");
                        break;
                    case "map_ka":
                        ExpectExactly(pieces, 1, "map_ka");
                        if (_currentDefinition != null)
                            _currentDefinition.AmbientTexture = pieces[1];
                        break;
                    case "map_kd":
                        ExpectExactly(pieces, 1, "map_kd");
                        if (_currentDefinition != null)
                            _currentDefinition.DiffuseTexture = pieces[1];
                        break;
                    case "map_ks":
                        ExpectExactly (pieces, 1, "map_ks");
                        if (_currentDefinition != null)
                            _currentDefinition.SpecularColorTexture = pieces [1];
                        break;
                    case "map_bump":
                    case "bump":
                        ExpectExactly(pieces, 1, "map_bump");
                        if (_currentDefinition != null)
                            _currentDefinition.BumpMap = pieces[1];
                        break;
                    case "map_d":
                        ExpectExactly(pieces, 1, "map_d");
                        if (_currentDefinition != null)
                            _currentDefinition.AlphaMap = pieces[1];
                        break;
                    case "map_ns":
                        ExpectExactly(pieces, 1, "map_ns");
                        if (_currentDefinition != null)
                            _currentDefinition.SpecularHighlightTexture = pieces[1];
                        break;


                    default:
                        throw new ObjParseException(
                            string.Format("An unsupported line-type specifier, '{0}', was used on line {1}, \"{2}\"",
                            pieces[0],
                            _currentLine,
                            _currentLineText));
                }
            }

            private void FinalizeCurrentMaterial()
            {
                if (_currentDefinition != null)
                {
                    _definitions.Add(_currentDefinition);
                    _currentDefinition = null;
                }
            }

            public void EndOfFileReached()
            {
                FinalizeCurrentMaterial();
            }

            public MtlFile FinalizeFile()
            {
                return new MtlFile(_definitions);
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

            private float ParseFloat(string intStr, string location)
            {
                try
                {
                    float f = float.Parse(intStr, CultureInfo.InvariantCulture);
                    return f;
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
                    throw new MtlParseException(message);
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
                    throw new MtlParseException(message);
                }
            }

            private MtlParseException CreateParseException(string location, Exception e)
            {
                string message = string.Format("An error ocurred while parsing {0} on line {1}, \"{2}\"", location, _currentLine, _currentLineText);
                return new MtlParseException(message, e);
            }
        }
    }

    public class MtlParseException : Exception
    {
        public MtlParseException(string message) : base(message)
        {
        }

        public MtlParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents a parsed MTL definition file.
    /// </summary>
    public class MtlFile
    {
        /// <summary>
        /// Gets a mapping of all <see cref="MaterialDefinition"/>s contained in this <see cref="MtlFile"/>.
        /// </summary>
        public IReadOnlyDictionary<string, MaterialDefinition> Definitions { get; }

        /// <summary>
        /// Constructs a new <see cref="MtlFile"/> from pre-parsed material definitions.
        /// </summary>
        /// <param name="definitions">A collection of material definitions.</param>
        public MtlFile(IEnumerable<MaterialDefinition> definitions)
        {
            Definitions = definitions.ToDictionary(def => def.Name);
        }
    }

    /// <summary>
    /// An individual material definition from a Wavefront MTL file.
    /// </summary>
    public class MaterialDefinition
    {
        internal MaterialDefinition(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Vector3 AmbientReflectivity { get; internal set; }
        public Vector3 DiffuseReflectivity { get; internal set; }
        public Vector3 SpecularReflectivity { get; internal set; }
        public Vector3 EmissiveCoefficient { get; internal set; }
        public Vector3 TransmissionFilter { get; internal set; }
        public int IlluminationModel { get; internal set; }
        public float Opacity { get; internal set; }
        public float Transparency => 1 - Opacity;
        public float SpecularExponent { get; internal set; }
        public float Sharpness { get; internal set; }
        public float OpticalDensity { get; internal set; }

        public string AmbientTexture { get; internal set; }
        public string DiffuseTexture { get; internal set; }
        public string SpecularColorTexture { get; internal set; }
        public string SpecularHighlightTexture { get; internal set; }
        public string AlphaMap { get; internal set; }
        public string BumpMap { get; internal set; }
        public string DisplacementMap { get; internal set; }
        public string StencilDecalTexture { get; internal set; }
    }
}
