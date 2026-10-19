using System.Globalization;
using FluentValidation;
using FluentValidation.TestHelper;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Application.Tests.Products;

/// <summary>
///     Tests the business rules of the product fields using FluentValidation TestHelper.
/// </summary>
[TestFixture]
public class ProductRulesTests
{
    public class ProductTestModel
    {
        public string? Name { get; set; }

        public decimal Price { get; set; }

        public string? ImgUri { get; set; }

        public string? Description { get; set; }
    }

    public class ProductTestValidator : AbstractValidator<ProductTestModel>
    {
        public ProductTestValidator()
        {
            RuleFor(x => x.Name).ProductNameRules();
            RuleFor(x => x.Price).ProductPriceRules();
            RuleFor(x => x.ImgUri).ProductImageUrlRules();
            RuleFor(x => x.Description).ProductDescriptionRules();
        }
    }

    private ProductTestValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ValidateName_WhenEmpty_Fails(string? name)
    {
        ProductTestModel model = new() { Name = name };
        TestValidationResult<ProductTestModel> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void ValidateName_AtTheLimit_Succeeds()
    {
        ProductTestModel model = new() { Name = new('x', ProductLimits.MAX_NAME_LENGTH) };
        TestValidationResult<ProductTestModel> result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void ValidateName_OneCharacterOverTheLimit_Fails()
    {
        ProductTestModel model = new() { Name = new('x', ProductLimits.MAX_NAME_LENGTH + 1) };
        TestValidationResult<ProductTestModel> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [TestCase("0")]
    [TestCase("-1")]
    [TestCase("0.001")]
    [TestCase("1.005")]
    [TestCase("10000000000000000")]
    [TestCase("1e20")]
    public void ValidatePrice_WhenOutOfRule_Fails(string price)
    {
        ProductTestModel model = new() { Price = Parse(price) };
        TestValidationResult<ProductTestModel> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [TestCase("0.01")]
    [TestCase("1.5")]
    [TestCase("48900")]
    [TestCase("9999999999999999.99")]
    public void ValidatePrice_WhenWithinRule_Succeeds(string price)
    {
        ProductTestModel model = new() { Price = Parse(price) };
        TestValidationResult<ProductTestModel> result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [TestCase("javascript:alert(1)")]
    [TestCase("file:///etc/passwd")]
    [TestCase("ftp://example.com/a.jpg")]
    [TestCase("not-a-uri")]
    [TestCase(null)]
    public void ValidateImgUri_WhenNotHttpOrHttps_Fails(string? imgUri)
    {
        ProductTestModel model = new() { ImgUri = imgUri };
        TestValidationResult<ProductTestModel> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ImgUri);
    }

    [TestCase("https://example.com/a.jpg")]
    [TestCase("http://example.com/a.jpg")]
    public void ValidateImgUri_WhenHttpOrHttps_Succeeds(string imgUri)
    {
        ProductTestModel model = new() { ImgUri = imgUri };
        TestValidationResult<ProductTestModel> result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.ImgUri);
    }

    [Test]
    public void ValidateImgUri_LongerThanTheLimit_Fails()
    {
        ProductTestModel model = new()
        {
            ImgUri = "https://example.com/" + new string('a', ProductLimits.MAX_IMG_URI_LENGTH)
        };
        TestValidationResult<ProductTestModel> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.ImgUri);
    }

    [Test]
    public void ValidateDescription_WhenNull_Succeeds()
    {
        ProductTestModel model = new() { Description = null };
        TestValidationResult<ProductTestModel> result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void ValidateDescription_OneCharacterOverTheLimit_Fails()
    {
        ProductTestModel model = new() { Description = new('x', ProductLimits.MAX_DESCRIPTION_LENGTH + 1) };
        TestValidationResult<ProductTestModel> result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    private static decimal Parse(string value)
    {
        return decimal.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}
