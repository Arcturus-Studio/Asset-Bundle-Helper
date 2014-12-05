//From: http://stackoverflow.com/q/1577822
//Author: http://stackoverflow.com/users/69809/groo
using System.Collections.Generic;
public static class IEnumerableExt
{
    /// <summary>
    /// Wraps this object instance into an IEnumerable&lt;T&gt;
    /// consisting of a single item.
    /// </summary>
    /// <typeparam name="T"> Type of the wrapped object.</typeparam>
    /// <param name="item"> The object to wrap.</param>
    /// <returns>
    /// An IEnumerable&lt;T&gt; consisting of a single item.
    /// </returns>
    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }
}
