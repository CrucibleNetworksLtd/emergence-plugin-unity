using System.IO;

namespace EmergenceEVMLocalServer.Utils
{
    public class FileUtils
    {
        public static bool SaveStringIfFileDoesNotExist(string path, string content)
        {
            string fileName = path;
            //Check if the file does not exist
            if (!File.Exists(fileName))
            {
                using (StreamWriter writer = File.CreateText(fileName))
                {
                    writer.Write(content);
                    return true;
                }
            }
            return false;
        }

    }
}
