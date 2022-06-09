using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

namespace ICL.Core.AgentPos
{
    /// <summary>
    /// The AgentPos namespace contains fundamental types that define commonly used value types and classes for computing the column agent start pos
    /// </summary>
    internal class NamespaceDoc { }

    internal class BeamInitializeAgents
    {
        public int ColumnNumbers;

        public List<Point3d> EnvironmentBoundary;
        public BeamInitializeAgents(int columnNumbers, List<Point3d> environmentBoundary)
        {
            this.ColumnNumbers = columnNumbers;
            this.EnvironmentBoundary = environmentBoundary;
        }
        //gets number of columns 
        //get environment geometry 

        public List<Point3d> ComputeColumnStartPos()
        {
            List<Point3d> columnStartPos = new List<Point3d>();

            if (this.ColumnNumbers == 0 )
            {
                RhinoApp.WriteLine("input column numbers greater than 0"); //this warning could be a pop up called from utilities
            }

            if (this.ColumnNumbers == this.EnvironmentBoundary.Count)
            {
                foreach(Point3d point in this.EnvironmentBoundary)
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
                Point3d[] point;
                var columnPos = line.ToNurbsCurve().DivideByCount((this.ColumnNumbers - 1), true, out point);
                foreach(var pt in point)
                {
                    columnStartPos.Add(new Point3d(pt[0], pt[1], pt[2]));
                }
            }

            return columnStartPos;
        }
        //if number of agents more than the number of boundary points 
        //divide the beam curve and position them evenly 
        //return column agent positions as list of points 
    }
}
