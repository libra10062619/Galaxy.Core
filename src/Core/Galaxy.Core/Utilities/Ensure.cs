using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Core.Utilities
{
    public static class Ensure
    {
        public static void NotNull<T>(T argument, string argumentName = null, string message = null) where T : class
        {
            if (argument == null || argument == default)
            {
                argumentName = argumentName ?? nameof(argument);
                throw new ArgumentNullException(argumentName, message);
            }
        }

        public static void NotEmpty<T>(IEnumerable<T> argument, string argumentName = null, string message = null) where T : class
        {
            NotNull(argument, argumentName, message);

            if (!argument.Any()) throw new ArgumentNullException(argumentName, message);
        }
    }
}
