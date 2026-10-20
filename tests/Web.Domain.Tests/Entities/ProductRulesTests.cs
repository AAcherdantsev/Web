using System.Globalization;
using Web.Domain.Entities;

namespace Web.Domain.Tests.Entities;

/// <summary>
///     Tests the business rules of the product fields. The request validators and the entity both call these rules.
/// </summary>
[TestFixture]
public class ProductRulesTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void IsNameProvided_WhenEmpty_IsFalse(string? name)
    {
        Assert.That(ProductRules.IsNameProvided(name), Is.False);
    }

    [Test]
    public void IsNameWithinLimit_AtTheLimit_IsTrue()
    {
        Assert.That(ProductRules.IsNameWithinLimit(new('x', ProductLimits.MAX_NAME_LENGTH)), Is.True);
    }

    [Test]
    public void IsNameWithinLimit_OneCharacterOverTheLimit_IsFalse()
    {
        Assert.That(ProductRules.IsNameWithinLimit(new('x', ProductLimits.MAX_NAME_LENGTH + 1)), Is.False);
    }

    [TestCase("0")]
    [TestCase("-1")]
    public void IsPricePositive_WhenZeroOrNegative_IsFalse(string price)
    {
        Assert.That(ProductRules.IsPricePositive(Parse(price)), Is.False);
    }

    [TestCase("0.01")]
    [TestCase("9999999999999999.99")]
    public void IsPriceWithinLimit_AtOrBelowTheLimit_IsTrue(string price)
    {
        Assert.That(ProductRules.IsPriceWithinLimit(Parse(price)), Is.True);
    }

    [TestCase("10000000000000000")]
    [TestCase("1e20")]
    public void IsPriceWithinLimit_AboveTheLimit_IsFalse(string price)
    {
        Assert.That(ProductRules.IsPriceWithinLimit(Parse(price)), Is.False);
    }

    [TestCase("0.001")]
    [TestCase("1.005")]
    public void HasAtMostTwoDecimalPlaces_WhenMoreDigits_IsFalse(string price)
    {
        Assert.That(ProductPrice.HasAtMostTwoDecimalPlaces(Parse(price)), Is.False);
    }

    [TestCase("javascript:alert(1)")]
    [TestCase("file:///etc/passwd")]
    [TestCase("ftp://example.com/a.jpg")]
    [TestCase("not-a-uri")]
    [TestCase(null)]
    public void IsImgUriValid_WhenNotHttpOrHttps_IsFalse(string? imgUri)
    {
        Assert.That(ProductRules.IsImgUriValid(imgUri), Is.False);
    }

    [TestCase("https://example.com/a.jpg")]
    [TestCase("http://example.com/a.jpg")]
    public void IsImgUriValid_WhenHttpOrHttps_IsTrue(string imgUri)
    {
        Assert.That(ProductRules.IsImgUriValid(imgUri), Is.True);
    }

    [Test]
    public void IsImgUriWithinLimit_OneCharacterOverTheLimit_IsFalse()
    {
        string imgUri = "https://example.com/" + new string('a', ProductLimits.MAX_IMG_URI_LENGTH);

        Assert.That(ProductRules.IsImgUriWithinLimit(imgUri), Is.False);
    }

    [Test]
    public void IsDescriptionWithinLimit_WhenMissing_IsTrue()
    {
        Assert.That(ProductRules.IsDescriptionWithinLimit(null), Is.True);
    }

    [Test]
    public void IsDescriptionWithinLimit_OneCharacterOverTheLimit_IsFalse()
    {
        Assert.That(ProductRules.IsDescriptionWithinLimit(new('x', ProductLimits.MAX_DESCRIPTION_LENGTH + 1)),
            Is.False);
    }

    [Test]
    public void Violations_WithValidValues_IsEmpty()
    {
        IReadOnlyList<string> violations =
            ProductRules.Violations("Phone", "https://example.com/p.jpg", 1.5m, "A phone");

        Assert.That(violations, Is.Empty);
    }

    private static decimal Parse(string value)
    {
        return decimal.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}
