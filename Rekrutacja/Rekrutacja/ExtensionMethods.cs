using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rekrutacja
{
    static class ExtensionMethods
    {
        public static int ConvertToInt(this string str)
        {
            int result = 0;
            for (int i = 0; i < str.Length; i++) 
            { 
                result = 10 * result + (str[i] - '0');
            }
            return result;
        }
    }
}
