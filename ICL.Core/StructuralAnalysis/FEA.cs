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

namespace ICL.Core.StructuralAnalysis
{
    public class FEA
    {
        //public attributes
        Model KarambaModel;
        List<Point3> Nodes = new List<Point3>();
        //define attributes 
        public FEA(Model karambaModel, List<Point3> nodes)
        {
            this.KarambaModel = karambaModel;
            this.Nodes = nodes;
        }
        public List<List<Vector3>> ComputeNodalDisplacements()
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

            return trans;
        }
    }
}
