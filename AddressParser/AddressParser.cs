namespace AddressParser
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

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
        #region Directionals

        /// <summary>
        /// Maps directional names (north, northeast, etc.) to abbreviations (N, NE, etc.).
        /// </summary>
        private static Dictionary<string, string> directionals =
            new Dictionary<string, string>()
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

        #endregion

        #region States

        /// <summary>
        /// Maps lowercased US state and territory names to their canonical two-letter
        /// postal abbreviations.
        /// </summary>
        private static Dictionary<string, string> states =
            new Dictionary<string, string>()
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
                { "DISTRICT OF COLUMBIA", "DC" },
                { "FEDERATED STATES OF MICRONESIA", "FM" },
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
                { "MARSHALL ISLANDS", "MH" },
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
                { "NORTH CAROLINA", "NC" },
                { "NORTH DAKOTA", "ND" },
                { "NORTHERN MARIANA ISLANDS", "MP" },
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

        #endregion

        #region Suffixes

        /// <summary>
        /// Maps lowerecased USPS standard street suffixes to their canonical postal
        /// abbreviations as found in TIGER/Line.
        /// </summary>
        private static Dictionary<string, string> suffixes = 
            new Dictionary<string, string>()
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

        #endregion

        #region Secondary Unit Designators - Ranged

        /// <summary>
        /// Secondary units that require a number after them.
        /// </summary>
        private static Dictionary<string, string> rangedSecondaryUnits =
            new Dictionary<string, string>()
            {
                { @"SU?I?TE", "STE" },
                { @"(?:AP)(?:AR)?T(?:ME?NT)?", "APT" },
                { @"(?:DEP)(?:AR)?T(?:ME?NT)?", "DEPT" },
                { @"RO*M", "RM" },
                { @"FLO*R?", "FL" },
                { @"UNI?T", "UNIT" },
                { @"BU?I?LDI?N?G", "BLDG" },
                { @"HA?NGA?R", "HNGR" },
                { @"KEY", "KEY" },
                { @"LO?T", "LOT" },
                { @"PIER", "PIER" },
                { @"SLIP", "SLIP" },
                { @"SPA?CE?", "SPACE" },
                { @"STOP", "STOP" },
                { @"TRA?I?LE?R", "TRLR" },
                { @"BOX", "BOX" }
            };

        #endregion

        #region Secondary Unit Designators - Rangeless

        /// <summary>
        /// Secondary units that do not require a number after them.
        /// </summary>
        private static Dictionary<string, string> rangelessSecondaryUnits =
            new Dictionary<string, string>()
            {
                { "BA?SE?ME?N?T", "BSMT" },
                { "FRO?NT", "FRNT" },
                { "LO?BBY", "LBBY" },
                { "LOWE?R", "LOWR" },
                { "OFF?I?CE?", "OFC" },
                { "PE?N?T?HO?U?S?E?", "PH" },
                { "REAR", "REAR" },
                { "SIDE", "SIDE" },
                { "UPPE?R", "UPPR" }
            };

        #endregion

        /// <summary>
        /// A combined dictionary of the ranged and rangeless secondary units.
        /// </summary>
        private static Dictionary<string, string> allSecondaryUnits;

        /// <summary>
        /// The gigantic regular expression that actually extracts the bits and pieces
        /// from a given address.
        /// </summary>
        private static Regex addressRegex;

        /// <summary>
        /// In the <see cref="M:addressRegex"/> member, these are the names
        /// of the groups in the result that we care to inspect.
        /// </summary>
        private static string[] fields = 
            new[]
            {
                "NUMBER",
                "PREDIRECTIONAL",
                "STREET",
                "STREETLINE",
                "SUFFIX",
                "POSTDIRECTIONAL",
                "CITY",
                "STATE",
                "ZIP",
                "SECONDARYUNIT",
                "SECONDARYNUMBER"
            };

        /// <summary>
        /// Initializes the <see cref="AddressParser"/> class.
        /// </summary>
        static AddressParser()
        {
            // Build a combined dictionary of both the ranged and rangeless secondary units.
            // This is used by the Normalize() method to convert the unit into the USPS
            // standardized form.
            allSecondaryUnits = new[] { rangedSecondaryUnits, rangelessSecondaryUnits }
                .SelectMany(x => x)
                .ToDictionary(y => y.Key, y => y.Value);

            // Build the giant regex
            InitializeRegex();
        }

        /// <summary>
        /// Attempts to parse the given input as a US address.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The parsed address, or null if the address could not be parsed.</returns>
        public AddressParseResult ParseAddress(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                var match = addressRegex.Match(input.ToUpperInvariant());
                if (match.Success)
                {
                    var extracted = GetApplicableFields(match);
                    return new AddressParseResult(Normalize(extracted));
                }
            }

            return null;
        }

        /// <summary>
        /// Given a successful <see cref="Match"/>, this method creates a dictionary 
        /// consisting of the fields that we actually care to extract from the address.
        /// </summary>
        /// <param name="match">The successful <see cref="Match"/> instance.</param>
        /// <returns>A dictionary in which the keys are the name of the fields and the values
        /// are pulled from the input address.</returns>
        private static Dictionary<string, string> GetApplicableFields(Match match)
        {
            var applicable = new Dictionary<string, string>();

            foreach (var field in addressRegex.GetGroupNames())
            {
                if (fields.Contains(field))
                {
                    if (match.Groups[field].Success)
                    {
                        applicable[field] = match.Groups[field].Value;
                    }
                }
            }

            return applicable;
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
        private static string GetNormalizedValueByRegexLookup(
            Dictionary<string, string> map,
            string input)
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
        private static string GetNormalizedValueByStaticLookup(
            Dictionary<string, string> map,
            string input)
        {
            string output;

            if (!map.TryGetValue(input, out output))
            {
                output = input;
            }

            return output;
        }

        /// <summary>
        /// Given a field type and an input value, this method returns the proper USPS
        /// abbreviation for it (or the original value if no substitution can be found or is
        /// necessary).
        /// </summary>
        /// <param name="field">The type of the field.</param>
        /// <param name="input">The value of the field.</param>
        /// <returns>The normalized value.</returns>
        private static string GetNormalizedValueForField(
            string field,
            string input)
        {
            var output = input;

            switch (field)
            {
                case "PREDIRECTIONAL":
                case "POSTDIRECTIONAL":
                    output = GetNormalizedValueByStaticLookup(directionals, input);
                    break;
                case "SUFFIX":
                    output = GetNormalizedValueByStaticLookup(suffixes, input);
                    break;
                case "SECONDARYUNIT":
                    output = GetNormalizedValueByRegexLookup(allSecondaryUnits, input);
                    break;
                case "STATE":
                    output = GetNormalizedValueByStaticLookup(states, input);
                    break;
                case "NUMBER":
                    if (!input.Contains('/'))
                    {
                        output = input.Replace(" ", string.Empty);
                    }

                    break;
                default:
                    break;
            }

            return output;
        }

        /// <summary>
        /// Builds the gigantic regular expression stored in the addressRegex static
        /// member that actually does the parsing.
        /// </summary>
        private static void InitializeRegex()
        {
            var suffixPattern = new Regex(
                string.Join(
                    "|",
                    new [] {
                        string.Join("|", suffixes.Keys), 
                        string.Join("|", suffixes.Values.Distinct())
                    }),
                RegexOptions.Compiled);

            var statePattern = 
                @"\b(?:" + 
                string.Join(
                    "|",
                    new [] {
                        string.Join("|", states.Keys.Select(x => Regex.Escape(x))),
                        string.Join("|", states.Values)
                    }) +
                @")\b";

            var directionalPattern =
                string.Join(
                    "|",
                    new [] {
                        string.Join("|", directionals.Keys),
                        string.Join("|", directionals.Values),
                        string.Join("|", directionals.Values.Select(x => Regex.Replace(x, @"(\w)", @"$1\.")))
                    });

            var zipPattern = @"\d{5}(?:-?\d{4})?";

            var numberPattern =
                @"(
                    ((?<NUMBER>\d+)(?<SECONDARYNUMBER>(-[0-9])|(\-?[A-Z]))(?=\b))    # Unit-attached
                    |(?<NUMBER>\d+[\-\ ]?\d+\/\d+)                                   # Fractional
                    |(?<NUMBER>\d+-?\d*)                                             # Normal Number
                    |(?<NUMBER>[NSWE]\ ?\d+\ ?[NSWE]\ ?\d+)                          # Wisconsin/Illinois
                  )";

            var streetPattern =
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"
                        (?:
                          # special case for addresses like 100 South Street
                          (?:(?<STREET>{0})\W+
                             (?<SUFFIX>{1})\b)
                          |
                          (?:(?<PREDIRECTIONAL>{0})\W+)?
                          (?:
                            (?<STREET>[^,]*\d)
                            (?:[^\w,]*(?<POSTDIRECTIONAL>{0})\b)
                           |
                            (?<STREET>[^,]+)
                            (?:[^\w,]+(?<SUFFIX>{1})\b)
                            (?:[^\w,]+(?<POSTDIRECTIONAL>{0})\b)?
                           |
                            (?<STREET>[^,]+?)
                            (?:[^\w,]+(?<SUFFIX>{1})\b)?
                            (?:[^\w,]+(?<POSTDIRECTIONAL>{0})\b)?
                          )
                        )
                    ",
                    directionalPattern,
                    suffixPattern);

            var rangedSecondaryUnitPattern =
                @"(?<SECONDARYUNIT>" +
                string.Join("|", rangedSecondaryUnits.Keys) +
                @")(?![a-z])";
            var rangelessSecondaryUnitPattern =
                @"(?<SECONDARYUNIT>" +
                string.Join(
                    "|",
                    string.Join("|", rangelessSecondaryUnits.Keys)) +
                @")\b";
            var allSecondaryUnitPattern = string.Format(
                CultureInfo.InvariantCulture,
                @"
                    (
                        (:?
                            (?: (?:{0} \W*)
                                | (?<SECONDARYUNIT>\#)\W*
                            )
                            (?<SECONDARYNUMBER>[\w-]+)
                        )
                        |{1}
                    ),?
                ",
                 rangedSecondaryUnitPattern,
                 rangelessSecondaryUnitPattern);

            var cityAndStatePattern = string.Format(
                CultureInfo.InvariantCulture,
                @"
                    (?:
                        (?<CITY>[^\d,]+?)\W+
                        (?<STATE>{0})
                    )
                ",
                statePattern);
            var placePattern = string.Format(
                CultureInfo.InvariantCulture,
                @"
                    (?:{0}\W*)?
                    (?:(?<ZIP>{1}))?
                ",
                cityAndStatePattern,
                zipPattern);

            var addressPattern = string.Format(
                CultureInfo.InvariantCulture,
                @"
                    ^
                    # Special case for APO/FPO/DPO addresses
                    (
                        [^\w\#]*
                        (?<STREETLINE>.+?)
                        (?<CITY>[AFD]PO)\W+
                        (?<STATE>A[AEP])\W+
                        (?<ZIP>{4})
                        \W*
                    )
                    |
                    # Special case for PO boxes
                    (
                        \W*
                        (?<STREETLINE>(P[\.\ ]?O[\.\ ]?\ )?BOX\ [0-9]+)\W+
                        {3}
                        \W*
                    )
                    |
                    (
                        [^\w\#]*    # skip non-word chars except # (eg unit)
                        (  {0} )\W*
                           {1}\W+
                        (?:{2}\W+)?
                           {3}
                        \W*         # require on non-word chars at end
                    )
                    $           # right up to end of string
                ",
                numberPattern,
                streetPattern,
                allSecondaryUnitPattern,
                placePattern,
                zipPattern);
            addressRegex = new Regex(
                addressPattern,
                RegexOptions.Compiled | 
                RegexOptions.Singleline | 
                RegexOptions.IgnorePatternWhitespace);
        }

        /// <summary>
        /// Given a set of fields pulled from a successful match, this normalizes each value
        /// by stripping off some punctuation and, if applicable, converting it to a standard
        /// USPS abbreviation.
        /// </summary>
        /// <param name="extracted">The dictionary of extracted fields.</param>
        /// <returns>A dictionary of the extracted fields with normalized values.</returns>
        private static Dictionary<string, string> Normalize(Dictionary<string, string> extracted)
        {
            var normalized = new Dictionary<string, string>();

            foreach (var pair in extracted)
            {
                var key = pair.Key;
                var value = pair.Value;

                // Strip off some punctuation
                value = Regex.Replace(
                    value,
                    @"^\s+|\s+$|[^\/\w\s\-\#\&]",
                    string.Empty);

                // Normalize to official abbreviations where appropriate
                value = GetNormalizedValueForField(key, value);

                normalized[key] = value;
            }

            // Special case for an attached unit
            if (extracted.ContainsKey("SECONDARYNUMBER") &&
                (!extracted.ContainsKey("SECONDARYUNIT") ||
                 string.IsNullOrWhiteSpace(extracted["SECONDARYUNIT"])))
            {
                normalized["SECONDARYUNIT"] = "APT";
            }

            return normalized;
        }
    }
}
