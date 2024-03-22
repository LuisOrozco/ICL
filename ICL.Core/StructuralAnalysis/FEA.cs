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
        public Model KarambaModel;
        public Model AnalysedModel;
        public List<Point3> Nodes = new List<Point3>();

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
        public List<double> ComputeNodalDisplacements()
        {
            //Analysis 
            var k3d = new Toolkit();
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
            List<int> ids = Enumerable.Range(0, this.Nodes.Count).ToList();
            NodalDisp.solve(this.KarambaModel, lc_ind, ids, out trans, out rotat);

            // Scale Displacement Vectors, and Convert resulting translations into Rhino Vector3d
            double c = Math.Pow(10, 7);
            List<Vector3d> dispVectors = trans[0].Select(v => new Vector3d(v.X, v.Y, v.Z / c)).ToList();

            // Compute the displacement distances
            List<double> displacementDistances = dispVectors.Select(vec => vec.Length).ToList();

            return displacementDistances;

        }

    }
}
