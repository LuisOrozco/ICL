using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

///Copy "..\bin\yourAddon.dll" "$(AppData)\Grasshopper\Libraries\yourAddon.dll"


namespace ICL.Core.AgentPos
{
    /// <summary>
    /// The AgentPos namespace contains fundamental types that define commonly used value types and classes for computing the column agent start pos
    /// </summary>
    //internal class NamespaceDoc { }

    public class BeamInitializeAgents
    {
        public int ColumnNumbers;
        public List<Point3d> EnvironmentBoundary;
        public BeamInitializeAgents(int columnNumbers, List<Point3d> environmentBoundary)
        {
            this.ColumnNumbers = columnNumbers;
            this.EnvironmentBoundary = environmentBoundary;
        }

        public List<Point3d> ComputeColumnStartPos()
        {
            List<Point3d> columnStartPos = new List<Point3d>();

            if (this.ColumnNumbers == this.EnvironmentBoundary.Count)
            {
                foreach (Point3d point in this.EnvironmentBoundary)
                {
                    columnStartPos.Add(point);
                }
            }

            if (this.ColumnNumbers < this.EnvironmentBoundary.Count && this.ColumnNumbers > 0)
            {
                Line line = new Line(this.EnvironmentBoundary[0], this.EnvironmentBoundary[1]);
                Point3d midPoint = line.PointAt(0.5);
                columnStartPos.Add(midPoint);
            }

            if (this.ColumnNumbers > this.EnvironmentBoundary.Count)
            {
                Line line = new Line(this.EnvironmentBoundary[0], this.EnvironmentBoundary[1]);
                Point3d[] points;
                var columnPos = line.ToNurbsCurve().DivideByCount((this.ColumnNumbers - 1), true, out points);
                foreach (var pt in points)
                {
                    columnStartPos.Add(new Point3d(pt[0], pt[1], pt[2]));
                }

            }
            return columnStartPos;
        }

    }
}
