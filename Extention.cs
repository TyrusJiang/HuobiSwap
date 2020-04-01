using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuobiSwap
{
    public static class Extention
    {
        public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object value)
        {
            if (value == null)
            {

            }
            else if (value is string && (string)value == "")
            {

            }
            else
            {
                parameters.Add(key, value);
            }
        }

        public static string AddOptionalParas(string resource, string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return resource;
            }
            else
            {
                if (resource.Contains('?'))
                {
                    resource = resource + "&" + key + "=" + value;
                }
                else
                {
                    resource = resource + "?" + key + "=" + value;
                }
                return resource;
            }
        }

        public static string AddPostOptionalParas(string resource, string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return resource;
            }
            else
            {
                resource = resource + "&" + key + "=" + value;
                return resource;
            }
        }
    }
}
