using System;
using System.IO;

namespace PixelShiftBatchProcessor
{
    public class Programm
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Lade Raw Dateien");
            
            var files = GetFiles(Environment.CurrentDirectory);

            foreach (var file in files)
            {
                Console.WriteLine($"Verarbeite Datei: {file}");
                var fileHandler = new FileHandlerRaw(file);
                fileHandler.Process();
            }

            Console.ReadKey();
        }
      

        private static string[] GetFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.arw", SearchOption.AllDirectories);
            return files;
        }
    }
}


