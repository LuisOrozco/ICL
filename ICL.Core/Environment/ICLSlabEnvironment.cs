using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

using ABxM.Core.Environments;

using ICL.Core.StructuralModelling;
using ICL.Core.StructuralAnalysis;

using Karamba.Models;
using Karamba.Loads;
using Karamba.Supports;
using Karamba.Materials;
using Karamba.Geometry;
using Karamba.CrossSections;
using Karamba.Elements;
using ABxM.Core.Agent;

namespace ICL.Core.Environment
{
    public class ICLSlabEnvironment : CartesianEnvironment
    {
        ///beam environment boundary points
        public Mesh3 EnvironmentMesh;

        ///beam environment loads
        public List<string> SlabLoads = new List<string>();

        ///beam environment material
        public List<string> SlabMaterial = new List<string>();

        public Model SlabModel;

        /// <summary>
        /// Dictionary for optional additional data at the start.
        /// </summary>
        public Dictionary<string, object> startCustomData = new Dictionary<string, object>();
        /// <summary>
        /// Dictionary for optional additional data at the execute step.
        /// </summary>
        public Dictionary<string, object> NewCustomData = new Dictionary<string, object>();

        ///displacements, as a dict<index, deltaZ> is in the CustomData

        public ICLSlabEnvironment(List<Point3d> BoundaryCorners, Mesh3 environmentMesh, List<string> slabLoads, List<string> slabMaterial, Dictionary<string, object> initialData) :base(BoundaryCorners)
        {
            this.EnvironmentMesh = environmentMesh;
            this.SlabLoads = slabLoads;
            this.SlabMaterial = slabMaterial;
            CustomData = new Dictionary<string, object>(initialData);  // this creates a shallow copy of the data dictionary, i.e., 
            // if the value of data is a reference type, then CustomData
            // will also update the value of data...
            startCustomData = new Dictionary<string, object>(initialData); // shallow copy of data
        }

        public void Reset()
        {
            CustomData = new Dictionary<string, object>(startCustomData);
            NewCustomData.Clear();
        }

        public void UpdateEnvironment()
        {
            this.Reset();
            NewCustomData = new Dictionary<string, object>(CustomData); // this is to make sure that in case we prematurely 
            // exit out of a behavior NewCustomData is not empty
            this.Execute();
        }

        public void Execute()
        {
            //create slab FEM class instance, which just contains a mesh and columns
            SlabFEM femModel = new SlabFEM(this.EnvironmentMesh, this.AgentSystems[0].Agents.OfType<CartesianAgent>().Select(agent => agent.Position).ToList());
            Mesh3 environmentMesh = this.EnvironmentMesh;
            List<Point3d> columnPositions = new List<Point3d>(AgentSystems[0].Agents.OfType<CartesianAgent>().Select(agent => agent.Position).ToList());
            //Create Karamba "Model"
            List<Point3> nodes = new List<Point3>(environmentMesh.Vertices);
            this.SlabModel = femModel.ComputeSlabFEM(ref nodes); //note model before analysis
            //Calculate Nodal Displacement
            FEA createSlabEnvironmentFEA = new FEA(this.SlabModel, nodes);

            List<double> nodalDispDist = createSlabEnvironmentFEA.ComputeNodalDisplacements();
            Dictionary<string, object> NodalDisplacement = new Dictionary<string, object>();

            for (int i = 0; i < nodalDispDist.Count; i++)
            {
                NodalDisplacement.Add(i.ToString(), nodalDispDist[i]);
            }
            CustomData = NodalDisplacement;


            //return createSlabEnvironmentFEA;
            //compute FEA
            //compute Nodal Displacements 
            //create beam nodes 
            //add values to nodaldisplacements dictionary 

        }
    }
}
