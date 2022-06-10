using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace ICL.Core.Environment
{
    public class BeamEnvironmentNodalDisplacement
    {
        ///dict of  nodal displacements 
        public Dictionary<double, Point3d> NodalDisplacement = new Dictionary<double, Point3d>();

        ///list of positions of the agent 
        public List<Point3d> AgentPositions = new List<Point3d>();

        ///list of start positions of the agents 
        public List<Point3d> AgentStartPositons = new List<Point3d>();

        ///beam environment boundary points
        public List<Point3d> EnvironmentBoundary = new List<Point3d>();

        ///new instance of EnvironmentNodalNodalDisplacement
        public BeamEnvironmentNodalDisplacement(List<Point3d> agentPositions, List<Point3d> environmentBoundary)
        {
            this.AgentStartPositons = this.AgentPositions = agentPositions;
            this.EnvironmentBoundary = environmentBoundary;
        }


    }
}
