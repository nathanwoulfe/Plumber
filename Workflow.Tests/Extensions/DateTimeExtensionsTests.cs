using System;
using Workflow.Extensions;
using Xunit;

namespace Workflow.Tests.Extensions
{
    public class DateTimeExtensionsTests
    {
        private DateTime _date;

        [Fact]
        public void Can_Get_Friendly_Date_With_Minutes()
        {
            _date = new DateTime(2018, 11, 6, 4, 5, 0);
            Assert.Equal("6 November 2018 4:05AM", _date.ToFriendlyDate());
        }

        [Fact]
        public void Can_Get_Friendly_Date_Without_Minutes()
        {
            _date = new DateTime(2014, 7, 1, 22, 0, 0);
            Assert.Equal("1 July 2014 10PM", _date.ToFriendlyDate());
        }
    }
}
