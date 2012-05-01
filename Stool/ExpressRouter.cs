using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;

namespace Stool
{
    public class ExpressRouter : IRouteConstraint
    {
        /// <summary>
        /// Shamelessly borrowed from expressjs
        /// https://github.com/visionmedia/express/blob/master/lib/utils.js
        /// </summary>
        /// <param name="path"></param>
        /// <param name="keys"></param>
        /// <param name="sensitive"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        public static Regex GetPathRegex(object path, List<dynamic> keys, bool sensitive, bool strict)
        {
            if (path is Regex)
                return path as Regex;
            var enumerable = path as IEnumerable<string>;
            if (enumerable != null)
                return new Regex("(" + string.Join("|", enumerable) + ")");
            var sp = path as string;
            sp = string.Concat(sp, strict ? "" : "/?");
            sp = Regex.Replace(sp, @"\/\(", "(?:/", RegexOptions.ECMAScript);
            sp = Regex.Replace(sp, @"(\/)?(\.)?:(\w+)(?:(\(.*?\)))?(\?)?", m =>
                                                                               {
                                                                                   var slash = m.Groups[1].Value;
                                                                                   var format = m.Groups[2].Value;
                                                                                   var key = m.Groups[3].Value;
                                                                                   var capture = m.Groups[4].Value;
                                                                                   var optional = !string.IsNullOrEmpty(m.Groups[5].Value);
                                                                                   keys.Add(new {name = key, optional});
                                                                                   return ""
                                                                                       + (optional ? "" : slash)
                                                                                       + "(?:"
                                                                                       + (optional ? slash : "")
                                                                                       + format
                                                                                       + (string.IsNullOrEmpty(capture)
                                                                                            ? string.IsNullOrEmpty(format)
                                                                                               ? "([^/.]+?)"
                                                                                               : "([^/]+?)"
                                                                                               : capture)
                                                                                       + ")"
                                                                                       + (optional ? "?" : "");
                                                                               }, RegexOptions.ECMAScript);
            sp = Regex.Replace(sp, @"([\/.])", "\\$1");
            sp = Regex.Replace(sp, @"\*", "(.*)");

            return new Regex("^" + sp + "$", !sensitive ? RegexOptions.IgnoreCase | RegexOptions.ECMAScript : RegexOptions.ECMAScript);
        }

        public static void Test()
        {
            var keys = new List<dynamic>();
            var regex = GetPathRegex("user/:id", keys, false, false);
            Console.WriteLine(regex);
            Console.WriteLine("user/12 is matched: {0}", regex.IsMatch("user/12"));
            Console.WriteLine("user is matched: {0}", regex.IsMatch("User"));

            keys.Clear();
            var route = "users/:limit([0-9]+)?/:status?";
            Console.WriteLine("testing " + route);
            regex = GetPathRegex(route, keys, false, false);
            Console.WriteLine("Route translated to: " + regex);
            var tests = new[] {"users/10/active", "users/active"};
            foreach(var test in tests)
            {
                Console.WriteLine(test + ": " + regex.IsMatch(test));
                var match = regex.Match(test);
                for(var i = 0; i < keys.Count; i++)
                {
                    var g = match.Groups[i+1];
                    var key = keys[i];
                    Console.WriteLine("{0}: {1}", key.name, g);
                }
            }
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if(routeDirection == RouteDirection.UrlGeneration)
            {
                return false;
            }

            var keys = new List<dynamic>();
            var regex = GetPathRegex(route.Url, keys, false, false);
            var match = regex.Match(httpContext.Request.Path);
            if(!match.Success) return false;
            for (int i = 1, len = match.Groups.Count; i < len; i++ )
            {
                if(keys.Count >= i)
                {
                    var key = keys[i - 1];
                    httpContext.Items.Add(key, match.Groups[i]);
                }
                else httpContext.Items.Add(i, match.Groups[i]);
            }
            return true;
        }
    }
}
