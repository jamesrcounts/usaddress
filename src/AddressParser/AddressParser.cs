// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddressParser.cs" company="Jim Counts">
//     Copyright (c) Jim Counts 2013.
// </copyright>
// <summary>
//   Defines the AddressParser type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace USAddress
{
    /// <summary>
    ///     <para>
    ///         This is an attempt at a port of the Perl CPAN module Geo::StreetAddress::US
    ///         to C#. It's a regex-based street address and street intersection parser for the
    ///         United States.
    ///     </para>
    ///     <para>
    ///         The original Perl version was written and is copyrighted by
    ///         Schuyler D. Erle &lt;schuyler@geocoder.us&gt; and is accessible at
    ///         <a href="http://search.cpan.org/~timb/Geo-StreetAddress-US-1.03/US.pm">CPAN</a>.
    ///     </para>
    ///     <para>
    ///         It says that "this library is free software; you can redistribute it and/or modify
    ///         it under the same terms as Perl itself, either Perl version 5.8.4 or, at
    ///         your option, any later version of Perl 5 you may have available."
    ///     </para>
    ///     <para>
    ///         According to the <a href="http://dev.perl.org/licenses/">Perl licensing page</a>,
    ///         that seems to mean you have a choice between GPL V1 (or at your option, a later version)
    ///         or the Artistic License.
    ///     </para>
    /// </summary>
    public class AddressParser
    {
        /// <summary>
        /// In the <see cref="M:addressRegex"/> member, these are the names
        /// of the groups in the result that we care to inspect.
        /// </summary>
        private readonly string[] _fields =
            {
                Components.Number, Components.Predirectional, Components.Street, Components.StreetLine, Components.Suffix,
                Components.Postdirectional, Components.City, Components.State, Components.Zip, Components.SecondaryUnit,
                Components.SecondaryNumber
            };

        /// <summary>
        /// A regular expression that only extracts the street address fields.
        /// </summary>
        private Regex _addressLineRegex;

        /// <summary>
        /// The gigantic regular expression that actually extracts the bits and pieces
        /// from a given address.
        /// </summary>
        private Regex _addressRegex;

        /// <summary>
        /// A combined dictionary of the ranged and not ranged secondary units.
        /// </summary>
        private Dictionary<string, string> _allSecondaryUnits;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressParser"/> class.
        /// </summary>
        public AddressParser()
        {
            CompileRegex = true;
        }

        /// <summary>
        /// Gets the default parser instance.
        /// </summary>
        /// <value>
        /// The default parser.
        /// </value>
        public static AddressParser Default { get; } = new AddressParser();

        /// <summary>
        /// The postal box pattern without the place fields (no city/state/zip)
        /// </summary>
        public static string PostalBoxPatternAddressLineOnly => @"(?<{0}>(P[\.\s]?O[\.\s]?\s?)?BOX\s[0-9]+)".FormatInvariant(Components.StreetLine);

        /// <summary>
        /// Gets the zip pattern.
        /// </summary>
        /// <value>
        /// The zip pattern.
        /// </value>
        public static string ZipPattern => @"\d{5}(?:-?\d{4})?";

        /// <summary>
        /// Gets the regular expression that only extracts the street address fields.
        /// </summary>
        public Regex AddressLineRegex => _addressLineRegex ?? (_addressLineRegex = InitializeAddressLineRegex());

        /// <summary>
        /// Gets the gigantic regular expression that actually extracts the bits and pieces
        /// from a given address.
        /// </summary>
        public Regex AddressRegex => _addressRegex ?? (_addressRegex = InitializeRegex());

        /// <summary>
        /// Gets the pattern to match all known secondary units.
        /// </summary>
        /// <value>
        /// The all secondary unit pattern.
        /// </value>
        public string AllSecondaryUnitPattern => @"
                    (
                        (:?
                            (?: (?:{0} \W*)
                                | (?<{2}>\#)\W*
                            )
                            (?<{3}>[\w-]+)
                        )
                        |{1}
                    ),?
                ".FormatInvariant(
            RangedSecondaryUnitPattern,
            RangelessSecondaryUnitPattern,
            Components.SecondaryUnit,
            Components.SecondaryNumber);

        /// <summary>
        /// Gets a combined dictionary of the ranged and not ranged secondary units.
        /// </summary>
        public Dictionary<string, string> AllUnits => _allSecondaryUnits ?? (_allSecondaryUnits = CombineSecondaryUnits());

        /// <summary>
        /// Gets the city and state pattern.
        /// </summary>
        /// <value>
        /// The city and state pattern.
        /// </value>
        public string CityAndStatePattern => @"
                    (?:
                        (?<{1}>[^\d,]+)\W+
                        (?<{2}>{0})
                    )
                ".FormatInvariant(
            StatePattern,
            Components.City,
            Components.State);

        /// <summary>
        /// Gets or sets a value indicating whether to compile the regular expression objects. Enabled by default.
        /// </summary>
        public bool CompileRegex { get; set; }

        /// <summary>
        /// Gets a map of directional names (north, northeast, etc.) to abbreviations (N, NE, etc.).
        /// </summary>
        public Dictionary<string, string> DirectionalNames { get; } = new Dictionary<string, string>
        {
            { "NORTH", "N" },
            { "NORTHEAST", "NE" },
            { "EAST", "E" },
            { "SOUTHEAST", "SE" },
            { "SOUTH", "S" },
            { "SOUTHWEST", "SW" },
            { "WEST", "W" },
            { "NORTHWEST", "NW" }
        };

        /// <summary>
        /// Gets the pattern to match direction indicators (north, south, east, west, etc.)
        /// </summary>
        /// <value>
        /// The directional pattern.
        /// </value>
        public string DirectionalPattern
        {
            get
            {
                var arguments = DirectionalNames.Values
                    .Select(x => Regex.Replace(x, @"(\w)", @"$1\."))
                    .Concat(DirectionalNames.Keys)
                    .Concat(DirectionalNames.Values)
                    .OrderByDescending(x => x.Length)
                    .Distinct();
                return string.Join("|", arguments);
            }
        }

        /// <summary>
        /// Gets the match options to use with the address regular expression.
        /// </summary>
        public RegexOptions MatchOptions
        {
            get
            {
                var options = RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace
                    | RegexOptions.IgnoreCase;

                if (CompileRegex)
                {
                    options |= RegexOptions.Compiled;
                }

                return options;
            }
        }

        /// <summary>
        /// Gets the pattern to match city, state and zip.
        /// </summary>
        /// <value>
        /// The place pattern.
        /// </value>
        public string PlacePattern => @"
                    (?:{0}\W*)?
                    (?:(?<{2}>{1}))?
                ".FormatInvariant(CityAndStatePattern, ZipPattern, Components.Zip);

        /// <summary>
        /// Gets the post office box pattern.
        /// </summary>
        /// <value>
        /// The postal box pattern.
        /// </value>
        public string PostalBoxPattern => @"# Special case for PO boxes
                    (
                        \W*
                        {0}\W+
                        {1}
                        \W*
                    )".FormatInvariant(PostalBoxPatternAddressLineOnly, PlacePattern);

        /// <summary>
        /// Gets the ranged secondary unit pattern.
        /// </summary>
        /// <value>
        /// The ranged secondary unit pattern.
        /// </value>
        public string RangedSecondaryUnitPattern
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    @"(?<{1}>{0})(?![a-z])",
                    string.Join("|", RangedUnits.Keys.OrderByDescending(x => x.Length)),
                    Components.SecondaryUnit);
            }
        }

        /// <summary>
        /// Gets a map from unit names that require a number after them to their standard forms.
        /// </summary>
        public Dictionary<string, string> RangedUnits { get; } = new Dictionary<string, string>
        {
            {
                @"SU?I?TE",
                "STE"
            },
            {
                @"(?:AP)(?:AR)?T(?:ME?NT)?",
                "APT"
            },
            {
                @"(?:DEP)(?:AR)?T(?:ME?NT)?",
                "DEPT"
            },
            {
                @"RO*M",
                "RM"
            },
            {
                @"FLO*R?",
                "FL"
            },
            {
                @"UNI?T",
                "UNIT"
            },
            {
                @"BU?I?LDI?N?G",
                "BLDG"
            },
            {
                @"HA?NGA?R",
                "HNGR"
            },
            {
                @"KEY",
                "KEY"
            },
            {
                @"LO?T",
                "LOT"
            },
            {
                @"PIER",
                "PIER"
            },
            {
                @"SLIP",
                "SLIP"
            },
            {
                @"SPA?CE?",
                "SPACE"
            },
            {
                @"STOP",
                "STOP"
            },
            {
                @"TRA?I?LE?R",
                "TRLR"
            },
            {
                @"BOX",
                "BOX"
            }
        };

        /// <summary>
        /// Gets the pattern to match unit names that do not require a number after them.
        /// </summary>
        /// <value>
        /// The unit name pattern for units that do not require a number.
        /// </value>
        public string RangelessSecondaryUnitPattern
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    @"\b(?<{1}>{0})\b",
                    string.Join("|", RangelessUnits.Keys.OrderByDescending(x => x.Length)),
                    Components.SecondaryUnit);
            }
        }

        /// <summary>
        /// Gets a map from unit names that do not require a number after them to their standard forms.
        /// </summary>
        public Dictionary<string, string> RangelessUnits { get; } = new Dictionary<string, string>
        {
            {
                "BA?SE?ME?N?T",
                "BSMT"
            },
            {
                "FRO?NT",
                "FRNT"
            },
            {
                "LO?BBY",
                "LBBY"
            },
            {
                "LOWE?R",
                "LOWR"
            },
            {
                "OFF?I?CE?",
                "OFC"
            },
            {
                "PE?N?T?HO?U?S?E?",
                "PH"
            },
            {
                "REAR",
                "REAR"
            },
            {
                "SIDE",
                "SIDE"
            },
            {
                "UPPE?R",
                "UPPR"
            }
        };

        /// <summary>
        /// Gets the pattern to match states and provinces.
        /// </summary>
        /// <value>
        /// The state pattern.
        /// </value>
        public string StatePattern
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    @"\b(?:{0})\b?",
                    string.Join("|", StatesAndProvinces.Keys.Select(Regex.Escape).Concat(StatesAndProvinces.Values).OrderByDescending(k => k.Length).Distinct()));
            }
        }

        /// <summary>
        /// Gets a map from lowercase US state and territory names to their canonical two-letter
        /// postal abbreviations.
        /// </summary>
        public Dictionary<string, string> StatesAndProvinces { get; } = new Dictionary<string, string>
        {
            { "ALABAMA", "AL" },
            { "ALASKA", "AK" },
            { "AMERICAN SAMOA", "AS" },
            { "ARIZONA", "AZ" },
            { "ARKANSAS", "AR" },
            { "CALIFORNIA", "CA" },
            { "COLORADO", "CO" },
            { "CONNECTICUT", "CT" },
            { "DELAWARE", "DE" },
            {
                "DISTRICT OF COLUMBIA",
                "DC"
            },
            {
                "FEDERATED STATES OF MICRONESIA",
                "FM"
            },
            { "FLORIDA", "FL" },
            { "GEORGIA", "GA" },
            { "GUAM", "GU" },
            { "HAWAII", "HI" },
            { "IDAHO", "ID" },
            { "ILLINOIS", "IL" },
            { "INDIANA", "IN" },
            { "IOWA", "IA" },
            { "KANSAS", "KS" },
            { "KENTUCKY", "KY" },
            { "LOUISIANA", "LA" },
            { "MAINE", "ME" },
            {
                "MARSHALL ISLANDS", "MH"
            },
            { "MARYLAND", "MD" },
            { "MASSACHUSETTS", "MA" },
            { "MICHIGAN", "MI" },
            { "MINNESOTA", "MN" },
            { "MISSISSIPPI", "MS" },
            { "MISSOURI", "MO" },
            { "MONTANA", "MT" },
            { "NEBRASKA", "NE" },
            { "NEVADA", "NV" },
            { "NEW HAMPSHIRE", "NH" },
            { "NEW JERSEY", "NJ" },
            { "NEW MEXICO", "NM" },
            { "NEW YORK", "NY" },
            { "N.Y.", "NY" },
            { "NORTH CAROLINA", "NC" },
            { "NORTH DAKOTA", "ND" },
            {
                "NORTHERN MARIANA ISLANDS",
                "MP"
            },
            { "OHIO", "OH" },
            { "OKLAHOMA", "OK" },
            { "OREGON", "OR" },
            { "PALAU", "PW" },
            { "PENNSYLVANIA", "PA" },
            { "PUERTO RICO", "PR" },
            { "RHODE ISLAND", "RI" },
            { "SOUTH CAROLINA", "SC" },
            { "SOUTH DAKOTA", "SD" },
            { "TENNESSEE", "TN" },
            { "TEXAS", "TX" },
            { "UTAH", "UT" },
            { "VERMONT", "VT" },
            { "VIRGIN ISLANDS", "VI" },
            { "VIRGINIA", "VA" },
            { "WASHINGTON", "WA" },
            { "WEST VIRGINIA", "WV" },
            { "WISCONSIN", "WI" },
            { "WYOMING", "WY" }
        };

        /// <summary>
        /// Gets the pattern to match the street number, name, and suffix.
        /// </summary>
        /// <value>
        /// The street pattern.
        /// </value>
        public string StreetPattern => @"
                        (?:
                          # special case for addresses like 100 COUNTY ROAD F  http://regexstorm.net/tester?p=%28%3f%3a%28%3f%3cSTREET%3eCOUNTY%5cW*ROAD%5cW*%5cw%2b%29%29&i=COUNTY+ROAD+FG%0d%0a%0d%0a&o=ixs
                          (?:(?<STREET>COUNTY\W*ROAD\W*\w+))
                          |
                          # special case for addresses like 3419 Avenue C  http://regexstorm.net/tester?p=%28%3f%3a%28%3f%3cSTREET%3eAVENUE%5cW*%5cw%2b%29%29%0d%0a%23%28%3f%3a%28%3f%3cSTREET%3eAVENUE%5cW*%5ba-zA-Z%5d%7b1%7d%29%29%28%3f%3a%5b%5cs%5d%7c%5b%2c%5d%2b%29%0d%0a&i=3419+Avenue+C+Council+Bluffs+IA+51501%0d%0a5946+AVENUE+E%2c+MCINTOSH%2c+FL+32664%0d%0a&o=ixs
                          (?:(?<STREET>AVENUE\W*\w+))
                          |
                          # special case for addresses like 100 South Street
                          (?:(?<{2}>{0})\W+
                             (?<{3}>{1})\b)
                          |
                          (?:(?<{4}>{0})\W+)?
                          (?:
                            (?<{2}>[^,]*\d)
                            (?:[^\w,]*(?<{5}>{0})\b)
                           |
                            (?<{2}>[^,]+)
                            (?:[^\w,]+(?<{3}>{1})\b)
                            (?:[^\w,]+(?<{5}>{0})\b)?
                           |
                            (?<{2}>[^,]+?)
                            (?:[^\w,]+(?<{3}>{1})\b)?
                            (?:[^\w,]+(?<{5}>{0})\b)?
                          )
                        )
                    ".FormatInvariant(DirectionalPattern, SuffixPattern, Components.Street, Components.Suffix, Components.Predirectional, Components.Postdirectional);

        /// <summary>
        /// Gets a map from the lowercase USPS standard street suffixes to their canonical postal
        /// abbreviations as found in TIGER/Line.
        /// </summary>
        public Dictionary<string, string> StreetSuffixes { get; } = new Dictionary<string, string>
        {
            { "ALLEE", "ALY" },
            { "ALLEY", "ALY" },
            { "ALLY", "ALY" },
            { "ANEX", "ANX" },
            { "ANNEX", "ANX" },
            { "ANNX", "ANX" },
            { "ARCADE", "ARC" },
            { "AV", "AVE" },
            { "AVEN", "AVE" },
            { "AVENU", "AVE" },
            { "AVENUE", "AVE" },
            { "AVN", "AVE" },
            { "AVNUE", "AVE" },
            { "BAYOO", "BYU" },
            { "BAYOU", "BYU" },
            { "BEACH", "BCH" },
            { "BEND", "BND" },
            { "BLUF", "BLF" },
            { "BLUFF", "BLF" },
            { "BLUFFS", "BLFS" },
            { "BOT", "BTM" },
            { "BOTTM", "BTM" },
            { "BOTTOM", "BTM" },
            { "BOUL", "BLVD" },
            { "BOULEVARD", "BLVD" },
            { "BOULV", "BLVD" },
            { "BRANCH", "BR" },
            { "BRDGE", "BRG" },
            { "BRIDGE", "BRG" },
            { "BRNCH", "BR" },
            { "BROOK", "BRK" },
            { "BROOKS", "BRKS" },
            { "BURG", "BG" },
            { "BURGS", "BGS" },
            { "BYPA", "BYP" },
            { "BYPAS", "BYP" },
            { "BYPASS", "BYP" },
            { "BYPS", "BYP" },
            { "CAMP", "CP" },
            { "CANYN", "CYN" },
            { "CANYON", "CYN" },
            { "CAPE", "CPE" },
            { "CAUSEWAY", "CSWY" },
            { "CAUSWAY", "CSWY" },
            { "CEN", "CTR" },
            { "CENT", "CTR" },
            { "CENTER", "CTR" },
            { "CENTERS", "CTRS" },
            { "CENTR", "CTR" },
            { "CENTRE", "CTR" },
            { "CIRC", "CIR" },
            { "CIRCL", "CIR" },
            { "CIRCLE", "CIR" },
            { "CIRCLES", "CIRS" },
            { "CK", "CRK" },
            { "CLIFF", "CLF" },
            { "CLIFFS", "CLFS" },
            { "CLUB", "CLB" },
            { "CMP", "CP" },
            { "CNTER", "CTR" },
            { "CNTR", "CTR" },
            { "CNYN", "CYN" },
            { "COMMON", "CMN" },
            { "CORNER", "COR" },
            { "CORNERS", "CORS" },
            { "COURSE", "CRSE" },
            { "COURT", "CT" },
            { "COURTS", "CTS" },
            { "COVE", "CV" },
            { "COVES", "CVS" },
            { "CR", "CRK" },
            { "CRCL", "CIR" },
            { "CRCLE", "CIR" },
            { "CRECENT", "CRES" },
            { "CREEK", "CRK" },
            { "CRESCENT", "CRES" },
            { "CRESENT", "CRES" },
            { "CREST", "CRST" },
            { "CROSSING", "XING" },
            { "CROSSROAD", "XRD" },
            { "CRSCNT", "CRES" },
            { "CRSENT", "CRES" },
            { "CRSNT", "CRES" },
            { "CRSSING", "XING" },
            { "CRSSNG", "XING" },
            { "CRT", "CT" },
            { "CURVE", "CURV" },
            { "DALE", "DL" },
            { "DAM", "DM" },
            { "DIV", "DV" },
            { "DIVIDE", "DV" },
            { "DRIV", "DR" },
            { "DRIVE", "DR" },
            { "DRIVES", "DRS" },
            { "DRV", "DR" },
            { "DVD", "DV" },
            { "ESTATE", "EST" },
            { "ESTATES", "ESTS" },
            { "EXP", "EXPY" },
            { "EXPR", "EXPY" },
            { "EXPRESS", "EXPY" },
            { "EXPRESSWAY", "EXPY" },
            { "EXPW", "EXPY" },
            { "EXTENSION", "EXT" },
            { "EXTENSIONS", "EXTS" },
            { "EXTN", "EXT" },
            { "EXTNSN", "EXT" },
            { "FALLS", "FLS" },
            { "FERRY", "FRY" },
            { "FIELD", "FLD" },
            { "FIELDS", "FLDS" },
            { "FLAT", "FLT" },
            { "FLATS", "FLTS" },
            { "FORD", "FRD" },
            { "FORDS", "FRDS" },
            { "FOREST", "FRST" },
            { "FORESTS", "FRST" },
            { "FORG", "FRG" },
            { "FORGE", "FRG" },
            { "FORGES", "FRGS" },
            { "FORK", "FRK" },
            { "FORKS", "FRKS" },
            { "FORT", "FT" },
            { "FREEWAY", "FWY" },
            { "FREEWY", "FWY" },
            { "FRRY", "FRY" },
            { "FRT", "FT" },
            { "FRWAY", "FWY" },
            { "FRWY", "FWY" },
            { "GARDEN", "GDN" },
            { "GARDENS", "GDNS" },
            { "GARDN", "GDN" },
            { "GATEWAY", "GTWY" },
            { "GATEWY", "GTWY" },
            { "GATWAY", "GTWY" },
            { "GLEN", "GLN" },
            { "GLENS", "GLNS" },
            { "GRDEN", "GDN" },
            { "GRDN", "GDN" },
            { "GRDNS", "GDNS" },
            { "GREEN", "GRN" },
            { "GREENS", "GRNS" },
            { "GROV", "GRV" },
            { "GROVE", "GRV" },
            { "GROVES", "GRVS" },
            { "GTWAY", "GTWY" },
            { "HARB", "HBR" },
            { "HARBOR", "HBR" },
            { "HARBORS", "HBRS" },
            { "HARBR", "HBR" },
            { "HAVEN", "HVN" },
            { "HAVN", "HVN" },
            { "HEIGHT", "HTS" },
            { "HEIGHTS", "HTS" },
            { "HGTS", "HTS" },
            { "HIGHWAY", "HWY" },
            { "HIGHWY", "HWY" },
            { "HILL", "HL" },
            { "HILLS", "HLS" },
            { "HIWAY", "HWY" },
            { "HIWY", "HWY" },
            { "HLLW", "HOLW" },
            { "HOLLOW", "HOLW" },
            { "HOLLOWS", "HOLW" },
            { "HOLWS", "HOLW" },
            { "HRBOR", "HBR" },
            { "HT", "HTS" },
            { "HWAY", "HWY" },
            { "INLET", "INLT" },
            { "ISLAND", "IS" },
            { "ISLANDS", "ISS" },
            { "ISLES", "ISLE" },
            { "ISLND", "IS" },
            { "ISLNDS", "ISS" },
            { "JCTION", "JCT" },
            { "JCTN", "JCT" },
            { "JCTNS", "JCTS" },
            { "JUNCTION", "JCT" },
            { "JUNCTIONS", "JCTS" },
            { "JUNCTN", "JCT" },
            { "JUNCTON", "JCT" },
            { "KEY", "KY" },
            { "KEYS", "KYS" },
            { "KNOL", "KNL" },
            { "KNOLL", "KNL" },
            { "KNOLLS", "KNLS" },
            { "LA", "LN" },
            { "LAKE", "LK" },
            { "LAKES", "LKS" },
            { "LANDING", "LNDG" },
            { "LANE", "LN" },
            { "LANES", "LN" },
            { "LDGE", "LDG" },
            { "LIGHT", "LGT" },
            { "LIGHTS", "LGTS" },
            { "LNDNG", "LNDG" },
            { "LOAF", "LF" },
            { "LOCK", "LCK" },
            { "LOCKS", "LCKS" },
            { "LODG", "LDG" },
            { "LODGE", "LDG" },
            { "LOOPS", "LOOP" },
            { "MANOR", "MNR" },
            { "MANORS", "MNRS" },
            { "MEADOW", "MDW" },
            { "MEADOWS", "MDWS" },
            { "MEDOWS", "MDWS" },
            { "MILL", "ML" },
            { "MILLS", "MLS" },
            { "MISSION", "MSN" },
            { "MISSN", "MSN" },
            { "MNT", "MT" },
            { "MNTAIN", "MTN" },
            { "MNTN", "MTN" },
            { "MNTNS", "MTNS" },
            { "MOTORWAY", "MTWY" },
            { "MOUNT", "MT" },
            { "MOUNTAIN", "MTN" },
            { "MOUNTAINS", "MTNS" },
            { "MOUNTIN", "MTN" },
            { "MSSN", "MSN" },
            { "MTIN", "MTN" },
            { "NECK", "NCK" },
            { "ORCHARD", "ORCH" },
            { "ORCHRD", "ORCH" },
            { "OVERPASS", "OPAS" },
            { "OVL", "OVAL" },
            { "PARKS", "PARK" },
            { "PARKWAY", "PKWY" },
            { "PARKWAYS", "PKWY" },
            { "PARKWY", "PKWY" },
            { "PASSAGE", "PSGE" },
            { "PATHS", "PATH" },
            { "PIKES", "PIKE" },
            { "PINE", "PNE" },
            { "PINES", "PNES" },
            { "PK", "PARK" },
            { "PKWAY", "PKWY" },
            { "PKWYS", "PKWY" },
            { "PKY", "PKWY" },
            { "PLACE", "PL" },
            { "PLAIN", "PLN" },
            { "PLAINES", "PLNS" },
            { "PLAINS", "PLNS" },
            { "PLAZA", "PLZ" },
            { "PLZA", "PLZ" },
            { "POINT", "PT" },
            { "POINTS", "PTS" },
            { "PORT", "PRT" },
            { "PORTS", "PRTS" },
            { "PRAIRIE", "PR" },
            { "PRARIE", "PR" },
            { "PRK", "PARK" },
            { "PRR", "PR" },
            { "RAD", "RADL" },
            { "RADIAL", "RADL" },
            { "RADIEL", "RADL" },
            { "RANCH", "RNCH" },
            { "RANCHES", "RNCH" },
            { "RAPID", "RPD" },
            { "RAPIDS", "RPDS" },
            { "RDGE", "RDG" },
            { "REST", "RST" },
            { "RIDGE", "RDG" },
            { "RIDGES", "RDGS" },
            { "RIVER", "RIV" },
            { "RIVR", "RIV" },
            { "RNCHS", "RNCH" },
            { "ROAD", "RD" },
            { "ROADS", "RDS" },
            { "ROUTE", "RTE" },
            { "RVR", "RIV" },
            { "SHOAL", "SHL" },
            { "SHOALS", "SHLS" },
            { "SHOAR", "SHR" },
            { "SHOARS", "SHRS" },
            { "SHORE", "SHR" },
            { "SHORES", "SHRS" },
            { "SKYWAY", "SKWY" },
            { "SPNG", "SPG" },
            { "SPNGS", "SPGS" },
            { "SPRING", "SPG" },
            { "SPRINGS", "SPGS" },
            { "SPRNG", "SPG" },
            { "SPRNGS", "SPGS" },
            { "SPURS", "SPUR" },
            { "SQR", "SQ" },
            { "SQRE", "SQ" },
            { "SQRS", "SQS" },
            { "SQU", "SQ" },
            { "SQUARE", "SQ" },
            { "SQUARES", "SQS" },
            { "STATION", "STA" },
            { "STATN", "STA" },
            { "STN", "STA" },
            { "STR", "ST" },
            { "STRAV", "STRA" },
            { "STRAVE", "STRA" },
            { "STRAVEN", "STRA" },
            { "STRAVENUE", "STRA" },
            { "STRAVN", "STRA" },
            { "STREAM", "STRM" },
            { "STREET", "ST" },
            { "STREETS", "STS" },
            { "STREME", "STRM" },
            { "STRT", "ST" },
            { "STRVN", "STRA" },
            { "STRVNUE", "STRA" },
            { "SUMIT", "SMT" },
            { "SUMITT", "SMT" },
            { "SUMMIT", "SMT" },
            { "TERR", "TER" },
            { "TERRACE", "TER" },
            { "THROUGHWAY", "TRWY" },
            { "TPK", "TPKE" },
            { "TR", "TRL" },
            { "TRACE", "TRCE" },
            { "TRACES", "TRCE" },
            { "TRACK", "TRAK" },
            { "TRACKS", "TRAK" },
            { "TRAFFICWAY", "TRFY" },
            { "TRAIL", "TRL" },
            { "TRAILS", "TRL" },
            { "TRK", "TRAK" },
            { "TRKS", "TRAK" },
            { "TRLS", "TRL" },
            { "TRNPK", "TPKE" },
            { "TRPK", "TPKE" },
            { "TUNEL", "TUNL" },
            { "TUNLS", "TUNL" },
            { "TUNNEL", "TUNL" },
            { "TUNNELS", "TUNL" },
            { "TUNNL", "TUNL" },
            { "TURNPIKE", "TPKE" },
            { "TURNPK", "TPKE" },
            { "UNDERPASS", "UPAS" },
            { "UNION", "UN" },
            { "UNIONS", "UNS" },
            { "VALLEY", "VLY" },
            { "VALLEYS", "VLYS" },
            { "VALLY", "VLY" },
            { "VDCT", "VIA" },
            { "VIADCT", "VIA" },
            { "VIADUCT", "VIA" },
            { "VIEW", "VW" },
            { "VIEWS", "VWS" },
            { "VILL", "VLG" },
            { "VILLAG", "VLG" },
            { "VILLAGE", "VLG" },
            { "VILLAGES", "VLGS" },
            { "VILLE", "VL" },
            { "VILLG", "VLG" },
            { "VILLIAGE", "VLG" },
            { "VIST", "VIS" },
            { "VISTA", "VIS" },
            { "VLLY", "VLY" },
            { "VST", "VIS" },
            { "VSTA", "VIS" },
            { "WALKS", "WALK" },
            { "WELL", "WL" },
            { "WELLS", "WLS" },
            { "WY", "WAY" }
        };

        /// <summary>
        /// Gets the pattern to match standard street suffixes
        /// </summary>
        /// <value>
        /// The suffix pattern.
        /// </value>
        public string SuffixPattern
        {
            get
            {
                return string.Join("|", StreetSuffixes.Values.Concat(StreetSuffixes.Keys).OrderByDescending(k => k.Length).Distinct());
            }
        }

        /// <summary>
        /// Attempts to parse the given input as a US address.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>
        /// The parsed address, or null if the address could not be parsed.
        /// </returns>
        public AddressParseResult ParseAddress(string input)
        {
            return ParseAddress(input, true);
        }

        /// <summary>
        /// Attempts to parse the given input as a US address.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="normalize">if set to <c>true</c> then normalize extracted fields.</param>
        /// <returns>
        /// The parsed address, or null if the address could not be parsed.
        /// </returns>
        public AddressParseResult ParseAddress(string input, bool normalize)
        {
            return ParseAddress(input, AddressRegex, normalize);
        }

        /// <summary>
        /// Attempts to parse the given input as a US address.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="pattern">The regex pattern to use for matching address fields</param>
        /// <param name="normalize">if set to <c>true</c> then normalize extracted fields.</param>
        /// <returns>
        /// The parsed address, or null if the address could not be parsed.
        /// </returns>
        public AddressParseResult ParseAddress(string input, Regex pattern, bool normalize)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (normalize)
            {
                input = input.ToUpperInvariant();
            }

            var match = pattern.Match(input);
            if (!match.Success)
            {
                return null;
            }

            var extracted = GetApplicableFields(match, pattern);
            if (normalize)
            {
                extracted = Normalize(extracted);
            }

            return new AddressParseResult(extracted, match);
        }

        /// <summary>
        /// Attempts to parse the given input as the street line of a US address.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="normalize">if set to <c>true</c> then normalize extracted fields.</param>
        /// <returns>
        /// The parsed address fields, or null if the address could not be parsed.
        /// </returns>
        public AddressParseResult ParseAddressLine(string input, bool normalize)
        {
            return ParseAddress(input, AddressLineRegex, normalize);
        }

        /// <summary>
        /// Given a dictionary that maps regular expressions to USPS abbreviations,
        /// this function finds the first entry whose regular expression matches the given
        /// input value and supplies the corresponding USPS abbreviation as its output. If
        /// no match is found, the original value is returned.
        /// </summary>
        /// <param name="map">The dictionary that maps regular expressions to USPS abbreviations.</param>
        /// <param name="input">The value to test against the regular expressions.</param>
        /// <returns>The correct USPS abbreviation, or the original value if no regular expression
        /// matched successfully.</returns>
        private static string GetNormalizedValueByRegexLookup(Dictionary<string, string> map, string input)
        {
            var output = input;

            foreach (var pair in map)
            {
                var pattern = pair.Key;
                if (Regex.IsMatch(input, pattern))
                {
                    output = pair.Value;
                    break;
                }
            }

            return output;
        }

        /// <summary>
        /// Given a dictionary that maps strings to USPS abbreviations,
        /// this function finds the first entry whose key matches the given
        /// input value and supplies the corresponding USPS abbreviation as its output. If
        /// no match is found, the original value is returned.
        /// </summary>
        /// <param name="map">The dictionary that maps strings to USPS abbreviations.</param>
        /// <param name="input">The value to search for in the list of strings.</param>
        /// <returns>The correct USPS abbreviation, or the original value if no string
        /// matched successfully.</returns>
        private static string GetNormalizedValueByStaticLookup(IDictionary<string, string> map, string input)
        {
            if (!map.TryGetValue(input, out string output))
            {
                output = input;
            }

            return output;
        }

        /// <summary>
        /// Build a combined dictionary of both the ranged and singular secondary units.
        /// </summary>
        /// <returns>A map of ranged and singular secondary unit names to their standard forms.</returns>
        /// <remarks>This is used by the Normalize() method to convert the unit into the USPS
        /// standardized form.</remarks>
        private Dictionary<string, string> CombineSecondaryUnits()
        {
            return new[] { RangedUnits, RangelessUnits }.SelectMany(x => x)
                .ToDictionary(y => y.Key, y => y.Value);
        }

        /// <summary>
        /// Given a successful <see cref="Match"/>, this method creates a dictionary
        /// consisting of the fields that we actually care to extract from the address.
        /// </summary>
        /// <param name="match">The successful <see cref="Match"/> instance.</param>
        /// <param name="regex">The parsing <see cref="Regex"/>.</param>
        /// <returns>A dictionary in which the keys are the name of the fields and the values
        /// are pulled from the input address.</returns>
        private Dictionary<string, string> GetApplicableFields(Match match, Regex regex)
        {
            var applicable = new Dictionary<string, string>();

            foreach (var field in regex.GetGroupNames())
            {
                if (!_fields.Contains(field))
                {
                    continue;
                }

                var mg = match.Groups[field];
                if (mg != null && mg.Success)
                {
                    applicable[field] = mg.Value;
                }
            }

            return applicable;
        }

        /// <summary>
        /// Given a field type and an input value, this method returns the proper USPS
        /// abbreviation for it (or the original value if no substitution can be found or is
        /// necessary).
        /// </summary>
        /// <param name="field">The type of the field.</param>
        /// <param name="input">The value of the field.</param>
        /// <returns>The normalized value.</returns>
        private string GetNormalizedValueForField(string field, string input)
        {
            if (input == null)
            {
                return null;
            }

            var output = input;

            switch (field)
            {
                case Components.Predirectional:
                case Components.Postdirectional:
                    output = GetNormalizedValueByStaticLookup(DirectionalNames, input);
                    break;

                case Components.Suffix:
                    output = GetNormalizedValueByStaticLookup(StreetSuffixes, input);
                    break;

                case Components.SecondaryUnit:
                    output = GetNormalizedValueByRegexLookup(AllUnits, input);
                    break;

                case Components.State:
                    output = GetNormalizedValueByStaticLookup(StatesAndProvinces, input);
                    break;

                case Components.Number:
                    if (!input.Contains('/'))
                    {
                        output = input.Replace(" ", string.Empty);
                    }

                    break;
            }

            return output;
        }

        /// <summary>
        /// Builds the regular expression stored in <see cref="AddressLineRegex"/>
        /// that does the parsing for address line fields only.
        /// </summary>
        /// <returns>The parser <see cref="Regex"/>.</returns>
        private Regex InitializeAddressLineRegex()
        {
            var numberPattern = @"(
                    ((?<{0}>\d+)(?<{1}>(-[0-9])|(\-?[A-Z]))(?=\b))    # Unit-attached
                    |(?<{0}>\d+[\-\ ]?\d+\/\d+)                                   # Fractional
                    |(?<{0}>\d+-?\d*)                                             # Normal Number
                    |(?<{0}>[NSWE]\ ?\d+\ ?[NSWE]\ ?\d+)                          # Wisconsin/Illinois
                  )".FormatInvariant(Components.Number, Components.SecondaryNumber);

            var generalPattern = @"(
                        [^\w\#]*    # skip non-word chars except # (e.g. unit)
                        (  {0} )\W*
                           {1}\W*
                        (?:{2}\W+)?
                    )".FormatInvariant(numberPattern, StreetPattern, AllSecondaryUnitPattern);

            var addressPattern = @"
                    ^
                    {0}
                    |
                    {1}
                ".FormatInvariant(PostalBoxPatternAddressLineOnly, generalPattern);

            return new Regex(addressPattern, MatchOptions);
        }

        /// <summary>
        /// Builds the gigantic regular expression stored in the addressRegex static
        /// member that actually does the parsing.
        /// </summary>
        /// <returns>The parser <see cref="Regex"/>.</returns>
        private Regex InitializeRegex()
        {
            var numberPattern = @"(
                    ((?<{0}>\d+)(?<{1}>(-[0-9])|(\-?[A-Z]))(?=\b))    # Unit-attached
                    |(?<{0}>\d+[\-\ ]?\d+\/\d+)                                   # Fractional
                    |(?<{0}>\d+-?\d*)                                             # Normal Number
                    |(?<{0}>[NSWE]\ ?\d+\ ?[NSWE]\ ?\d+)                          # Wisconsin/Illinois
                  )".FormatInvariant(Components.Number, Components.SecondaryNumber);

            var armedForcesPattern = @"# Special case for APO/FPO/DPO addresses
                    (
                        [^\w\#]*
                        (?<{1}>.+?)
                        (?<{2}>[AFD]PO)\W+
                        (?<{3}>A[AEP])\W+
                        (?<{4}>{0})
                        \W*
                    )".FormatInvariant(ZipPattern, Components.StreetLine, Components.City, Components.State, Components.Zip);

            var generalPattern = @"(
                        [^\w\#]*    # skip non-word chars except # (e.g. unit)
                        (  {0} )\W*
                           {1}\W+
                        (?:{2}\W+)?
                           {3}
                        \W*         # require on non-word chars at end
                    )".FormatInvariant(numberPattern, StreetPattern, AllSecondaryUnitPattern, PlacePattern);

            var addressPattern = @"
                    ^
                    {0}
                    |
                    {1}
                    |
                    {2}
                    $           # right up to end of string
                ".FormatInvariant(armedForcesPattern, PostalBoxPattern, generalPattern);

            return new Regex(addressPattern, MatchOptions);
        }

        /// <summary>
        /// Given a set of fields pulled from a successful match, this normalizes each value
        /// by stripping off some punctuation and, if applicable, converting it to a standard
        /// USPS abbreviation.
        /// </summary>
        /// <param name="extracted">The dictionary of extracted fields.</param>
        /// <returns>A dictionary of the extracted fields with normalized values.</returns>
        private Dictionary<string, string> Normalize(IDictionary<string, string> extracted)
        {
            var normalized = new Dictionary<string, string>();

            foreach (var pair in extracted)
            {
                var key = pair.Key;
                var value = pair.Value;
                if (value == null)
                {
                    continue;
                }

                // Strip off some punctuation
                value = Regex.Replace(value, @"^\s+|\s+$|[^\/\w\s\-\#\&]", string.Empty);

                // Normalize to official abbreviations where appropriate
                value = GetNormalizedValueForField(key, value);

                normalized[key] = value;
            }

            // Special case for an attached unit
            if (extracted.ContainsKey(Components.SecondaryNumber)
                && (!extracted.ContainsKey(Components.SecondaryUnit) || string.IsNullOrWhiteSpace(extracted[Components.SecondaryUnit])))
            {
                normalized[Components.SecondaryUnit] = "APT";
            }

            return normalized;
        }

   
    }
}