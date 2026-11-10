using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.Data
{
    public static class TextLogSave
    {
        public static void AppendToTextLog(string log, string filePath)
        {
            try
            {
                string dir = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);      

                using (StreamWriter writer = File.AppendText(filePath))   
                {
                    writer.WriteLine();
                    writer.WriteLine(log);
                    writer.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing text log: {ex.Message}");
            }
        }
    }
}
