using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

using ICD.AbmFramework.Core.Environments;

using ICL.Core.StructuralModelling;

using Karamba.Models;
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
        public Mesh EnvironmentBoundary;

        ///beam environment loads
        public List<string> SlabLoads = new List<string>();

        ///beam environment material
        public List<string> SlabMaterial = new List<string>();

        public Model SlabModel;

        public ICLslabCartesianEnvironment(List<Point3d> agentPositions, Mesh environmentBoundary, List<string> beamLoads, List<string> beamMaterial)
        {
            this.AgentPositions = this.AgentStartPositons = agentPositions;
            this.EnvironmentBoundary = environmentBoundary;
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

        public List<BuilderBeam> Execute()
        {
            //create slab FEM class instance
            SlabFEM femModel = new SlabFEM(this.EnvironmentBoundary, this.AgentPositions);
            //compute FEM
            List<Point3d> nodes = new List<Point3d>();
            List<BuilderBeam> testMesh = femModel.ComputeSlabFEM(ref nodes);

            return testMesh;
            //compute FEA
            //compute Nodal Displacements 
            //create beam nodes 
            //add values to nodaldisplacements dictionary 
        }
    }
}
