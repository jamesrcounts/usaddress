namespace AddressParser
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Contains the fields that were extracted by the <see cref="AddressParser"/> object.
    /// </summary>
    public class AddressParseResult
    {
        /// <summary>
        /// The street line.
        /// </summary>
        private string streetLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressParseResult"/> class.
        /// </summary>
        /// <param name="fields">The fields that were parsed.</param>
        internal AddressParseResult(Dictionary<string, string> fields)
        {
            if (fields == null)
            {
                throw new ArgumentNullException("fields");
            }

            var type = this.GetType();
            foreach (var pair in fields)
            {
                var bindingFlags = 
                    BindingFlags.Instance | 
                    BindingFlags.Public | 
                    BindingFlags.IgnoreCase;
                var propertyInfo = type.GetProperty(pair.Key, bindingFlags);
                if (propertyInfo != null)
                {
                    var methodInfo = propertyInfo.GetSetMethod(true);
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(this, new[] { pair.Value });
                    }
                }
            }
        }

        /// <summary>
        /// Gets the city name.
        /// </summary>
        public string City
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the house number.
        /// </summary>
        public string Number
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the predirectional, such as "N" in "500 N Main St".
        /// </summary>
        public string Predirectional
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the postdirectional, such as "NW" in "500 Main St NW".
        /// </summary>
        public string Postdirectional
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the state or territory.
        /// </summary>
        public string State
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the street, such as "Main" in "500 N Main St".
        /// </summary>
        public string Street
        {
            get;
            private set;
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
                if (this.streetLine == null)
                {
                    var streetLine = string.Join(
                        " ",
                        new[] {
                            this.Number,
                            this.Predirectional,
                            this.Street,
                            this.Suffix,
                            this.Postdirectional,
                            this.SecondaryUnit,
                            this.SecondaryNumber
                    });
                    streetLine = Regex
                        .Replace(streetLine, @"\ +", " ")
                        .Trim();
                    return streetLine;
                }

                return this.streetLine;
            }

            private set
            {
                this.streetLine = value;
            }
        }

        /// <summary>
        /// Gets the street suffix, such as "ST" in "500 N MAIN ST".
        /// </summary>
        public string Suffix
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the secondary unit, such as "APT" in "500 N MAIN ST APT 3".
        /// </summary>
        public string SecondaryUnit
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the secondary unit, such as "3" in "500 N MAIN ST APT 3".
        /// </summary>
        public string SecondaryNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ZIP code.
        /// </summary>
        public string Zip
        {
            get;
            private set;
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
    }
}
