// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Extension methods for integers.
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        /// Converts a number to text - supports up to 9999.
        /// </summary>
        /// <param name="i">The number to convert.</param>
        /// <returns>The number as text.</returns>
        public static string GetText(this int i)
        {
            var singleDigits = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
            var tensTeens = new[] { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var twoDigits = new[] { "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            var builder = new StringBuilder();
            var text = i.ToString(CultureInfo.InvariantCulture).Reverse().ToArray();
            if (text.Length == 4)
            {
                builder.Append(singleDigits[text[3] - '0']).Append(" thousand");
            }

            if (text.Length >= 3 && text[2] != '0')
            {
                builder.Append(builder.Length > 0 ? " " : string.Empty);
                builder.Append(singleDigits[text[2] - '0']).Append(" hundred");
            }

            if (text.Length >= 2 && text[1] != '0')
            {
                builder.Append(builder.Length > 0 ? " and " : string.Empty);
                if (text[1] == '1')
                {
                    builder.Append(tensTeens[text[0] - '0']);
                }
                else
                {
                    builder.Append(twoDigits[text[1] - '2']);
                    if (text[0] != '0')
                    {
                        builder.Append(' ').Append(singleDigits[text[0] - '0']);
                    }
                }
            }
            else if (text.Length == 1 || text[0] != '0')
            {
                builder.Append(builder.Length > 0 ? " and " : string.Empty);
                builder.Append(singleDigits[text[0] - '0']);
            }

            return builder.ToString();
        }
    }
}
