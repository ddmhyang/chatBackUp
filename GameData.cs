using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualBackUpApp
{
    public class ResourceItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public static class GameDataManager
    {
        public static List<ResourceItem> Faces = new List<ResourceItem>();
        public static List<ResourceItem> Backgrounds = new List<ResourceItem>();
        public static string ExcelFilePath = "";
    }
}
