using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenEmailPush
{
    public static class ExtensionMethods
    {
        public static bool IsDBNull(this object input)
        {
            return input.Equals(System.DBNull.Value);
        }
    }
}
