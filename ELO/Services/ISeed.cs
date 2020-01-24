using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ELO.Services
{
    public interface ISeed
    {
        Task Seed();
    }
}
