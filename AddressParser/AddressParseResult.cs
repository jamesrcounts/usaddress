// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddressParseResult.cs" company="Jim Counts">
//     Copyright (c) Jim Counts 2013.
// </copyright>
// <summary>
//   Defines the Components type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace USAddress
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Contains the fields that were extracted by the <see cref="AddressParser"/> object.
    /// </summary>
    public class AddressParseResult
    {
        /// <summary>
        /// The fields
        /// </summary>
        private readonly Dictionary<string, string> fields;

        /// <summary>
        /// The street line.
        /// </summary>
        private string streetLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressParseResult"/> class.
        /// </summary>
        /// <param name="fields">The fields that were parsed.</param>
        public AddressParseResult(Dictionary<string, string> fields)
        {
            this.fields = fields ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the city name.
        /// </summary>
        public string City
        {
            get
            {
                return this.GetField(Components.City);
            }
        }

        /// <summary>
        /// Gets the house number.
        /// </summary>
        public string Number
        {
            get
            {
                return this.GetField(Components.Number);
            }
        }

        /// <summary>
        /// Gets the post-directional, such as "NW" in "500 Main St NW".
        /// </summary>
        public string Postdirectional
        {
            get
            {
                return this.GetField(Components.Postdirectional);
            }
        }

        /// <summary>
        /// Gets the pre-directional, such as "N" in "500 N Main St".
        /// </summary>
        public string Predirectional
        {
            get
            {
                return this.GetField(Components.Predirectional);
            }
        }

        /// <summary>
        /// Gets the secondary unit, such as "3" in "500 N MAIN ST APT 3".
        /// </summary>
        public string SecondaryNumber
        {
            get
            {
                return this.GetField(Components.SecondaryNumber);
            }
        }

        /// <summary>
        /// Gets the secondary unit, such as "APT" in "500 N MAIN ST APT 3".
        /// </summary>
        public string SecondaryUnit
        {
            get
            {
                return this.GetField(Components.SecondaryUnit);
            }
        }

        /// <summary>
        /// Gets the state or territory.
        /// </summary>
        public string State
        {
            get
            {
                return this.GetField(Components.State);
            }
        }

        /// <summary>
        /// Gets the name of the street, such as "Main" in "500 N Main St".
        /// </summary>
        public string Street
        {
            get
            {
                return this.GetField(Components.Street);
            }
        }

        /// <summary>
        /// Gets the full street line, such as "500 N Main St" in "500 N Main St".
        /// This is typically constructed by combining other elements in the parsed result.
        /// However, in some special circumstances, most notably APO/FPO/DPO addresses, the
        /// street line is set directly and the other elements will be null.
        /// </summary>
        public string StreetLine
        {
            get
            {
                if (this.streetLine != null)
                {
                    return this.streetLine;
                }

                this.streetLine = this.GetField(Components.StreetLine);
                if (!string.IsNullOrWhiteSpace(this.streetLine))
                {
                    return this.streetLine;
                }

                return this.streetLine = this.CreateStreetLine();
            }
        }

        /// <summary>
        /// Gets the street suffix, such as "ST" in "500 N MAIN ST".
        /// </summary>
        public string Suffix
        {
            get
            {
                return this.GetField(Components.Suffix);
            }
        }

        /// <summary>
        /// Gets the ZIP code.
        /// </summary>
        public string Zip
        {
            get
            {
                return this.GetField(Components.Zip);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}; {1}, {2}  {3}",
                this.StreetLine,
                this.City,
                this.State,
                this.Zip);
        }

        /// <summary>
        /// Creates the street line.
        /// </summary>
        /// <returns>A street line, assembled from various components that belong on line 1 of the address.</returns>
        private string CreateStreetLine()
        {
            var line = string.Join(
                " ",
                this.Number,
                this.Predirectional,
                this.Street,
                this.Suffix,
                this.Postdirectional,
                this.SecondaryUnit,
                this.SecondaryNumber);
            return Regex.Replace(line, @"\ +", " ").Trim();
        }

        /// <summary>
        /// Gets the field.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The field value if found; otherwise an empty string.</returns>
        private string GetField(string key)
        {
            return !this.fields.ContainsKey(key) ? string.Empty : this.fields[key];
        }

        /// <summary>
        /// Contracts that are true throughout the life of the class instance.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by the CodeContract analyzer")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "CodeContract analyzer requires this to be an instance member.")]
        [ExcludeFromCodeCoverage]
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(this.fields != null);
        }
    }
}