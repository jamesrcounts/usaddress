// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringHelper.cs" company="Jim Counts">
//     Copyright (c) Jim Counts 2013.
// </copyright>
// <summary>
//   Defines the StringHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Globalization;

namespace USAddress
{
    /// <summary>
    /// String extensions
    /// </summary>
    internal static class StringHelper
    {
        /// <summary>
        /// Generate a string using the supplied format and arguments, with the invariant culture.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>A string formatted in the invariant culture.</returns>
        public static string FormatInvariant(this string format, params object[] arguments)
        {
            if (format == null || arguments == null)
            {
                throw new ArgumentNullException(format == null ? "format" : "arguments");
            }

            return string.Format(CultureInfo.InvariantCulture, format, arguments);
        }
    }
}