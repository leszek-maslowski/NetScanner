using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetScanner
{
    public class Serializer
    {
        public static void Store<T>(T obj, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(T));
                xmls.Serialize(fs, obj);
            }
        }

        public static void Store(IEnumerable<string> collection, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (string s in collection)
                    sw.WriteLine(s);
            }
        }

        public static T Restore<T>(string path)
        {
            if (!File.Exists(path))
                return default(T);

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(T));
                return (T)xmls.Deserialize(fs);
            }
        }
    }
}
