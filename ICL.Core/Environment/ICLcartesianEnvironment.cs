using System.Collections.Generic;
using Rhino.Geometry;
using ICL.Core.StructuralModelling;
using ICL.Core.StructuralAnalysis;
using Karamba.Geometry;
using Karamba.Models;

using ICD.AbmFramework.Core.Environments;
namespace ICL.Core.Environment
{
    public class ICLcartesianEnvironment : EnvironmentBase
    {
        ///dict of  nodal displacements 
        public Dictionary<int, List<Point3d>> NodalDisplacement = new Dictionary<int, List<Point3d>>();

        ///list of positions of the agent 
        public List<Point3d> AgentPositions = new List<Point3d>();

        ///list of start positions of the agents 
        public List<Point3d> AgentStartPositons = new List<Point3d>();

        ///beam environment boundary points
        public List<Point3d> EnvironmentBoundary = new List<Point3d>();

        ///beam environment loads
        public List<string> BeamLoads = new List<string>();

        ///beam environment material
        public List<string> BeamMaterial = new List<string>();

        /// <summary>
        /// Method for initializing global attributes
        /// </summary>
        /// <Params> 
        /// agentPositions: list of Karamba.Geometry.Point3
        /// environmentBoundary: list of Rhino.Geometry.Point3d
        /// beamLoads: list of string 
        /// beamMaterial: List of string</Params>
        public ICLcartesianEnvironment(List<Point3d> agentPositions, List<Point3d> environmentBoundary, List<string> beamLoads, List<string> beamMaterial)
        {
            this.AgentStartPositons = this.AgentPositions = agentPositions;
            this.EnvironmentBoundary = environmentBoundary;
            this.BeamLoads = beamLoads;
            this.BeamMaterial = beamMaterial;
        }

        /// Method:0 reset the environment
        /// <summary>
        /// Method to Reset NodalDisplacements Dictionary
        /// </summary>
        public void Reset()
        {
            //this.AgentPositions = this.AgentStartPositons;
            NodalDisplacement.Clear();
        }

        /// Method1: PreExecute 
        /// <summary>
        /// Method to clear NodalDisplacements Dictionary
        /// </summary>
        public void PreExecute()
        {
            NodalDisplacement.Clear();
        }

        //Method2: update
        /// Method1: PreExecute 
        /// <summary>
        /// Method to Update Environment nodal displacements
        /// </summary>
        public void UpdateEnvironment()
        {
            this.Reset();
            this.Execute();
            //call execute
        }

        //Method3: Execute
        /// <summary>
        /// Method to compute FEM and FEA of Bema. Updates NodalDisplacement Dict 
        /// </summary>
        public void Execute()
        {
            BeamFEM createBeamEnvironmentFEM = new BeamFEM(this.EnvironmentBoundary, this.AgentPositions, this.BeamLoads, this.BeamMaterial[0]);
            List<Point3> nodes = new List<Point3>();
            Model beamModelTest = createBeamEnvironmentFEM.ComputeFEM(ref nodes);
            FEA createBeamEnvironmentFEA = new FEA(beamModelTest, nodes);
            List<Point3d> nodalDisp = createBeamEnvironmentFEA.ComputeNodalDisplacements();
            List<Point3d> rhNodes = createBeamEnvironmentFEA.ConvertPt3ToPt3d(nodes);

            for (int i = 0; i < nodalDisp.Count; i++)
            {
                NodalDisplacement.Add(i, new List<Point3d>() { rhNodes[i], nodalDisp[i] });
            }
        }

    }
}
