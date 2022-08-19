using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Rhino;

namespace ICL.Core.Utilities
{
    public class SlabInitializeAgents
    {
        /// <summary>
        /// number of columns at start 
        /// </summary>
        public int ColumnNumbers;
        /// <summary>
        /// List of Beam boundary points (length will always be 2, not more, not less)
        /// </summary>
        public Curve EnvironmentBoundary;

        public SlabInitializeAgents(int columnNumbers, Curve environmentBoundary)

        {
            this.ColumnNumbers = columnNumbers;
            this.EnvironmentBoundary = environmentBoundary;
        }

        /// <summary>
        /// Computes the start pos of columns in the slab base on the number of start columns (cann never be 0)
        /// </summary>
        public List<Point3d> ComputeColumnStartPosSlab()
        {
            List<Point3d> columnStartPos = new List<Point3d>();

            if (this.ColumnNumbers == 0)
            {
                RhinoApp.WriteLine("number of column Numbers must be greater than 0");
            }
            else
            {
                //Point3d[] points;
                double[] tParams = this.EnvironmentBoundary.DivideByCount(this.ColumnNumbers, true);
                foreach (var t in tParams)
                {
                    Point3d pt = this.EnvironmentBoundary.PointAt(t);
                    columnStartPos.Add(pt);
                }
            }
            return columnStartPos;
        }
        ///column numbers 
        ///environment boundary 
        ///initialize the class 
        ///condition for 0 columns 
        ///condition for same number as boundary vertices 
        ///condition to distribute the columns as equal intervals of the boundary 

    }
}
