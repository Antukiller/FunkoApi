using FluentAssertions;
using FunkoApiV._2.Error;
using FunkoApiV._2.Repository;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;

namespace FunkoApiV2Test.Proveedor.Repository;

[TestFixture]
public class ProveedorRepositoryTests : ProveedorRepositoryBaseTest {
    private ProveedorRepository _repository = null!;

    [SetUp]
    public void SetUpRepository() {
        _repository = new ProveedorRepository(DbContext, NullLogger<ProveedorRepository>.Instance);
    }

    [TestFixture]
    public class CasosPositivos : ProveedorRepositoryTests {

        [Test]
        public async Task GetAll_devuelve_los_proveedores() {
            await CreateTestProveedor("Luis", "España");
            await CreateTestProveedor("Antonio", "España");

            var resultado = await _repository.GetAll();

            resultado.Should().HaveCount(3);   // 2 + el del seed
        }

        [Test]
        public async Task GetAll_ignora_los_borrados() {
            await _repository.SoftDelete(ProveedorId.ToString());

            (await _repository.GetAll()).Should().BeEmpty();
        }

        [Test]
        public async Task GetAllDeleted_devuelve_solo_los_borrados() {
            await _repository.SoftDelete(ProveedorId.ToString());

            var resultado = await _repository.GetAllDeleted();

            resultado.Should().ContainSingle(p => p.Id == ProveedorId);
        }

        [Test]
        public async Task GetById_devuelve_el_proveedor() {
            var resultado = await _repository.GetById(ProveedorId.ToString());

            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Id.Should().Be(ProveedorId);
            resultado.Value.Nombre.Should().Be("Proveedor");
        }

        [Test]
        public async Task Add_persiste_el_proveedor() {
            var creado = await _repository.Add(new FunkoApiV._2.Models.Proveedor { Nombre = "Nuevo", Direccion = "Calle 3" });

            creado.IsSuccess.Should().BeTrue();
            creado.Value.Id.Should().NotBe(ObjectId.Empty);

            var recuperado = await _repository.GetById(creado.Value.Id.ToString());
            recuperado.IsSuccess.Should().BeTrue();
            recuperado.Value.Nombre.Should().Be("Nuevo");
        }

        [Test]
        public async Task Update_guarda_los_cambios() {
            var actualizado = await _repository.Update(ProveedorId.ToString(), new FunkoApiV._2.Models.Proveedor {
                Nombre = "Renombrado",
                Direccion = "Calle 99"
            });

            actualizado.IsSuccess.Should().BeTrue();
            actualizado.Value.Nombre.Should().Be("Renombrado");
            actualizado.Value.UpdatedAt.Should().NotBeNull();
        }

        [Test]
        public async Task SoftDelete_lo_marca_borrado() {
            var resultado = await _repository.SoftDelete(ProveedorId.ToString());

            resultado.IsSuccess.Should().BeTrue();
            (await _repository.GetAll()).Should().BeEmpty();
        }

        [Test]
        public async Task Restore_lo_devuelve_al_listado() {
            await _repository.SoftDelete(ProveedorId.ToString());

            var resultado = await _repository.Restore(ProveedorId.ToString());

            resultado.IsSuccess.Should().BeTrue();
            (await _repository.GetAll()).Should().ContainSingle(p => p.Id == ProveedorId);
        }
    }

    [TestFixture]
    public class CasosNegativos : ProveedorRepositoryTests {

        [Test]
        public async Task GetById_con_id_inexistente_devuelve_NotFound() {
            var resultado = await _repository.GetById(ProveedorInexistenteId.ToString());

            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<NotFound>();
        }

        [Test]
        public async Task GetById_con_id_mal_formado_devuelve_ValidationError() {
            var resultado = await _repository.GetById("no-es-un-objectid");

            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<ValidationError>();
        }

        [Test]
        public async Task Update_de_un_id_inexistente_devuelve_NotFound() {
            var resultado = await _repository.Update(ProveedorInexistenteId.ToString(), new FunkoApiV._2.Models.Proveedor {
                Nombre = "Fantasma",
                Direccion = "Ninguna"
            });

            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<NotFound>();
        }

        [Test]
        public async Task SoftDelete_de_un_id_inexistente_devuelve_NotFound() {
            var resultado = await _repository.SoftDelete(ProveedorInexistenteId.ToString());

            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<NotFound>();
        }

        [Test]
        public async Task Restore_de_un_proveedor_no_borrado_devuelve_NotFound() {
            var resultado = await _repository.Restore(ProveedorId.ToString());

            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<NotFound>();
        }
    }
}