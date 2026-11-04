using FluentAssertions;
using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Mapper.Provedor;
using FunkoApiV._2.Models;
using MongoDB.Bson;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Mappers;

[TestFixture]
[Category("Unit")]
public class ProveedorMapperTests {
    [Test]
    public void ToDto_MapeaTodosLosCampos() {
        var id = ObjectId.GenerateNewId();
        var p = new Proveedor {
            Id = id, Nombre = "Acme", Direccion = "Calle 1",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = null
        };
        var dto = p.ToDto();
        dto.Id.Should().Be(id.ToString());
        dto.Nombre.Should().Be("Acme");
        dto.Direccion.Should().Be("Calle 1");
        dto.CreatedAt.Should().Be(p.CreatedAt);
        dto.UpdatedAt.Should().BeNull();
    }

    [Test]
    public void ToModel_DesdeRequest_MapeaYMarcaNoBorrado() {
        var req = new ProveedorRequestDto { Nombre = "Acme", Direccion = "Calle 1" };
        var m = req.ToModel();
        m.Nombre.Should().Be("Acme");
        m.Direccion.Should().Be("Calle 1");
        m.IsDeleted.Should().BeFalse();
        m.UpdatedAt.Should().BeNull();
    }
}
