using Microsoft.VisualBasic.FileIO;

namespace PixelShiftBatchProcessor.Tests
{
    [TestClass]
    public class HelperTests
    {
        [TestMethod]
        [DynamicData(nameof(GetSpeicherbedarfInByteTestData))]
        public void GetSpeicherbedarfInByte_GueltigeWerte(int feldtyp, int expected)
        {
            var result = Helper.GetSpeicherbedarfInByte((uint)feldtyp);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DynamicData(nameof(GetSpeicherbedarfInByteTestDataUngueltig), DynamicDataSourceType.Method)]
        public void GetSpeicherbedarfInByte_UngueltigeWerte(int input)
        {
            Assert.ThrowsException<ArgumentException>(() => Helper.GetSpeicherbedarfInByte((uint)input), "Input: " + input.ToString());
        }

        public static IEnumerable<object[]> GetSpeicherbedarfInByteTestData
        {
            get
            {
                yield return new object[] { Feldtyp.BYTE, 1 };
                yield return new object[] { Feldtyp.ASCII, 1 };
                yield return new object[] { Feldtyp.SHORT, 2 };
                yield return new object[] { Feldtyp.LONG, 4 };
                yield return new object[] { Feldtyp.RATIONAL, 8 };
                yield return new object[] { Feldtyp.SBYTE, 1 };
                yield return new object[] { Feldtyp.UNDEFINED, 1 };
                yield return new object[] { Feldtyp.SSHORT, 2 };
                yield return new object[] { Feldtyp.SLONG, 4 };
                yield return new object[] { Feldtyp.SRATIONAL, 8 };
                yield return new object[] { Feldtyp.FLAOT, 4 };
                yield return new object[] { Feldtyp.DOUBLE, 8 };
            }
        }

        public static IEnumerable<object[]> GetSpeicherbedarfInByteTestDataUngueltig()
        {
            var sampleSize = 500;
            var random = new Random();

            for (int i = 0; i < sampleSize; i++)
            {
                var randomValue = random.Next(13, 10000);
                yield return new object[] { randomValue }; 
            }
        }
    }
}