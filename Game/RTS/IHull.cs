using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Penumbra;

namespace CarbonField
{
    public interface IHull
    {
        Hull Hull { get; set; }
        void AddHull(LightingManager lightingManager);
    }
}
