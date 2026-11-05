using FluentAssertions;
using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Validator.Proveedor;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Validators;

[TestFixture]
[Category("Unit")]
public class ProveedorValidatorTests {
    private ProveedorValidator _validator = null!;
    [SetUp] public void SetUp() => _validator = new ProveedorValidator();

    [Test] public async Task Validate_ConDatosValidos_EsValido() {
        var r = await _validator.ValidateAsync(new ProveedorRequestDto { Nombre = "Acme", Direccion = "Calle 1" });
        r.IsValid.Should().BeTrue();
    }

    [Test]
    public async Task Validate_ConCamposEnLongitudMaxima_EsValido() {
        var r = await _validator.ValidateAsync(new ProveedorRequestDto {
            Nombre = new string('N', 20),
            Direccion = new string('D', 20)
        });

        r.IsValid.Should().BeTrue();
        r.Errors.Should().BeEmpty();
    }

    [Test] public async Task Validate_ConNombreVacio_TieneError() {
        var r = await _validator.ValidateAsync(new ProveedorRequestDto { Nombre = "", Direccion = "Calle 1" });
        r.IsValid.Should().BeFalse();
        r.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Test]
    public async Task Validate_ConNombreMayorALimite_TieneError() {
        var r = await _validator.ValidateAsync(new ProveedorRequestDto {
            Nombre = new string('N', 21), Direccion = "Calle 1"
        });

        r.IsValid.Should().BeFalse();
        r.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Test] public async Task Validate_ConDireccionCorta_TieneError() {
        var r = await _validator.ValidateAsync(new ProveedorRequestDto { Nombre = "Acme", Direccion = "X" });
        r.IsValid.Should().BeFalse();
        r.Errors.Should().Contain(e => e.PropertyName == "Direccion");
    }

    [Test]
    public async Task Validate_ConNombreYDireccionInvalidos_DevuelveErroresMixtos() {
        var r = await _validator.ValidateAsync(new ProveedorRequestDto {
            Nombre = "", Direccion = "X"
        });

        r.IsValid.Should().BeFalse();
        r.Errors.Select(e => e.PropertyName).Should().Contain(["Nombre", "Direccion"]);
    }
}
