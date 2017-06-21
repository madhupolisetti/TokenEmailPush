using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenEmailPush
{
    public class Attachment
    {
        private string _name = string.Empty;
        private string _path = string.Empty;
        private string _contentType = "application/unknown";
        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }
        public string Path
        {
            get { return this._path; }
            set { this._path = value; }
        }
        public string ContentType
        {
            get 
            {
                Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(System.IO.Path.GetExtension(this._path).ToLower());
                if (regKey != null && regKey.GetValue("Content Type") != null)
                    this._contentType = regKey.GetValue("Content Type").ToString();
                return this._contentType; 
            }
        }
    }
}
