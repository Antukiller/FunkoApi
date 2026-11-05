using FluentAssertions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Validator;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Validators;

[TestFixture]
[Category("Unit")]
public class CategoriaValidatorTests {
    private CategoriaValidator _validator = null!;
    [SetUp] public void SetUp() => _validator = new CategoriaValidator();

    [Test] public async Task Validate_ConDatosValidos_EsValido() {
        var r = await _validator.ValidateAsync(new CategoriaRequestDto { Nombre = "Anime", Descripcion = "Series" });
        r.IsValid.Should().BeTrue();
    }

    [Test]
    public async Task Validate_ConNombreYDescripcionEnLimite_EsValido() {
        var r = await _validator.ValidateAsync(new CategoriaRequestDto {
            Nombre = new string('A', 100),
            Descripcion = new string('x', 500)
        });

        r.IsValid.Should().BeTrue();
        r.Errors.Should().BeEmpty();
    }

    [Test]
    public async Task Validate_ConNombreVacio_TieneError() {
        var r = await _validator.ValidateAsync(new CategoriaRequestDto { Nombre = "" });

        r.IsValid.Should().BeFalse();
        r.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Test] public async Task Validate_ConNombreCorto_TieneError() {
        var r = await _validator.ValidateAsync(new CategoriaRequestDto { Nombre = "ab" });
        r.IsValid.Should().BeFalse();
        r.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }
    [Test] public async Task Validate_ConDescripcionMuyLarga_TieneError() {
        var r = await _validator.ValidateAsync(new CategoriaRequestDto { Nombre = "Anime", Descripcion = new string('x', 501) });
        r.IsValid.Should().BeFalse();
        r.Errors.Should().Contain(e => e.PropertyName == "Descripcion");
    }

    [Test]
    public async Task Validate_ConNombreYDescripcionInvalidos_DevuelveErroresMixtos() {
        var r = await _validator.ValidateAsync(new CategoriaRequestDto {
            Nombre = "ab",
            Descripcion = new string('x', 501)
        });

        r.IsValid.Should().BeFalse();
        r.Errors.Select(e => e.PropertyName).Should().Contain(["Nombre", "Descripcion"]);
    }
}
