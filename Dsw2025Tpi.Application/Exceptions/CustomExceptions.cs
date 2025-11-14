using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions;

// 404 para not found
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
}

// 409 para conflictos por ej para datos duplicados 
public class DuplicatedEntityException : Exception
{
    public DuplicatedEntityException(string message) : base(message) { }
}

// 400 bad request
public class BadRequestException : Exception
{
    // Añadimos una propiedad pública para el código
    public string? ErrorCode { get; }
    public BadRequestException(string message) : base(message) { }
    // Creamos un nuevo constructor que acepte el código
    public BadRequestException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}

// 401 para unauthorized
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

// 409 para conflictos generales en las rgl de negoc
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}


