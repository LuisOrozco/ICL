using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Karamba;
using Karamba.Geometry;
using Karamba.Nodes;
using Karamba.Models;
using KarambaCommon;
using Karamba.Results;
using Rhino;
using Rhino.Geometry;

namespace ICL.Core.StructuralAnalysis
{
    public class FEA
    {
        /// <summary>
        /// public variables 
        /// </summary>
        Model KarambaModel;
        List<Point3> Nodes = new List<Point3>();

        /// <summary>
        /// initialize attributes
        /// </summary>
        /// <Params> 
        /// nodes: list of Rhino.Geometry.Point3d
        /// karambaModel: Karamba.Models.Model
        public FEA(Model karambaModel, List<Point3> nodes)
        {
            this.KarambaModel = karambaModel;
            this.Nodes = nodes;
        }

        /// Method:0
        /// <summary>
        /// Method to compute nodal displacements
        /// </summary>
        /// <returns> List of Rhino.Geometry.Point3d </returns>
        public List<Point3d> ComputeNodalDisplacements()
        {
            List<Point3> nodalDisplacements = new List<Point3>();
            //Analysis 
            var k3d = new KarambaCommon.Toolkit();
            List<double> max_disp;
            List<double> out_g;
            List<double> out_comp;
            string message;
            this.KarambaModel = k3d.Algorithms.AnalyzeThI(this.KarambaModel, out max_disp, out out_g, out out_comp, out message);

            //NodalDisplacements
            var trans = new List<List<Vector3>>();
            var rotat = new List<List<Vector3>>();
            this.KarambaModel = this.KarambaModel.Clone();
            this.KarambaModel.cloneElements();
            string lc_ind = "0";
            List<int> ids = new List<int>();
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                ids.Add(i);
            }
            NodalDisp.solve(this.KarambaModel, lc_ind, ids, out trans, out rotat);
            List<Vector3d> dispVectors = ConvertVec3ToVec3d(trans);
            List<Point3d> vecToPt = ConvertVec3dToPoint3d(dispVectors);
            List<Point3d> rhNodes = ConvertPt3ToPt3d(this.Nodes);
            vecToPt = DecimalChange(vecToPt);
            List<Point3d> dispacementNodePoints = new List<Point3d>();
            for (int i = 0; i < rhNodes.Count; i++)
            {
                Point3d pt = new Point3d(rhNodes[i][0], rhNodes[i][1], vecToPt[i][2]);
                dispacementNodePoints.Add(pt);
            }
            return dispacementNodePoints;
        }

        /// Method:1
        /// <summary>
        /// Method for converting Karamba.Geomerty.Vector3 to Rhino.Geometry.Vector3d
        /// </summary>
        /// <Params> List or List of Karamba.Geometry.Vector3 </Params>
        /// <returns> List<Vector3d> </Line3></returns>
        public List<Vector3d> ConvertVec3ToVec3d(List<List<Vector3>> kVecs)
        {
            List<Vector3d> rhVecs = new List<Vector3d>();
            foreach (Vector3 v in kVecs[0])
            {
                Vector3d rhVec = new Vector3d(v[0], v[1], v[2]);
                rhVecs.Add(rhVec);
            }
            return rhVecs;
        }

        /// Method:2
        /// <summary>
        /// Method for converting Rhino.Geomerty.Vector3d to Rhino.Geometry.Point3d
        /// </summary>
        /// <Params> List of Karamba.Geometry.Vector3 </Params>
        /// <returns> List<Point3d> </Line3></returns>
        public List<Point3d> ConvertVec3dToPoint3d(List<Vector3d> rhVecs)
        {
            List<Point3d> points = new List<Point3d>();
            foreach (Vector3d v in rhVecs)
            {
                Point3d pt = new Point3d(v[0], v[1], v[2]);
                points.Add(pt);
            }
            return points;
        }

        /// Method:3
        /// <summary>
        /// Method for converting Karamba.Geometry.Point3 to Rhino.Geometry.Point3d
        /// </summary>
        /// <Params> List Karamba.Geometry.Point3 </Params>
        /// <returns> List<Point3d>  </Line3></returns>
        public List<Point3d> ConvertPt3ToPt3d(List<Point3> kPoints)
        {
            List<Point3d> points = new List<Point3d>();
            foreach (Point3 pt in kPoints)
            {
                Point3d p = new Point3d(pt[0], pt[1], pt[2]);
                points.Add(p);
            }
            return points;
        }

        /// Method:4
        /// <summary>
        /// Method to round or convert value by 7 decimal points
        /// </summary>
        /// <Params> List Rhino.Geometry.Point3d </Params>
        /// <returns> List<Point3d>  </Line3></returns>
        public List<Point3d> DecimalChange(List<Point3d> rhPoints)
        {
            List<Point3d> points = new List<Point3d>();
            foreach (Point3d pt in rhPoints)
            {
                double c = Math.Pow(10, 7);
                Point3d ptCorrection = new Point3d(pt[0], pt[1], pt[2] / c);
                points.Add(ptCorrection);
            }
            return points;
        }
    }
}
