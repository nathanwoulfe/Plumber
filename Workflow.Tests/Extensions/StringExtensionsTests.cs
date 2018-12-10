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
    }
}
