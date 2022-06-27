using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using ICL.Core.StructuralModelling;
using ICL.Core.StructuralAnalysis;
using Karamba.Geometry;
using Karamba.Supports;
using Karamba.Models;
using Karamba.CrossSections;
using Karamba.Elements;
using Karamba.Loads;



namespace ICL.Core.Environment
{
    public class BeamEnvironmentNodalDisplacement
    {
        ///dict of  nodal displacements 
        public Dictionary<int, Point3d> NodalDisplacement = new Dictionary<int, Point3d>();

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

        ///new instance of EnvironmentNodalNodalDisplacement
        public BeamEnvironmentNodalDisplacement(List<Point3d> agentPositions, List<Point3d> environmentBoundary, List<string> beamLoads, List<string> beamMaterial)
        {
            this.AgentStartPositons = this.AgentPositions = agentPositions;
            this.EnvironmentBoundary = environmentBoundary;
            this.BeamLoads = beamLoads;
            this.BeamMaterial = beamMaterial;
        }

        ///Method:0 reset the environment
        public void Reset()
        {
            //this.AgentPositions = this.AgentStartPositons;
            NodalDisplacement.Clear();
        }

        //Method1: PreExecute 
        public void PreExecute()
        {
            NodalDisplacement.Clear();
        }

        //Method2: Execute
        public List<List<Vector3>> Execute()
        {
            BeamFEM createBeamEnvironmentFEM = new BeamFEM(this.EnvironmentBoundary, this.AgentPositions, this.BeamLoads, this.BeamMaterial[0]);
            List<Point3> nodes = new List<Point3>();
            Model beamModelTest = createBeamEnvironmentFEM.ComputeFEM(ref nodes);
            FEA createBeamEnvironmentFEA = new FEA(beamModelTest, nodes);
            List<List<Vector3>> nodalDisp = createBeamEnvironmentFEA.ComputeNodalDisplacements();

            return nodalDisp;
        }

    }
}
