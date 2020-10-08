using System;
using System.IO;

namespace SaferThanLight {
    public class Data {
        public static String SaveDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "FasterThanLight");
        public static String SaveFile => Path.Combine(SaveDirectory, "continue.sav");
        public static String MetaFile => Path.Combine(SaveDirectory, "meta.json");
    }
}
