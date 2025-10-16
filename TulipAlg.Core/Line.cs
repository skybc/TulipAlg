using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TulipAlg.Core
{
    /// <summary>
    /// 线
    /// </summary>
    public struct LineD
    {
        public PointD Start { get; set; }
        public PointD End { get; set; }
        public LineD(PointD start, PointD end)
        {
            Start = start;
            End = end;
        }
    }
}
