using System;
using System.Xml;
using NUnit.Framework;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Tests.Utils
{
    [TestFixture]
    public class DateTimeSerializerTests
        : TestBase
    {
        public void PrintFormats(DateTime dateTime)
        {
            Log("dateTime.ToShortDateString(): " + dateTime.ToShortDateString());
            Log("dateTime.ToShortTimeString(): " + dateTime.ToShortTimeString());
            Log("dateTime.ToLongTimeString(): " + dateTime.ToLongTimeString());
            Log("dateTime.ToShortTimeString(): " + dateTime.ToShortTimeString());
            Log("dateTime.ToString(): " + dateTime.ToString());
            Log("DateTimeSerializer.ToShortestXsdDateTimeString(dateTime): " + DateTimeSerializer.ToShortestXsdDateTimeString(dateTime));
            Log("DateTimeSerializer.ToDateTimeString(dateTime): " + DateTimeSerializer.ToDateTimeString(dateTime));
            Log("DateTimeSerializer.ToXsdDateTimeString(dateTime): " + DateTimeSerializer.ToXsdDateTimeString(dateTime));
            Log("\n");
        }

        public void PrintFormats(TimeSpan timeSpan)
        {
            Log("DateTimeSerializer.ToXsdTimeSpanString(timeSpan): " + DateTimeSerializer.ToXsdTimeSpanString(timeSpan));
            Log("\n");
        }

        [Test]
        public void PrintDate()
        {
            PrintFormats(DateTime.Now);
            PrintFormats(DateTime.UtcNow);
            PrintFormats(new DateTime(1979, 5, 9));
            PrintFormats(new DateTime(1979, 5, 9, 0, 0, 1));
            PrintFormats(new DateTime(1979, 5, 9, 0, 0, 0, 1));
            PrintFormats(new DateTime(2010, 10, 20, 10, 10, 10, 1));
            PrintFormats(new DateTime(2010, 11, 22, 11, 11, 11, 1));
        }

        [Test]
        public void PrintTimeSpan()
        {
            PrintFormats(new TimeSpan());
            PrintFormats(new TimeSpan(1));
            PrintFormats(new TimeSpan(1, 2, 3));
            PrintFormats(new TimeSpan(1, 2, 3, 4));
        }

        [Test]
        public void ToShortestXsdDateTimeString_works()
        {
            var shortDate = new DateTime(1979, 5, 9);
            const string shortDateString = "1979-05-09";

            var shortDateTime = new DateTime(1979, 5, 9, 0, 0, 1, DateTimeKind.Utc);
            var shortDateTimeString = shortDateTime.Equals(shortDateTime.ToStableUniversalTime())
                ? "1979-05-09T00:00:01Z"
                : "1979-05-08T23:00:01Z";

            var longDateTime = new DateTime(1979, 5, 9, 0, 0, 0, 1, DateTimeKind.Utc);
            var longDateTimeString = longDateTime.Equals(longDateTime.ToStableUniversalTime())
                ? "1979-05-09T00:00:00.001Z"
                : "1979-05-08T23:00:00.001Z";

            Assert.That(shortDateString, Is.EqualTo(DateTimeSerializer.ToShortestXsdDateTimeString(shortDate)));
            Assert.That(shortDateTimeString, Is.EqualTo(DateTimeSerializer.ToShortestXsdDateTimeString(shortDateTime)));
            Assert.That(longDateTimeString, Is.EqualTo(DateTimeSerializer.ToShortestXsdDateTimeString(longDateTime)));
        }

        [Test]
        public void CanDeserializeDateTimeOffsetWithTimeSpanIsZero()
        {
            var expectedValue = new DateTimeOffset(2012, 6, 27, 11, 26, 04, 524, TimeSpan.Zero);

            var s = DateTimeSerializer.ToWcfJsonDateTimeOffset(expectedValue);

            Assert.AreEqual("\\/Date(1340796364524)\\/", s);

            var afterValue = DateTimeSerializer.ParseWcfJsonDateOffset(s);

            Assert.AreEqual(expectedValue, afterValue);
        }

        [Test]
        [Ignore]
        public void Utc_Local_Equals()
        {
            var now = DateTime.Now;
            var utcNow = now.ToStableUniversalTime();

            Assert.That(now.Ticks, Is.EqualTo(utcNow.Ticks), "Ticks are different");
            Assert.That(now, Is.EqualTo(utcNow), "DateTimes are different");
        }

        [Test]
        public void ParseShortestXsdDateTime_works()
        {
            DateTime shortDate = DateTimeSerializer.ParseShortestXsdDateTime("2011-8-4");
            Assert.That(shortDate, Is.EqualTo(new DateTime(2011, 8, 4)), "Month and day without leading 0");
            shortDate = DateTimeSerializer.ParseShortestXsdDateTime("2011-8-05");
            Assert.That(shortDate, Is.EqualTo(new DateTime(2011, 8, 5)), "Month without leading 0");
            shortDate = DateTimeSerializer.ParseShortestXsdDateTime("2011-09-4");
            Assert.That(shortDate, Is.EqualTo(new DateTime(2011, 9, 4)), "Day without leading 0");
        }

        [Test]
        public void ParseRFC1123DateTime_works()
        {
            DateTime rfc1123Date = DateTimeSerializer.ParseRFC1123DateTime("Tue, 12 Nov 2013 14:32:07 GMT");
            Assert.That(rfc1123Date, Is.EqualTo(new DateTime(2013, 11, 12, 14, 32, 07)));
        }

        [Test]
        public void TestSqlServerDateTime()
        {
            var result = TypeSerializer.DeserializeFromString<DateTime>("2010-06-01 21:52:59.280");
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void DateTimeWithoutMilliseconds_should_always_be_deserialized_correctly_by_TypeSerializer()
        {
            var dateWithoutMillisecondsUtc = new DateTime(2013, 4, 9, 15, 20, 0, DateTimeKind.Utc);
            var dateWithoutMillisecondsLocal = new DateTime(2013, 4, 9, 15, 20, 0, DateTimeKind.Local);
            var dateWithoutMillisecondsUnspecified = new DateTime(2013, 4, 9, 15, 20, 0, DateTimeKind.Unspecified);

            string serialized = null;
            DateTime deserialized;

            serialized = TypeSerializer.SerializeToString(dateWithoutMillisecondsUtc);
            deserialized = TypeSerializer.DeserializeFromString<DateTime>(serialized);
            Assert.AreEqual(dateWithoutMillisecondsUtc.ToLocalTime(), deserialized);

            serialized = TypeSerializer.SerializeToString(dateWithoutMillisecondsLocal);
            deserialized = TypeSerializer.DeserializeFromString<DateTime>(serialized);
            Assert.AreEqual(dateWithoutMillisecondsLocal, deserialized);

            serialized = TypeSerializer.SerializeToString(dateWithoutMillisecondsUnspecified);
            deserialized = TypeSerializer.DeserializeFromString<DateTime>(serialized);
            Assert.AreEqual(dateWithoutMillisecondsUnspecified, deserialized);
        }

        [Test, Ignore("Don't pre-serialize into Utc")]
        public void UtcDateTime_Is_Deserialized_As_Kind_Utc()
        {
            //Serializing UTC
            var utcNow = new DateTime(2012, 1, 8, 12, 17, 1, 538, DateTimeKind.Utc);
            Assert.That(utcNow.Kind, Is.EqualTo(DateTimeKind.Utc));
            var serialized = JsonSerializer.SerializeToString(utcNow);
            //Deserializing UTC?
            var deserialized = JsonSerializer.DeserializeFromString<DateTime>(serialized);
            Assert.That(deserialized.Kind, Is.EqualTo(DateTimeKind.Utc)); //fails -> is DateTimeKind.Local
        }

        /// <summary>
        /// These timestamp strings were pulled from SQLite columns written via OrmLite using SQlite.1.88
        /// Most of the time, timestamps correctly use the 'T' separator between the date and time,
        /// but under some (still unknown) scnearios, SQLite will write timestamps using a space instead of a 'T'.
        /// If that happens, OrmLite will fail to read the row, complaining that: The string '...' is not a valid Xsd value.
        /// </summary>
        private static string[] _problematicXsdStrings = new[] {
	        "2013-10-10 20:04:04.8773249Z",
            "2013-10-10 20:04:04Z",
	    };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void CanParseProblematicXsdStrings(int whichString)
        {
            var xsdString = _problematicXsdStrings[whichString];

            var dateTime = DateTimeSerializer.ParseShortestXsdDateTime(xsdString);

            Assert.That(dateTime.Kind, Is.EqualTo(DateTimeKind.Local));
        }

        [Test]
        public void CanParseLongAndShortXsdStrings()
        {
            var shortXsdString = "2013-10-10T13:40:50Z";
            var longXsdString = shortXsdString.Substring(0, shortXsdString.Length - 1) + ".0000000" +
                                shortXsdString.Substring(shortXsdString.Length - 1);

            var dateTimeShort = DateTimeSerializer.ParseShortestXsdDateTime(shortXsdString);
            var dateTimeLong = DateTimeSerializer.ParseShortestXsdDateTime(longXsdString);

            Assert.That(dateTimeShort.Ticks, Is.EqualTo(dateTimeLong.Ticks));
            Assert.That(dateTimeShort.Kind, Is.EqualTo(dateTimeLong.Kind));
        }

        internal static readonly DateTime[] DateTimeTests = new[] {
			DateTime.Now,
			DateTime.UtcNow,
			new DateTime(1979, 5, 9),
			new DateTime(1972, 3, 24, 0, 0, 0, DateTimeKind.Local),
			new DateTime(1972, 4, 24),
			new DateTime(1979, 5, 9, 0, 0, 1),
			new DateTime(1979, 5, 9, 0, 0, 0, 1),
			new DateTime(2010, 10, 20, 10, 10, 10, 1),
			new DateTime(2010, 11, 22, 11, 11, 11, 1),
            new DateTime(622119282055250000)
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        //[TestCase(3)] //.NET Date BUG see: Test_MS_Dates
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void AssertDateIsEqual(int whichDate)
        {
            DateTime dateTime = DateTimeTests[whichDate];

            //Don't test short dates without time to UTC as you lose precision
            var shortDateStr = dateTime.ToString(DateTimeSerializer.ShortDateTimeFormat);
            var shortDateTimeStr = dateTime.ToStableUniversalTime().ToString(DateTimeSerializer.XsdDateTimeFormatSeconds);
            var longDateTimeStr = DateTimeSerializer.ToXsdDateTimeString(dateTime);
            var shortestDateStr = DateTimeSerializer.ToShortestXsdDateTimeString(dateTime);

            Log("{0} | {1} | {2}  [{3}]",
                shortDateStr, shortDateTimeStr, longDateTimeStr, shortestDateStr);

            var shortDate = DateTimeSerializer.ParseShortestXsdDateTime(shortDateStr);
            var shortDateTime = DateTimeSerializer.ParseShortestXsdDateTime(shortDateTimeStr);
            var longDateTime = DateTimeSerializer.ParseShortestXsdDateTime(longDateTimeStr);

            Assert.That(shortDate, Is.EqualTo(dateTime.Date));

            var shortDateTimeUtc = shortDateTime.ToStableUniversalTime();
            Assert.That(shortDateTimeUtc, Is.EqualTo(
                new DateTime(
                    shortDateTimeUtc.Year, shortDateTimeUtc.Month, shortDateTimeUtc.Day,
                    shortDateTimeUtc.Hour, shortDateTimeUtc.Minute, shortDateTimeUtc.Second,
                    shortDateTimeUtc.Millisecond, DateTimeKind.Utc)));

            AssertDatesAreEqual(longDateTime.ToStableUniversalTime(), dateTime.ToStableUniversalTime());

            var toDateTime = DateTimeSerializer.ParseShortestXsdDateTime(shortestDateStr);
            AssertDatesAreEqual(toDateTime, dateTime, "shortestDate");

            var unixTime = dateTime.ToUnixTimeMs();
            var fromUnixTime = DateTimeExtensions.FromUnixTimeMs(unixTime);
            AssertDatesAreEqual(fromUnixTime, dateTime, "unixTimeMs");

            var wcfDateString = DateTimeSerializer.ToWcfJsonDate(dateTime);
            var wcfDate = DateTimeSerializer.ParseWcfJsonDate(wcfDateString);
            AssertDatesAreEqual(wcfDate, dateTime, "wcf date");
        }

        private void AssertDatesAreEqual(DateTime toDateTime, DateTime dateTime, string which = null)
        {
            Assert.That(toDateTime.ToStableUniversalTime().RoundToMs(), Is.EqualTo(dateTime.ToStableUniversalTime().RoundToMs()), which);
        }

        [Test]
        public void Can_Serialize_new_DateTime()
        {
            var newDateTime = new DateTime();
            var convertedUnixTimeMs = newDateTime.ToUnixTimeMs();
            Assert.That(convertedUnixTimeMs.FromUnixTimeMs(), Is.EqualTo(newDateTime));
        }

        [Explicit("Test .NET Date Serialization behavior")]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void Test_MS_Dates(int whichDate)
        {
            var dateTime = DateTimeTests[whichDate];
            var dateTimeStr = XmlConvert.ToString(dateTime.ToStableUniversalTime(), XmlDateTimeSerializationMode.Utc);
            dateTimeStr.Print(); //1972-03-24T05:00:00Z

            var fromStr = DateTime.Parse(dateTimeStr);

            fromStr.ToString().Print();

            AssertDatesAreEqual(fromStr, dateTime);
        }
    }

    [TestFixture]
    public class DateTimeISO8601Tests
        : TestBase
    {
        public class TestObject
        {
            public DateTime Date { get; set; }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            JsConfig.Reset();
        }

        [Test]
        public void DateTime_Is_Serialized_As_Utc_and_Deserialized_as_local()
        {
            var testObject = new TestObject
            {
                Date = new DateTime(2013, 1, 1, 0, 0, 1, DateTimeKind.Utc)
            };

            Assert.AreEqual(DateTimeKind.Local, TypeSerializer.DeserializeFromString<TestObject>(TypeSerializer.SerializeToString<TestObject>(testObject)).Date.Kind);

            //Can change default behavior with config
            using (JsConfig.With(alwaysUseUtc: true))
            {
                Assert.AreEqual(DateTimeKind.Utc, TypeSerializer.DeserializeFromString<TestObject>(TypeSerializer.SerializeToString<TestObject>(testObject)).Date.Kind);
            }

            testObject = new TestObject
            {
                Date = new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            Assert.AreEqual(DateTimeKind.Local, TypeSerializer.DeserializeFromString<TestObject>(TypeSerializer.SerializeToString<TestObject>(testObject)).Date.Kind);

            //Can change default behavior with config
            using (JsConfig.With(alwaysUseUtc: true))
            {
                Assert.AreEqual(DateTimeKind.Utc, TypeSerializer.DeserializeFromString<TestObject>(TypeSerializer.SerializeToString<TestObject>(testObject)).Date.Kind);
            }
        }
    }

    [TestFixture]
    public class DateTimeRFC1123Tests
        : TestBase
    {
        public class TestObject
        {
            public DateTime Date { get; set; }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            JsConfig.DateHandler = JsonDateHandler.RFC1123;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            JsConfig.Reset();
        }

        [Test]
        public void DateTime_Is_Serialized_As_Utc_and_Deserialized_as_local()
        {
            var testObject = new TestObject
            {
                Date = new DateTime(2013, 1, 1, 0, 0, 1, DateTimeKind.Utc)
            };

            Assert.AreEqual(DateTimeKind.Local, TypeSerializer.DeserializeFromString<TestObject>(TypeSerializer.SerializeToString<TestObject>(testObject)).Date.Kind);

            //Can change default behavior with config
            using (JsConfig.With(alwaysUseUtc: true))
            {
                Assert.AreEqual(DateTimeKind.Utc, TypeSerializer.DeserializeFromString<TestObject>(TypeSerializer.SerializeToString<TestObject>(testObject)).Date.Kind);
            }

            testObject = new TestObject
            {
                Date = new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            Assert.AreEqual(DateTimeKind.Local, TypeSerializer.DeserializeFromString<TestObject>(TypeSerializer.SerializeToString<TestObject>(testObject)).Date.Kind);

            //Can change default behavior with config
            using (JsConfig.With(alwaysUseUtc: true))
            {
                Assert.AreEqual(DateTimeKind.Utc, TypeSerializer.DeserializeFromString<TestObject>(TypeSerializer.SerializeToString<TestObject>(testObject)).Date.Kind);
            }
        }
    }

    [TestFixture]
    public class UnixEpochTests
    {
        public class TestObject
        {
            public DateTime Date { get; set; }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            JsConfig.DateHandler = JsonDateHandler.UnixTime;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            JsConfig.Reset();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void Can_serialize_UnixEpoch_DateTimes(int whichDate)
        {
            var dateTime = DateTimeSerializerTests.DateTimeTests[whichDate].ToUniversalTime();
            var dto = new TestObject { Date = dateTime };
            var unixTime = dateTime.ToUnixTime();

            var expectedJson = "{\"Date\":" + unixTime + "}";
            var json = dto.ToJson();

            Assert.That(json, Is.EqualTo(expectedJson));

            var fromJson = json.FromJson<TestObject>();
            var nearestSec = TimeSpan.FromSeconds(1);
            Assert.That(fromJson.Date.RoundToSecond(), Is.EqualTo(dateTime.RoundToSecond()));
        }
    }

    [TestFixture]
    public class UnixEpochMsTests
    {
        public class TestObject
        {
            public DateTime Date { get; set; }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            JsConfig.DateHandler = JsonDateHandler.UnixTimeMs;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            JsConfig.Reset();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void Can_serialize_UnixEpochMs_DateTimes(int whichDate)
        {
            var dateTime = DateTimeSerializerTests.DateTimeTests[whichDate].ToUniversalTime();
            var dto = new TestObject { Date = dateTime };
            var unixTime = dateTime.ToUnixTimeMs();

            var expectedJson = "{\"Date\":" + unixTime + "}";
            var json = dto.ToJson();

            Assert.That(json, Is.EqualTo(expectedJson));

            var fromJson = json.FromJson<TestObject>();
            var nearestSec = TimeSpan.FromSeconds(1);
            Assert.That(fromJson.Date.RoundToMs(), Is.EqualTo(dateTime.RoundToMs()));
        }
    }
}