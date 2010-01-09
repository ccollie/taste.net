using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Taste.Common
{
    // Stub Class
    public class Properties : Dictionary<string, string>
    {
        Dictionary<string, string> values = new Dictionary<string,string>();

        public Properties()
        {
        }

        public string getProperty(string key)
        {
            string value = null;
            return TryGetValue(key, out value) ? value : null;            
        }

        public void Load(Stream str)
        {
        }
    }
}
