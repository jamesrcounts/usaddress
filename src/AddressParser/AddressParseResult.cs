// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddressParseResult.cs" company="Jim Counts">
//     Copyright (c) Jim Counts 2013.
// </copyright>
// <summary>
//   Defines the Components type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace USAddress
{
    /// <summary>
    ///     Contains the fields that were extracted by the <see cref="AddressParser" /> object.
    /// </summary>
    public class AddressParseResult
    {
        /// <summary>
        ///     The fields
        /// </summary>
        private readonly Dictionary<string, string> _fields;

        /// <summary>
        ///     The street line.
        /// </summary>
        private string _streetLine;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddressParseResult" /> class.
        /// </summary>
        /// <param name="fields">The fields that were parsed.</param>
        public AddressParseResult(Dictionary<string, string> fields, Match regexMatch = null)
        {
            _fields = fields ?? new Dictionary<string, string>();
            RegexMatch = regexMatch;
        }

        /// <summary>
        ///     Gets the source RegularExpressions.Match object holding positional information in input text.
        /// </summary>
        public Match RegexMatch { get; private set; }

        /// <summary>
        ///     Gets the city name.
        /// </summary>
        public string City => GetField(Components.City);

        /// <summary>
        ///     Gets the house number.
        /// </summary>
        public string Number => GetField(Components.Number);

        /// <summary>
        ///     Gets the post-directional, such as "NW" in "500 Main St NW".
        /// </summary>
        public string Postdirectional => GetField(Components.Postdirectional);

        /// <summary>
        ///     Gets the pre-directional, such as "N" in "500 N Main St".
        /// </summary>
        public string Predirectional => GetField(Components.Predirectional);

        /// <summary>
        ///     Gets the secondary unit, such as "3" in "500 N MAIN ST APT 3".
        /// </summary>
        public string SecondaryNumber => GetField(Components.SecondaryNumber);

        /// <summary>
        ///     Gets the secondary unit, such as "APT" in "500 N MAIN ST APT 3".
        /// </summary>
        public string SecondaryUnit => GetField(Components.SecondaryUnit);

        /// <summary>
        ///     Gets the state or territory.
        /// </summary>
        public string State => GetField(Components.State);

        /// <summary>
        ///     Gets the name of the street, such as "Main" in "500 N Main St".
        /// </summary>
        public string Street => GetField(Components.Street);

        /// <summary>
        ///     Gets the full street line, such as "500 N Main St" in "500 N Main St".
        ///     This is typically constructed by combining other elements in the parsed result.
        ///     However, in some special circumstances, most notably APO/FPO/DPO addresses, the
        ///     street line is set directly and the other elements will be null.
        /// </summary>
        public string StreetLine
        {
            get
            {
                if (_streetLine != null)
                {
                    return _streetLine;
                }

                _streetLine = GetField(Components.StreetLine);
                if (!string.IsNullOrWhiteSpace(_streetLine))
                {
                    return _streetLine;
                }

                return _streetLine = CreateStreetLine();
            }
        }

        /// <summary>
        ///     Gets the street suffix, such as "ST" in "500 N MAIN ST".
        /// </summary>
        public string Suffix => GetField(Components.Suffix);

        /// <summary>
        ///     Gets the ZIP code.
        /// </summary>
        public string Zip => GetField(Components.Zip);

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}; {1}, {2}  {3}",
                StreetLine,
                City,
                State,
                Zip);
        }

        /// <summary>
        ///     Creates the street line.
        /// </summary>
        /// <returns>A street line, assembled from various components that belong on line 1 of the address.</returns>
        private string CreateStreetLine()
        {
            var line = string.Join(
                " ",
                Number,
                Predirectional,
                Street,
                Suffix,
                Postdirectional,
                SecondaryUnit,
                SecondaryNumber);
            return Regex.Replace(line, @"\ +", " ").Trim();
        }

        /// <summary>
        ///     Gets the field.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The field value if found; otherwise an empty string.</returns>
        private string GetField(string key)
        {
            return !_fields.ContainsKey(key) ? string.Empty : _fields[key];
        }
    }
}