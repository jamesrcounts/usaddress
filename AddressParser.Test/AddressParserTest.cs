namespace AddressParser.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;

    [TestFixture]
    public class AddressParserTest
    {
        [Test]
        public void CanParseTypicalAddressWithoutPunctuationAfterStreetLine()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("1005 N Gravenstein Highway Sebastopol, CA 95472");

            Assert.AreEqual("SEBASTOPOL", address.City);
            Assert.AreEqual("1005", address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual("N", address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual(null, address.SecondaryUnit);
            Assert.AreEqual("CA", address.State);
            Assert.AreEqual("GRAVENSTEIN", address.Street);
            Assert.AreEqual("1005 N GRAVENSTEIN HWY", address.StreetLine);
            Assert.AreEqual("HWY", address.Suffix);
            Assert.AreEqual("95472", address.Zip);
        }

        [Test]
        public void CanParseTypicalAddressWithPunctuation()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("1005 N Gravenstein Highway, Sebastopol, CA 95472");

            Assert.AreEqual("SEBASTOPOL", address.City);
            Assert.AreEqual("1005", address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual("N", address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual(null, address.SecondaryUnit);
            Assert.AreEqual("CA", address.State);
            Assert.AreEqual("GRAVENSTEIN", address.Street);
            Assert.AreEqual("1005 N GRAVENSTEIN HWY", address.StreetLine);
            Assert.AreEqual("HWY", address.Suffix);
            Assert.AreEqual("95472", address.Zip);
        }

        [Test]
        public void CanParseAddressWithRangelessSecondaryUnit()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("1050 Broadway Penthouse, New York, NY 10001");

            Assert.AreEqual("NEW YORK", address.City);
            Assert.AreEqual("1050", address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual(null, address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual("PH", address.SecondaryUnit);
            Assert.AreEqual("NY", address.State);
            Assert.AreEqual("BROADWAY", address.Street);
            Assert.AreEqual("1050 BROADWAY PH", address.StreetLine);
            Assert.AreEqual(null, address.Suffix);
            Assert.AreEqual("10001", address.Zip);
        }

        [Test]
        public void CanParsePostOfficeBoxAddress()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("P.O. BOX 4857, New York, NY 10001");

            Assert.AreEqual("NEW YORK", address.City);
            Assert.AreEqual(null, address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual(null, address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual(null, address.SecondaryUnit);
            Assert.AreEqual("NY", address.State);
            Assert.AreEqual(null, address.Street);
            Assert.AreEqual("PO BOX 4857", address.StreetLine);
            Assert.AreEqual(null, address.Suffix);
            Assert.AreEqual("10001", address.Zip);
        }

        /// <summary>
        /// Military addresses seem to follow no convention whatsoever in the
        /// street line, but the APO/FPO/DPO AA/AE/AP 9NNNN part of the place line
        /// is pretty well standardized. I've made a special exception for these
        /// kinds of addresses so that the street line is just dumped as-is into
        /// the StreetLine field.
        /// </summary>
        [Test]
        public void CanParseMilitaryAddress()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("PSC BOX 453, APO AE 99969");

            Assert.AreEqual("APO", address.City);
            Assert.AreEqual(null, address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual(null, address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual(null, address.SecondaryUnit);
            Assert.AreEqual("AE", address.State);
            Assert.AreEqual(null, address.Street);
            Assert.AreEqual("PSC BOX 453", address.StreetLine);
            Assert.AreEqual(null, address.Suffix);
            Assert.AreEqual("99969", address.Zip);
        }

        [Test]
        public void CanParseAddressWithoutPunctuation()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("999 West 89th Street Apt A New York NY 10024");

            Assert.AreEqual("NEW YORK", address.City);
            Assert.AreEqual("999", address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual("W", address.Predirectional);
            Assert.AreEqual("A", address.SecondaryNumber);
            Assert.AreEqual("APT", address.SecondaryUnit);
            Assert.AreEqual("NY", address.State);
            Assert.AreEqual("89TH", address.Street);
            Assert.AreEqual("999 W 89TH ST APT A", address.StreetLine);
            Assert.AreEqual("ST", address.Suffix);
            Assert.AreEqual("10024", address.Zip);
        }

        /// <summary>
        /// Grid-style addresses are common in parts of Utah. The official USPS address database
        /// in this case treats "E" as a predirectional, "1700" as the street name, and "S" as a
        /// postdirectional, and nothing as the suffix, so that's how we parse it, too.
        /// </summary>
        [Test]
        public void CanParseGridStyleAddress()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("842 E 1700 S, Salt Lake City, UT 84105");

            Assert.AreEqual("SALT LAKE CITY", address.City);
            Assert.AreEqual("842", address.Number);
            Assert.AreEqual("S", address.Postdirectional);
            Assert.AreEqual("E", address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual(null, address.SecondaryUnit);
            Assert.AreEqual("UT", address.State);
            Assert.AreEqual("1700", address.Street);
            Assert.AreEqual("842 E 1700 S", address.StreetLine);
            Assert.AreEqual(null, address.Suffix);
            Assert.AreEqual("84105", address.Zip);
        }

        /// <summary>
        /// People in Wisconsin and Illinois are eating too much cheese, apparently, because
        /// you can encounter house numbers with letters in them. It's similar to the
        /// Utah grid-system, except the gridness is all crammed into the house number.
        /// </summary>
        [Test]
        public void CanParseAddressWithAlphanumericRange()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("N6W23001 BLUEMOUND ROAD, ROLLING MEADOWS, IL, 12345");

            Assert.AreEqual("ROLLING MEADOWS", address.City);
            Assert.AreEqual("N6W23001", address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual(null, address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual(null, address.SecondaryUnit);
            Assert.AreEqual("IL", address.State);
            Assert.AreEqual("BLUEMOUND", address.Street);
            Assert.AreEqual("N6W23001 BLUEMOUND RD", address.StreetLine);
            Assert.AreEqual("RD", address.Suffix);
            Assert.AreEqual("12345", address.Zip);
        }

        /// <summary>
        /// Speaking of weird addresses, sometimes people put a space in the number.
        /// USPS says we should squash it together.
        /// </summary>
        [Test]
        public void CanParseAddressWithSpacedAlphanumericRange()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("N645 W23001 BLUEMOUND ROAD, ROLLING MEADOWS, IL, 12345");

            Assert.AreEqual("ROLLING MEADOWS", address.City);
            Assert.AreEqual("N645W23001", address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual(null, address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual(null, address.SecondaryUnit);
            Assert.AreEqual("IL", address.State);
            Assert.AreEqual("BLUEMOUND", address.Street);
            Assert.AreEqual("N645W23001 BLUEMOUND RD", address.StreetLine);
            Assert.AreEqual("RD", address.Suffix);
            Assert.AreEqual("12345", address.Zip);
        }

        /// <summary>
        /// In parts of New York City, some people feel REALLY STRONGLY about
        /// the hyphen in their house number. The numbering system makes sense,
        /// but the USPS address database doesn't support hyphens in the number field.
        /// To the USPS, the hyphen does not exist, but the DMM specifically does say
        /// that "if present, the hyphen should not be removed."
        /// </summary>
        [Test]
        public void CanParseQueensStyleAddress()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("123-465 34th St New York NY 12345");

            Assert.AreEqual("NEW YORK", address.City);
            Assert.AreEqual("123-465", address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual(null, address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual(null, address.SecondaryUnit);
            Assert.AreEqual("NY", address.State);
            Assert.AreEqual("34TH", address.Street);
            Assert.AreEqual("123-465 34TH ST", address.StreetLine);
            Assert.AreEqual("ST", address.Suffix);
            Assert.AreEqual("12345", address.Zip);
        }

        /// <summary>
        /// In Virginia Beach, for example, there's a South Blvd, which could really
        /// throw a spanner into our predirectional/postdirectional parsing. We call
        /// this case out specifically in our regex.
        /// </summary>
        [Test]
        public void CanParseAddressWithCardinalStreetName()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("500 SOUTH STREET VIRGINIA BEACH VIRGINIA 23452");

            Assert.AreEqual("VIRGINIA BEACH", address.City);
            Assert.AreEqual("500", address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual(null, address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual(null, address.SecondaryUnit);
            Assert.AreEqual("VA", address.State);
            Assert.AreEqual("SOUTH", address.Street);
            Assert.AreEqual("500 SOUTH ST", address.StreetLine);
            Assert.AreEqual("ST", address.Suffix);
            Assert.AreEqual("23452", address.Zip);
        }

        /// <summary>
        /// When people live in apartments with letters, they sometimes attach the apartment
        /// letter to the end of the house number. This is wrong, and these people need to be
        /// lined up and individually slapped. We pull out the unit and designate it as "APT",
        /// which in my experience is the designator that USPS uses in the vast, vast majority
        /// of cases.
        /// </summary>
        [Test]
        public void CanParseAddressWithRangedUnitAttachedToNumber()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("403D BERRYFIELD LANE CHESAPEAKE VA 23224");

            Assert.AreEqual("CHESAPEAKE", address.City);
            Assert.AreEqual("403", address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual(null, address.Predirectional);
            Assert.AreEqual("D", address.SecondaryNumber);
            Assert.AreEqual("APT", address.SecondaryUnit);
            Assert.AreEqual("VA", address.State);
            Assert.AreEqual("BERRYFIELD", address.Street);
            Assert.AreEqual("403 BERRYFIELD LN APT D", address.StreetLine);
            Assert.AreEqual("LN", address.Suffix);
            Assert.AreEqual("23224", address.Zip);
        }

        /// <summary>
        /// At least it's not platform 9 3/4.
        /// </summary>
        [Test]
        public void CanParseFractionalAddress()
        {
            var parser = new AddressParser();
            var address = parser.ParseAddress("123 1/2 MAIN ST, RICHMOND, VA 23221");

            Assert.AreEqual("RICHMOND", address.City);
            Assert.AreEqual("123 1/2", address.Number);
            Assert.AreEqual(null, address.Postdirectional);
            Assert.AreEqual(null, address.Predirectional);
            Assert.AreEqual(null, address.SecondaryNumber);
            Assert.AreEqual(null, address.SecondaryUnit);
            Assert.AreEqual("VA", address.State);
            Assert.AreEqual("MAIN", address.Street);
            Assert.AreEqual("123 1/2 MAIN ST", address.StreetLine);
            Assert.AreEqual("ST", address.Suffix);
            Assert.AreEqual("23221", address.Zip);
        }
    }
}
