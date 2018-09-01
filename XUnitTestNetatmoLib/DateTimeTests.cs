using System;
using NetatmoLib;
using Xunit;
using NetatmoLib;

namespace XUnitTestNetatmoLib
{
    public class DateTimeTests
    {
        // Test date time = August 1 2018 12:00:00
        int testTimeStamp = 1533124800;
        int testTimestampNow = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        private int testTimestamp30sec = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 30;

        private int testTimestamp1min = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 60;

        private int testTimestamp5mins = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 300;
        private int testTimestamp1hour = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3600;
        private int testTimestamp2hours = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 7200;



        [Fact]
        public void TestDateTimeOffset()
        {
            DateTime testDateTime = new DateTime(2018,8,1,12,0,0,DateTimeKind.Local);
            var timeStamp = DateTimeOps.GetDateTimeOffset(testTimeStamp);
            Assert.Equal(testDateTime.Ticks, timeStamp.Ticks);
        }

        [Fact]
        public void TestIsDataFresh()
        {
            // Compare with August 1 2018, this is too old, so it should fail
            Assert.False(NetatmoLib.DateTimeOps.IsDataFresh(testTimeStamp));
            // Compare to UtcNow, should count as fresh
            Assert.True(NetatmoLib.DateTimeOps.IsDataFresh(testTimestampNow));

        }

        [Fact]
        public void TestGetLastUpdateString()
        {

            

            Assert.Equal("less than a minute", NetatmoLib.DateTimeOps.GetLastUpdateString(testTimestamp30sec));
            Assert.Equal("a minute", NetatmoLib.DateTimeOps.GetLastUpdateString(testTimestamp1min));
            Assert.Equal("5 minutes", NetatmoLib.DateTimeOps.GetLastUpdateString(testTimestamp5mins));
            Assert.Equal("60 minutes", NetatmoLib.DateTimeOps.GetLastUpdateString(testTimestamp1hour));
            Assert.Equal("more than an hour", NetatmoLib.DateTimeOps.GetLastUpdateString(testTimestamp2hours));


        }
    }
}
