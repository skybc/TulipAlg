using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public LineD(double startX, double startY, double endX, double endY)
        {
            Start = new PointD(startX, startY);
            End = new PointD(endX, endY);
        }

        /// <summary>
        /// X轴的正方向到线段的角度（弧度）
        /// </summary>
        public double GetAngleXRad()
        {
            double deltaY = End.Y - Start.Y;
            double deltaX = End.X - Start.X;
            return Math.Atan2(deltaY, deltaX);// * (180.0 / Math.PI);
        }

        // Y轴的正方向到线段的角度（弧度）
        public double GetAngleYRad()
        {
            double deltaY = End.Y - Start.Y;
            double deltaX = End.X - Start.X;
            return Math.Atan2(deltaX, deltaY);// * (180.0 / Math.PI);
        }


        public double GetLength()
        {
            double deltaY = End.Y - Start.Y;
            double deltaX = End.X - Start.X;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
        // Direction
        public Vector2 GetDirection()
        {
            double length = GetLength();
            if (length == 0)
            {
                return new Vector2(0, 0);
            }
            return new Vector2((float)((End.X - Start.X) / length), (float)((End.Y - Start.Y) / length));

        }

        public PointD GetMidPoint()
        {
            PointD midPoint = new PointD((Start.X + End.X) / 2, (Start.Y + End.Y) / 2);
            return midPoint;
        }
    }
}
