using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheWorld.Services
{
    public interface ICoordService
    {
      Task<CoordServiceResult> LookUp(string location);
    }
}
