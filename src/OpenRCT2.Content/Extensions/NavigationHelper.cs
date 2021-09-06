using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace OpenRCT2.Content.Extensions
{
    public static class NavigationManagerExtensions
    {
        public static void NavigateWithQueryParam(this NavigationManager navManager, string key, string value)
        {
            var uri = AddOrReplaceQueryStringParameter(navManager.Uri, key, value);
            navManager.NavigateTo(uri);
        }

        public static bool TryGetQueryString<T>(this NavigationManager navManager, string key, out T value)
        {
            var uri = navManager.ToAbsoluteUri(navManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var valueFromQueryString))
            {
                if (typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, out var valueAsInt))
                {
                    value = (T)(object)valueAsInt;
                    return true;
                }

                if (typeof(T) == typeof(string))
                {
                    value = (T)(object)valueFromQueryString.ToString();
                    return true;
                }

                if (typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, out var valueAsDecimal))
                {
                    value = (T)(object)valueAsDecimal;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static string GetUriWithQueryParam(this NavigationManager navManager, string key, string value)
        {
            return AddOrReplaceQueryStringParameter(navManager.Uri, key, value);
        }

        public static string AddOrReplaceQueryStringParameter(string uri, string key, string value)
        {
            var queryStart = uri.LastIndexOf('?');
            if (queryStart == -1)
            {
                return value == null ? uri : $"{uri}?{key}={value}";
            }
            else
            {
                var baseUri = uri.Substring(0, queryStart);
                var args = uri[(queryStart + 1)..].Split('&').ToList();
                var found = false;
                for (int i = 0; i < args.Count; i++)
                {
                    var equalsIndex = args[i].IndexOf('=');
                    var argKey = equalsIndex == -1 ? args[i] : args[i].Substring(0, equalsIndex);
                    if (argKey == key)
                    {
                        if (value == null || found)
                        {
                            args.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            args[i] = $"{argKey}={value}";
                        }
                        found = true;
                    }
                }
                if (!found && value != null)
                {
                    args.Add($"{key}={value}");
                }
                return args.Count == 0 ? baseUri : $"{baseUri}?{string.Join('&', args)}";
            }
        }
    }
}
