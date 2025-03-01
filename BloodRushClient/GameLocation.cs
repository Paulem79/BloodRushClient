using System;
using System.Linq;
using System.Xml.Linq;

namespace BloodRushInstaller.utils
{
    public class GameLocation
    {
        public string path, version, folderDest;
        public bool demo;

        public GameLocation(string path, string version, bool demo)
        {
            this.path = path;
            this.version = version;

            var lastParenSet = this.path.LastIndexOf("/");
            this.folderDest = this.path.Substring(0, lastParenSet > -1 ? lastParenSet : this.path.Count());

            this.demo = demo;
        }
    }
}
