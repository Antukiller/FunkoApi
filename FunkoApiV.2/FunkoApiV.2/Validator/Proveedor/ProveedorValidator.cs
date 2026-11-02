using FluentValidation;
using FunkoApiV._2.Dto.Proveedor;
using FunkoApiV._2.Infrastructure.Interfaces;

namespace FunkoApiV._2.Validator.Proveedor;

public class ProveedorValidator : AbstractValidator<ProveedorRequestDto>, ITransientService{
    public ProveedorValidator() {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MinimumLength(2).WithMessage("El nombre debe tener 2 caracteres al menos")
            .MaximumLength(20).WithMessage("El nombre solo puede tener 20 caracteres");
        RuleFor(x => x.Direccion)
            .NotEmpty().WithMessage("La direccion es obligatoria")
            .MinimumLength(2).WithMessage("La direccion debe tener 2 caracteres al menos")
            .MaximumLength(20).WithMessage("La direccion solo puede tener 200 caracteres");
    }
}