using System;

namespace VideoToMP3
{
    public class FileParser
    {
        public String FullName { get; set; }
        public String Name { get; set; }
        public String Extension { get; set; }

        public FileParser(String FullName, String Name, String Extension)
        {
            this.FullName = FullName;
            this.Name = Name;
            this.Extension = Extension;
        }
    }
}
