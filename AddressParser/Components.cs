// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Components.cs" company="Jim Counts">
//     Copyright (c) Jim Counts 2013.
// </copyright>
// <summary>
//   Defines the Components type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace USAddress
{
    /// <summary>
    /// Constants to describe address parts.
    /// </summary>
    public abstract class Components
    {
        /// <summary>
        /// The city
        /// </summary>
        public const string City = "CITY";

        /// <summary>
        /// The number
        /// </summary>
        public const string Number = "NUMBER";

        /// <summary>
        /// The post-directional
        /// </summary>
        public const string Postdirectional = "POSTDIRECTIONAL";

        /// <summary>
        /// The pre-directional
        /// </summary>
        public const string Predirectional = "PREDIRECTIONAL";

        /// <summary>
        /// The secondary number
        /// </summary>
        public const string SecondaryNumber = "SECONDARYNUMBER";

        /// <summary>
        /// The secondary unit
        /// </summary>
        public const string SecondaryUnit = "SECONDARYUNIT";

        /// <summary>
        /// The state
        /// </summary>
        public const string State = "STATE";

        /// <summary>
        /// The street
        /// </summary>
        public const string Street = "STREET";

        /// <summary>
        /// The street line
        /// </summary>
        public const string StreetLine = "STREETLINE";

        /// <summary>
        /// The suffix
        /// </summary>
        public const string Suffix = "SUFFIX";

        /// <summary>
        /// The zip
        /// </summary>
        public const string Zip = "ZIP";
    }
}