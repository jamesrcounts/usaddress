namespace AddressParser.Tests
{
    using System.Collections.Generic;

    using ApprovalTests;

    using USAddress;

    using Xunit;

    public class AddressParseResultTests
    {
        private static readonly Dictionary<string, string> Fields = new Dictionary<string, string>
                                                                        {
                                                                            {
                                                                                Components
                                                                                .Number,
                                                                                "999"
                                                                            },
                                                                            {
                                                                                Components
                                                                                .Predirectional,
                                                                                "N"
                                                                            },
                                                                            {
                                                                                Components
                                                                                .Street,
                                                                                "Main"
                                                                            },
                                                                            {
                                                                                Components
                                                                                .Postdirectional,
                                                                                "NW"
                                                                            },
                                                                            {
                                                                                Components                                                                                .SecondaryUnit,
                                                                                "Apt"
                                                                            },
                                                                            {
                                                                                Components.SecondaryNumber,
                                                                                "11"
                                                                            },
                                                                            {
                                                                                Components.State,
                                                                                "CA"
                                                                            },
                                                                            {
                                                                                Components.Suffix,
                                                                                "St"
                                                                            },                                                                            {
                                                                                Components.Zip,
                                                                                "99999"
                                                                            },{
                                                                                Components.City,
                                                                                "San Diego"
                                                                            },
                                                                        };

        private static AddressParseResult AddressParseResult
        {
            get
            {
                return new AddressParseResult(Fields);
            }
        }

        [Fact]
        public void HandleMissingFields()
        {
            Assert.Equal("; ,   ", new AddressParseResult(null) + "");
        }

        [Fact]
        public void VerifyCity()
        {
            Assert.Equal("San Diego", AddressParseResult.City);
        }

        [Fact]
        public void VerifyExplicitStreetLine()
        {
            Assert.Equal("PO BOX 123", new AddressParseResult(new Dictionary<string, string>() { { Components.StreetLine, "PO BOX 123" } }).StreetLine);
        }

        [Fact]
        public void VerifyNumber()
        {
            Assert.Equal("999", AddressParseResult.Number);
        }

        [Fact]
        public void VerifyPostdirectional()
        {
            Assert.Equal("NW", AddressParseResult.Postdirectional);
        }

        [Fact]
        public void VerifyPredirectional()
        {
            Assert.Equal("N", AddressParseResult.Predirectional);
        }

        [Fact]
        public void VerifySecondaryNumber()
        {
            Assert.Equal("11", AddressParseResult.SecondaryNumber);
        }

        [Fact]
        public void VerifySecondaryUnit()
        {
            Assert.Equal("Apt", AddressParseResult.SecondaryUnit);
        }

        [Fact]
        public void VerifyState()
        {
            Assert.Equal("CA", AddressParseResult.State);
        }

        [Fact]
        public void VerifyStreet()
        {
            Assert.Equal("Main", AddressParseResult.Street);
        }

        [Fact]
        public void VerifyStreetLine()
        {
            Approvals.Verify(AddressParseResult.StreetLine);
        }

        [Fact]
        public void VerifySuffix()
        {
            Assert.Equal("St", AddressParseResult.Suffix);
        }

        [Fact]
        public void VerifyZip()
        {
            Assert.Equal("99999", AddressParseResult.Zip);
        }
    }
}