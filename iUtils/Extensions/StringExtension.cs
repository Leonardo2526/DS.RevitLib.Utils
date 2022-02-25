using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace iUtils.Extensions
{
    public static class StringExtension
    {
        public static bool ContainsAnyInList(this string str, List<string> list, bool ignoreCase)
        {
            if (ignoreCase)
            {
                list =  list.Select(x => x.ToLower()).ToList();
                return ContainsAnyInList(str.ToLower(), list);
            }
            else
            {
                return ContainsAnyInList(str, list);
            }
        }
        private static bool ContainsAnyInList(this string str, List<string> list)
        {
            foreach (var item in list)
            {
                if(!string.IsNullOrEmpty(item))
                    if (str.Contains(item))
                        return true;
            }
            return false;
        }
    }
}
