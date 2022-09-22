using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

using ICD.AbmFramework.Core.Environments;

using ICL.Core.StructuralModelling;
using ICL.Core.StructuralAnalysis;

using Karamba.Models;
using Karamba.Loads;
using Karamba.Supports;
using Karamba.Materials;
using Karamba.Geometry;
using Karamba.CrossSections;
using Karamba.Elements;

namespace ICL.Core.Environment
{
    public class ICLslabCartesianEnvironment : EnvironmentBase
    {
        ///dict of displacements 
        public Dictionary<int, List<Point3d>> NodalDisplacement = new Dictionary<int, List<Point3d>>();

        ///list of agent positions
        public List<Point3d> AgentPositions = new List<Point3d>();

        ///list of agent start positions
        public List<Point3d> AgentStartPositons = new List<Point3d>();

        ///beam environment boundary points
        public Mesh3 EnvironmentMesh;

        ///beam environment boundary points
        public List<Point3d> EnvironmentBoundaryPoints = new List<Point3d>();
        ///beam environment loads
        public List<string> SlabLoads = new List<string>();

        ///beam environment material
        public List<string> SlabMaterial = new List<string>();

        public Model SlabModel;

        public ICLslabCartesianEnvironment(List<Point3d> agentPositions, List<Point3d> environmentBoundaryPoints, Mesh3 environmentMesh, List<string> beamLoads, List<string> beamMaterial)
        {
            this.AgentPositions = this.AgentStartPositons = agentPositions;
            this.EnvironmentBoundaryPoints = environmentBoundaryPoints;
            this.EnvironmentMesh = environmentMesh;
            this.SlabLoads = beamLoads;
            this.SlabMaterial = beamMaterial;
        }

        public void Reset()
        {
            NodalDisplacement.Clear();
        }

        public void PreExecute()
        {
            this.Reset();
        }

        public void UpdateEnvironment()
        {
            this.PreExecute();
        }

        public void Execute()
        {
            //create slab FEM class instance
            SlabFEM femModel = new SlabFEM(this.EnvironmentMesh, this.AgentPositions);
            //compute FEM
            List<Point3> nodes = new List<Point3>();
            this.SlabModel = femModel.ComputeSlabFEM(ref nodes); //note model before analysis
            FEA createSlabEnvironmentFEA = new FEA(this.SlabModel, nodes);

            List<Point3d> nodalDisp = createSlabEnvironmentFEA.ComputeNodalDisplacements();
            List<Point3d> rhNodes = createSlabEnvironmentFEA.ConvertPt3ToPt3d(nodes);

            for (int i = 0; i < nodalDisp.Count; i++)
            {
                NodalDisplacement.Add(i, new List<Point3d>() { rhNodes[i], nodalDisp[i] });
            }

            //return createSlabEnvironmentFEA;
            //compute FEA
            //compute Nodal Displacements 
            //create beam nodes 
            //add values to nodaldisplacements dictionary 

        }
    }
}
