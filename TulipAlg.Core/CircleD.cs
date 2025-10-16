using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TulipAlg.Core
{
    public struct  CircleD
    {
        public PointD Center { get; set; }
        public double Radius { get; set; }
        public CircleD(PointD center, double radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
