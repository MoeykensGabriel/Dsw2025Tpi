using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions
{
    // 404 para not found
    public class EntityNotFoundException: Exception
    {
        public EntityNotFoundException(string message):base(message) { }
    }

    // 409 para conflictos por ej para datos duplicados
    public class DuplicatedEntityException: Exception 
    {
        public DuplicatedEntityException(string message): base(message) { }
    }

    // 400 bad request
    public class BadRequestException: Exception
    {
        public BadRequestException(string message):base(message) { }
    }

    // 401 para unauthorized
    public class  UnauthorizedException: Exception
    {
        public UnauthorizedException(string message) : base(message){ }
    }

    // 409 para conflictos generales
    public class ConflictException : Exception
    {
        public ConflictException(string message):base(message) { }
    }        
    
}
