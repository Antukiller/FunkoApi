using FluentAssertions;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Mapper;
using FunkoApiV._2.Models;
using NUnit.Framework;

namespace FunkoApiV2Test.Unit.Mappers;

[TestFixture]
[Category("Unit")]
public class FunkoMapperTests {
    [Test]
    public void ToDto_MapeaTodosLosCampos() {
        var f = new Funko {
            Id = 1, Name = "Kirito", Precio = 15.99m, Stock = 10,
            Imagen = "/uploads/funkos/a.png", CategoriaId = "Anime000001",
            ProveedorId = "672f1a8b3c9d4e5f6a7b8c9d",
            Categoria = new Categoria { Id = "Anime000001", Nombre = "Anime" },
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var dto = f.ToDto();
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Kirito");
        dto.Categoria.Should().Be("Anime");
        dto.CategoriaId.Should().Be("Anime000001");
        dto.ProveedorId.Should().Be("672f1a8b3c9d4e5f6a7b8c9d");
        dto.Imagen.Should().Be("/uploads/funkos/a.png");
    }
    [Test]
    public void ToModel_DesdeCreateRequest_MapeaYMarcaNoBorrado() {
        var req = new FunkoCreateRequest {
            Name = "Kirito", Precio = 15.99m, Stock = 10,
            CategoriaId = "Anime000001", ProveedorId = "672f1a8b3c9d4e5f6a7b8c9d",
            Imagen = "/uploads/funkos/a.png"
        };
        var m = req.ToModel();
        m.Name.Should().Be("Kirito");
        m.IsDeleted.Should().BeFalse();
        m.UpdatedAt.Should().BeNull();
    }
}
