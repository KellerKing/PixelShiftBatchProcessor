using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelShiftBatchProcessor.Tiff.Image_File_Directory
{
    /// <summary>
    /// Klasse Repräsentiert ein Image File Directory.
    /// 
    /// 
    /// </summary>
    internal class Ifd<T>
    {
        public ushort MyProperty { get; set; }
        public  T? Value { get; set; }
    }
}
