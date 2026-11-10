using FluentValidation;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Infrastructure.Interfaces;

namespace FunkoApiV._2.Validator;

public class FunkoUpdateValidator :  AbstractValidator<FunkoUpdateRequest>, ITransientService{
    public FunkoUpdateValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MinimumLength(2).WithMessage("El nombre debe tener 2 caracteres al menos")
            .MaximumLength(20).WithMessage("El nombre solo puede tener 20 caracteres");

        RuleFor(x => x.Precio)
            .InclusiveBetween(0.01m, 50.0m).WithMessage("El precio debe estar entre 0.01 y 50.0");
        
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");
        
        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("El ID de la categoría es obligatorio")
            .Length(11).WithMessage("El ID de la categoría debe tener 11 caracteres");

        RuleFor(x => x.ProveedorId)
            .NotEmpty().WithMessage("El ID del proveedor es obligatorio")
            .Matches("^[0-9a-fA-F]{24}$")
            .WithMessage("El ID del proveedor debe ser un ObjectId de 24 caracteres hex");

        RuleFor(x => x.Imagen)
            .MaximumLength(255).WithMessage("La imagen no puede superar los 255 caracteres")
            .Must(uri => string.IsNullOrWhiteSpace(uri) || FunkoImageReference.IsValid(uri))
            .WithMessage("La imagen debe ser una URL HTTP/HTTPS o una ruta que empiece por /uploads/funkos/");
    }
}