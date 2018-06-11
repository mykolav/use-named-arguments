using System.Linq;

namespace UseNamedArguments.Support
{
    internal static class GenericExtensions
    {
        public static bool In<T>(this T value, params T[] collection)
        {
            var contains = collection.Contains(value);
            return contains;
        }
    }
}