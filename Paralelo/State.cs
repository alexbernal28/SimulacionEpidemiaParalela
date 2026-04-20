using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paralelo
{
    public enum State : byte
    {
        Susceptible = 0,
        Infected = 1,
        Recovered = 2,
        Dead = 3
    }
}
