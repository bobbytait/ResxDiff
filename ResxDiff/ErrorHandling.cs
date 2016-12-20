using System;
using System.Text;


namespace ResxDiff
{
    public static class ErrorHandling
    {
        public static bool OutputError(string message, Exception e = null)
        {
            StringBuilder output = new StringBuilder(" [ERROR] " + message);

            if (e != null)
            {
                output.Append("\n" + e.Message + "\n" + e.StackTrace);
            }

            return false;
        }
    }
}
