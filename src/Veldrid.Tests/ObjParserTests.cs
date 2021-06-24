using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Veldrid.Utilities;
using Xunit;

namespace Veldrid.Tests
{
    public class ObjParserTests
    {
        public static IEnumerable<object[]> GetModelFiles()
        {
            string loc = Assembly.GetExecutingAssembly().Location;
            string src = Path.GetFullPath(Path.Combine(loc, "..", "..", "..", "..", "..", "src"));

            {
                string neodemo = Path.Combine(src, "NeoDemo");
                string neoDemoModels = Path.Combine(neodemo, "Assets", "Models");
                string sponza = Path.Combine(neoDemoModels, "SponzaAtrium", "sponza.obj");
                yield return new object[] { sponza };
            }

            {
                string vrSample = Path.Combine(src, "Veldrid.VirtualReality.Sample");
                string cat = Path.Combine(vrSample, "cat", "cat.obj");
                yield return new object[] { cat };
            }
        }

        [Theory]
        [MemberData(nameof(GetModelFiles))]
        public void ParseStream(string file)
        {
            ObjParser parser = new ObjParser();
            using (FileStream stream = File.OpenRead(file))
            {
                parser.Parse(stream);
                Assert.Throws<ArgumentException>(() => parser.Parse(stream));
            }

            using (FileStream stream = File.OpenRead(file))
            {
                parser.Parse(stream);
                Assert.Throws<ArgumentException>(() => parser.Parse(stream));
            }
        }

        [Theory]
        [MemberData(nameof(GetModelFiles))]
        public void ParseLines(string file)
        {
            string[] lines = File.ReadAllLines(file);

            ObjParser parser = new ObjParser();
            parser.Parse(lines);

            parser.Parse(lines);
        }
    }
}
