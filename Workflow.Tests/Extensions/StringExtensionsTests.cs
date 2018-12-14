using Workflow.Extensions;
using Xunit;

namespace Workflow.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("alias@domain.com.au", true)]
        [InlineData("this.is@valid", true)]
        [InlineData("thisisjusta-string", false)]
        public void Can_Validate_Email(string value, bool expected)
        {
            Assert.Equal(expected, value.IsValidEmailAddress());
        }

        [Theory]
        [InlineData("pascalCasedString", "Pascal Cased String")]
        [InlineData(null, null)]
        public void Can_Convert_String_Casing(string value, string expected)
        {
            Assert.Equal(expected, value.ToTitleCase());
        }

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("string has a value", true)]
        public void Returns_True_When_String_Has_Value(string value, bool expected)
        {
            Assert.Equal(expected, value.HasValue());
        }

        [Theory]
        [InlineData("", true)]
        [InlineData(null, true)]
        [InlineData("string has a value", false)]
        public void Returns_True_When_String_Has_No_Value(string value, bool expected)
        {
            Assert.Equal(expected, value.HasNoValue());
        }
    }
}
