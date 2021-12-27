using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace Rhino.Toolkit.PlugInLoader
{
    public class Settings
    {
        string FilePath { get; set; }

        public Settings()
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Rhino.PlugInLoader");
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            FilePath = Path.Combine(folderPath, "plugin.json");
        }

        public IList<PlugIn> Load()
        {
            if(!File.Exists(FilePath))
            {
                return null;
            }
            return JsonSerializer.Deserialize<List<PlugIn>>(File.ReadAllText(FilePath));
        }

        public void Save(IList<PlugIn> plugIns)
        {
            string jsonString = JsonSerializer.Serialize(plugIns);
            File.WriteAllText(FilePath, jsonString);
        }
    }
}
