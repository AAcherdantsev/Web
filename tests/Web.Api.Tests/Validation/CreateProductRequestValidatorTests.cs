using FluentValidation.TestHelper;
using Web.Api.Features.Products.CreateProduct;

namespace Web.Api.Tests.Validation;

[TestFixture]
public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    private static CreateProductRequest ValidRequest()
    {
        return new() { Name = "Phone", ImgUri = "https://example.com/phone.jpg", Price = 10, Description = "desc" };
    }

    [Test]
    public void ValidRequest_HasNoErrors()
    {
        _validator.TestValidate(ValidRequest()).ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void EmptyDescription_IsAllowed()
    {
        CreateProductRequest request = ValidRequest();
        request = new() { Name = request.Name, ImgUri = request.ImgUri, Price = request.Price };

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Name_WhenEmptyOrWhitespace_HasError(string name)
    {
        CreateProductRequest request = new() { Name = name, ImgUri = "https://example.com/a.jpg", Price = 1 };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Price_WhenNotPositive_HasError(decimal price)
    {
        CreateProductRequest request = new() { Name = "A", ImgUri = "https://example.com/a.jpg", Price = price };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Price);
    }

    [TestCase("not-a-uri")]
    [TestCase("")]
    public void ImgUri_WhenNotAbsoluteUri_HasError(string imgUri)
    {
        CreateProductRequest request = new() { Name = "A", ImgUri = imgUri, Price = 1 };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ImgUri);
    }

    [Test]
    public void Name_LongerThanTheLimit_HasError()
    {
        CreateProductRequest request = new()
        {
            Name = new('x', Domain.Entities.ProductLimits.MAX_NAME_LENGTH + 1),
            ImgUri = "https://example.com/a.jpg",
            Price = 1
        };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Description_LongerThanTheLimit_HasError()
    {
        CreateProductRequest request = new()
        {
            Name = "A",
            ImgUri = "https://example.com/a.jpg",
            Price = 1,
            Description = new('x', Domain.Entities.ProductLimits.MAX_DESCRIPTION_LENGTH + 1)
        };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Description_WhenNull_IsAllowed()
    {
        CreateProductRequest request = ValidRequest();
        request = new() { Name = request.Name, ImgUri = request.ImgUri, Price = request.Price, Description = null };

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [TestCase("javascript:alert(1)")]
    [TestCase("file:///etc/passwd")]
    [TestCase("ftp://example.com/a.jpg")]
    public void ImgUri_WithSchemeOtherThanHttp_HasError(string imgUri)
    {
        CreateProductRequest request = new() { Name = "A", ImgUri = imgUri, Price = 1 };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ImgUri);
    }
}
