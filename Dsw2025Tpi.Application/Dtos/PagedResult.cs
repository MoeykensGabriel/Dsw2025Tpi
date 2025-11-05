using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalPages,
    int CurrentPage,
    int TotalCount
);
}
