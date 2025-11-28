using Microsoft.AspNetCore.Identity;

namespace Dsw2025Tpi.Application.Services;

// Esta clase hereda del describer de errores por defecto de Identity
public class SpanishIdentityErrorDescriber : IdentityErrorDescriber
{
    // errores de usuario
    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = $"El nombre de usuario '{userName}' ya está en uso."
        };
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateEmail),
            Description = $"El email '{email}' ya está en uso."
        };
    }

    // errores de contraseña
    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = $"La contraseña debe tener al menos {length} caracteres."
        };
    }

    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "La contraseña debe tener al menos una mayúscula ('A'-'Z')."
        };
    }

    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = "La contraseña debe tener al menos una minúscula ('a'-'z')."
        };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = "La contraseña debe tener al menos un número ('0'-'9')."
        };
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "La contraseña debe tener al menos un carácter especial (ej. !, @, #)."
        };
    }
}

