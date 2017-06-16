using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;

using ApprovalTests;
using ApprovalTests.Combinations;

using ApprovalUtilities.Utilities;

using Xunit;

namespace USAddress.Tests
{
    public class AddressParserTest
    {
        private static readonly AddressParser Parser = AddressParser.Default;

        [Fact]
        public void StreetLineTests()
        {
            var addressParser = new AddressParser();
            var streetPattern = new Regex(addressParser.StreetPattern, addressParser.MatchOptions);
            var streetPatternMatch = streetPattern.Match("COUNTY ROAD F");
            Assert.NotNull(streetPatternMatch);
            Assert.True(streetPatternMatch.Success);
            Assert.True(streetPatternMatch.Captures.Count == 1);
            Assert.True(streetPatternMatch.Captures[0].Value == "COUNTY ROAD F");
        }

        [Fact]
        public void ParseAddressLine()
        {
            var addressParser = new AddressParser();
            var result = addressParser.ParseAddressLine("3360 County Road F", true);
            Approvals.Verify(result);
        }

        [Fact]
        public void StateDoesNotMatchCity()
        {
            var reg = new Regex(Parser.PlacePattern, Parser.MatchOptions);
            var match = reg.Match("URB LA FABRICA,PR,00704");
            var state = match.Groups[Components.State].Value;
            Approvals.Verify(state);
        }

        [Fact]
        public void AddressRegexIsLazy()
        {
            var addressParser = new AddressParser();
            addressParser.StreetSuffixes.Add("EDGE", "EDGE");
            Approvals.Verify(addressParser.AddressRegex);
        }

        /// <summary>
        /// People in Wisconsin and Illinois are eating too much cheese, apparently, because
        /// you can encounter house numbers with letters in them. It's similar to the
        /// Utah grid-system, except the gridness is all crammed into the house number.
        /// </summary>
        [Fact]
        public void CanParseAddressWithAlphanumericRange()
        {
            Approvals.Verify(Parser.ParseAddress("N6W23001 BLUEMOUND ROAD, ROLLING MEADOWS, IL, 12345"));
        }

        /// <summary>
        /// In Virginia Beach, for example, there's a South Blvd, which could really
        /// throw a spanner into our predirectional/postdirectional parsing. We call
        /// this case out specifically in our regex.
        /// </summary>
        [Fact]
        public void CanParseAddressWithCardinalStreetName()
        {
            Approvals.Verify(Parser.ParseAddress("500 SOUTH STREET VIRGINIA BEACH VIRGINIA 23452"));
        }

        [Fact]
        public void CanParseAddressWithoutPunctuation()
        {
            Approvals.Verify(Parser.ParseAddress("999 West 89th Street Apt A New York NY 10024"));
        }

        /// <summary>
        /// When people live in apartments with letters, they sometimes attach the apartment
        /// letter to the end of the house number. This is wrong, and these people need to be
        /// lined up and individually slapped. We pull out the unit and designate it as "APT",
        /// which in my experience is the designator that USPS uses in the vast, vast majority
        /// of cases.
        /// </summary>
        [Fact]
        public void CanParseAddressWithRangedUnitAttachedToNumber()
        {
            Approvals.Verify(Parser.ParseAddress("403D BERRYFIELD LANE CHESAPEAKE VA 23224"));
        }

        [Fact]
        public void CanParseAddressWithRangelessSecondaryUnit()
        {
            Approvals.Verify(Parser.ParseAddress("1050 Broadway Penthouse, New York, NY 10001"));
        }

        /// <summary>
        /// Speaking of weird addresses, sometimes people put a space in the number.
        /// USPS says we should squash it together.
        /// </summary>
        [Fact]
        public void CanParseAddressWithSpacedAlphanumericRange()
        {
            Approvals.Verify(Parser.ParseAddress("N645 W23001 BLUEMOUND ROAD, ROLLING MEADOWS, IL, 12345"));
        }

        /// <summary>
        /// At least it's not platform 9 3/4.
        /// </summary>
        [Fact]
        public void CanParseFractionalAddress()
        {
            Approvals.Verify(Parser.ParseAddress("123 1/2 MAIN ST, RICHMOND, VA 23221"));
        }

        /// <summary>
        /// Grid-style addresses are common in parts of Utah. The official USPS address database
        /// in this case treats "E" as a pre-directional, "1700" as the street name, and "S" as a
        /// post-directional, and nothing as the suffix, so that's how we parse it, too.
        /// </summary>
        [Fact]
        public void CanParseGridStyleAddress()
        {
            Approvals.Verify(Parser.ParseAddress("842 E 1700 S, Salt Lake City, UT 84105"));
        }

        /// <summary>
        /// Military addresses seem to follow no convention whatsoever in the
        /// street line, but the APO/FPO/DPO AA/AE/AP 9NNNN part of the place line
        /// is pretty well standardized. I've made a special exception for these
        /// kinds of addresses so that the street line is just dumped as-is into
        /// the StreetLine field.
        /// </summary>
        [Fact]
        public void CanParseMilitaryAddress()
        {
            Approvals.Verify(Parser.ParseAddress("PSC BOX 453, APO AE 99969"));
        }

        [Fact]
        public void CanParsePostOfficeBoxAddress()
        {
            Approvals.Verify(Parser.ParseAddress("P.O. BOX 4857, New York, NY 10001"));
        }

        /// <summary>
        /// In parts of New York City, some people feel REALLY STRONGLY about
        /// the hyphen in their house number. The numbering system makes sense,
        /// but the USPS address database doesn't support hyphens in the number field.
        /// To the USPS, the hyphen does not exist, but the DMM specifically does say
        /// that "if present, the hyphen should not be removed."
        /// </summary>
        [Fact]
        public void CanParseQueensStyleAddress()
        {
            Approvals.Verify(Parser.ParseAddress("123-465 34th St New York NY 12345"));
        }

        [Fact]
        public void CanParseTypicalAddressWithoutPunctuationAfterStreetLine()
        {
            Approvals.Verify(Parser.ParseAddress("1005 N Gravenstein Highway Sebastopol, CA 95472"));
        }

        [Fact]
        public void CanParseTypicalAddressWithPunctuation()
        {
            Approvals.Verify(Parser.ParseAddress("1005 N Gravenstein Highway, Sebastopol, CA 95472"));
        }

        [Fact]
        public void MatchSecondaryUnitAsWord()
        {
            var streets = new[] { "9999 RIVERSIDE DR WEST #8B", "9999 RIVERSIDE DR WEST SIDE LEFT", };
            var results = streets.Select(s => new { Text = s, Match = Regex.Match(s, AddressParser.Default.RangelessSecondaryUnitPattern, AddressParser.Default.MatchOptions) });
            Approvals.VerifyAll(results, "Match");
        }

        [Fact]
        public void ParseExampleAddresses()
        {
            var text = File.ReadAllText(PathUtilities.GetAdjacentFile("samples.txt"));
            var examples = text.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => !s.StartsWith("#", StringComparison.OrdinalIgnoreCase));
            bool[] normalize = { true, false };

            CombinationApprovals.VerifyAllCombinations(
                Parser.ParseAddress,
                x => Environment.NewLine + ((AddressParseResult)x).WritePropertiesToString(),
                examples,
                normalize);
        }

        [Fact]
        public void ParseWithoutNormalization()
        {
            Approvals.Verify(Parser.ParseAddress("999 West 89th Street Apt A New York NY 10024", false));
        }

        [Fact]
        public void VerifyAllSecondaryUnitPattern()
        {
            Approvals.Verify(AddressParser.Default.AllSecondaryUnitPattern);
        }

        [Fact]
        public void VerifyAllUnits()
        {
            Approvals.VerifyAll(AddressParser.Default.AllUnits);
        }

        [Fact]
        public void VerifyCityAndStatePattern()
        {
            Approvals.Verify(AddressParser.Default.CityAndStatePattern);
        }

        [Fact]
        public void VerifyDirectionalNames()
        {
            Approvals.VerifyAll(AddressParser.Default.DirectionalNames);
        }

        [Fact]
        public void VerifyDirectionalPattern()
        {
            Approvals.Verify(AddressParser.Default.DirectionalPattern);
        }

        [Fact]
        public void VerifyMatchOptions()
        {
            Approvals.Verify(AddressParser.Default.MatchOptions);
        }

        [Fact]
        public void VerifyPlacePattern()
        {
            Approvals.Verify(AddressParser.Default.PlacePattern);
        }

        [Fact]
        public void VerifyPostalBoxPattern()
        {
            Approvals.Verify(AddressParser.Default.PostalBoxPattern);
        }

        [Fact]
        public void VerifyRangedSecondaryUnitPattern()
        {
            Approvals.Verify(AddressParser.Default.RangedSecondaryUnitPattern);
        }

        [Fact]
        public void VerifyRangedUnits()
        {
            Approvals.VerifyAll(AddressParser.Default.RangedUnits);
        }

        [Fact]
        public void VerifyRangelessSecondaryUnitPattern()
        {
            Approvals.Verify(AddressParser.Default.RangelessSecondaryUnitPattern);
        }

        [Fact]
        public void VerifyRangelessUnits()
        {
            Approvals.VerifyAll(AddressParser.Default.RangelessUnits);
        }

        [Fact]
        public void VerifyStatePattern()
        {
            Approvals.Verify(AddressParser.Default.StatePattern);
        }

        [Fact]
        public void VerifyStates()
        {
            Approvals.VerifyAll(AddressParser.Default.StatesAndProvinces);
        }

        [Fact]
        public void VerifyStreetPattern()
        {
            Approvals.Verify(AddressParser.Default.StreetPattern);
        }

        [Fact]
        public void VerifySuffixes()
        {
            Approvals.VerifyAll(AddressParser.Default.StreetSuffixes);
        }

        [Fact]
        public void VerifySuffixPattern()
        {
            Approvals.Verify(AddressParser.Default.SuffixPattern);
        }

        [Fact]
        public void VerifyZipPattern()
        {
            Approvals.Verify(AddressParser.ZipPattern);
        }
    }
}