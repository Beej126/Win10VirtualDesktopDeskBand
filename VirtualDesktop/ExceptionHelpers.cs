using System;
using System.Collections.Generic;
using System.Linq;

internal static class ExceptionHelpers
{
    public static string GetNestedMessagesCombined(this Exception ex, string delimiter = "\r\n") => string.Join(delimiter, GetNestedMessages(ex));
    public static IEnumerable<string> GetNestedMessages(this Exception ex) =>
        InnerWalker(ex).Select(e => e.Message).Where(m=> m != "One or more errors occurred.");

    public static IEnumerable<Exception> InnerWalker(this Exception ex)
    {
        while (ex != null)
        {
            yield return ex;
            ex = ex.InnerException;
        }
    }
}
