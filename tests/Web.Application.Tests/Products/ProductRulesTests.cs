using System.Globalization;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Application.Tests.Products;

/// <summary>
///     Tests the business rules of the product fields.
/// </summary>
[TestFixture]
public class ProductRulesTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ValidateName_WhenEmpty_Fails(string? name)
    {
        Assert.That(ProductRules.ValidateName(name).IsFailed, Is.True);
    }

    [Test]
    public void ValidateName_AtTheLimit_Succeeds()
    {
        Assert.That(ProductRules.ValidateName(new('x', ProductLimits.MAX_NAME_LENGTH)).IsSuccess, Is.True);
    }

    [Test]
    public void ValidateName_OneCharacterOverTheLimit_Fails()
    {
        Assert.That(ProductRules.ValidateName(new('x', ProductLimits.MAX_NAME_LENGTH + 1)).IsFailed, Is.True);
    }

    [TestCase("0")]
    [TestCase("-1")]
    [TestCase("0.001")]
    [TestCase("1.005")]
    [TestCase("10000000000000000")]
    [TestCase("1e20")]
    public void ValidatePrice_WhenOutOfRule_Fails(string price)
    {
        Assert.That(ProductRules.ValidatePrice(Parse(price)).IsFailed, Is.True);
    }

    [TestCase("0.01")]
    [TestCase("1.5")]
    [TestCase("48900")]
    [TestCase("9999999999999999.99")]
    public void ValidatePrice_WhenWithinRule_Succeeds(string price)
    {
        Assert.That(ProductRules.ValidatePrice(Parse(price)).IsSuccess, Is.True);
    }

    [TestCase("javascript:alert(1)")]
    [TestCase("file:///etc/passwd")]
    [TestCase("ftp://example.com/a.jpg")]
    [TestCase("not-a-uri")]
    [TestCase(null)]
    public void ValidateImgUri_WhenNotHttpOrHttps_Fails(string? imgUri)
    {
        Assert.That(ProductRules.ValidateImgUri(imgUri).IsFailed, Is.True);
    }

    [TestCase("https://example.com/a.jpg")]
    [TestCase("http://example.com/a.jpg")]
    public void ValidateImgUri_WhenHttpOrHttps_Succeeds(string imgUri)
    {
        Assert.That(ProductRules.ValidateImgUri(imgUri).IsSuccess, Is.True);
    }

    [Test]
    public void ValidateImgUri_LongerThanTheLimit_Fails()
    {
        string imgUri = "https://example.com/" + new string('a', ProductLimits.MAX_IMG_URI_LENGTH);

        Assert.That(ProductRules.ValidateImgUri(imgUri).IsFailed, Is.True);
    }

    [Test]
    public void ValidateDescription_WhenNull_Succeeds()
    {
        Assert.That(ProductRules.ValidateDescription(null).IsSuccess, Is.True);
    }

    [Test]
    public void ValidateDescription_OneCharacterOverTheLimit_Fails()
    {
        Assert.That(ProductRules.ValidateDescription(new('x', ProductLimits.MAX_DESCRIPTION_LENGTH + 1)).IsFailed,
            Is.True);
    }

    private static decimal Parse(string value)
    {
        return decimal.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}
