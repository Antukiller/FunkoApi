using FluentValidation;
using FunkoApiV._2.Dto;
using FunkoApiV._2.Infrastructure.Interfaces;

namespace FunkoApiV._2.Validator;

public class CategoriaValidator : AbstractValidator<CategoriaRequestDto>, ITransientService {
    public CategoriaValidator() {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MinimumLength(3).WithMessage("El nombre debe tener 3 caracteres al menos")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres");
    }
}
