using PixelShiftBatchProcessor.Tiff.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelShiftBatchProcessor.Tests
{
    [TestClass]
    public class HeaderParserTiffTests
    {
        [TestMethod]
        public void ParseHeader_GueltigeWerte_LittleEndianTiff()
        {
            var bytes = new byte[] { 73, 73, 42, 0, 8, 0, 0, 0 };
            var expected = new HeaderDto
            {
                ByteOffset = 8,
                IsLittleEndian = true,
                IsTiff = true
            };

            var result = HeaderParserTiff.ParseHeader(bytes.AsSpan());

            Assert.AreEqual(expected.IsTiff, result.IsTiff);
            Assert.AreEqual(expected.IsLittleEndian, result.IsLittleEndian);
            Assert.AreEqual(expected.ByteOffset, result.ByteOffset);
        }

        [TestMethod]
        public void ParseHeader_GueltigeWerte_TiffZuKlein()
        {
            var bytes = new byte[] { 73, 73, 42, 0, 8, 0, 0 };
            var expected = new HeaderDto
            {
                IsTiff = false
            };

            var result = HeaderParserTiff.ParseHeader(bytes.AsSpan());

            Assert.AreEqual(expected.IsTiff, result.IsTiff);
        }
        [TestMethod]
        public void ParseHeader_GueltigeWerte_BigEndianTiff()
        {
            var bytes = new byte[] { 77, 77, 0, 42, 0, 0, 0, 8 };
            var expected = new HeaderDto
            {
                ByteOffset = 8,
                IsLittleEndian = false,
                IsTiff = true
            };

            var result = HeaderParserTiff.ParseHeader(bytes.AsSpan());

            Assert.AreEqual(expected.IsTiff, result.IsTiff);
        }
        [TestMethod]
        public void ParseHeader_GueltigeWerte_KeinTiffWeilFalscheMagicNumber()
        {
            var bytes = new byte[] { 73, 73, 43, 0, 8, 0, 0, 0 };
            var expected = new HeaderDto
            {
                IsTiff = false
            };

            var result = HeaderParserTiff.ParseHeader(bytes.AsSpan());

            Assert.AreEqual(expected.IsTiff, result.IsTiff);
        }

    }
}
