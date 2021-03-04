using System;
using System.Collections.Generic;
using Xunit;

namespace ControlFlowPractise.Core.Tests
{
    public class RequestBuilderTests
    {
        public static IEnumerable<object[]> ConvertTransactionDateTestData()
        {
            yield return new object[] { new DateTime(2021, 3, 4, 0, 52, 0), "2021-03-04T11:52:00+11:00" };
            yield return new object[] { new DateTime(2021, 3, 4, 0, 52, 0, DateTimeKind.Utc), "2021-03-04T11:52:00+11:00" };
            yield return new object[] { new DateTime(2021, 9, 4, 0, 52, 0), "2021-09-04T10:52:00+10:00" };
        }

        [Trait("assessability", "internal")]
        [Theory]
        [MemberData(nameof(ConvertTransactionDateTestData))]
        public void ConvertTransactionDate(DateTime source, string expected)
        {
            var requestBuilder = new RequestBuilder();
            var actual = requestBuilder.GetTransactionDateString(source);
            Assert.Equal(expected, actual);
        }
    }
}
