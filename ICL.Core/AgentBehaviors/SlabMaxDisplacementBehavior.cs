using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICD.AbmFramework.Core.Behavior;
using ICD.AbmFramework.Core.Agent;

using ICL.Core.AgentSystem;
using ICL.Core.Environment;
using Rhino.Geometry;
using Rhino;

namespace ICL.Core.AgentBehaviors
{
    public class SlabMaxDisplacementBehavior : BehaviorBase
    {
        //public variables 
        public Dictionary<int, List<Point3d>> NodalDisplacemenets = new Dictionary<int, List<Point3d>>();
        public Dictionary<int, List<Point3d>> StartNodalDisplacemenets = new Dictionary<int, List<Point3d>>();
        public List<Point3d> VertexNeighbours = new List<Point3d>();
        public Mesh SlabGeo;

        public SlabMaxDisplacementBehavior(Mesh slabGeo)
        {
            this.SlabGeo = slabGeo;
        }
        // Method:0
        //Execute 
        public override void Execute(AgentBase agent)
        {
            CartesianAgent columnAgent = (CartesianAgent)agent;
            ICLcartesianAgentSystem cartesianSystem = (ICLcartesianAgentSystem)(columnAgent.AgentSystem);
            ICLcartesianEnvironment cartesianEnvironment = cartesianSystem.CartesianEnvironment;

            //get nodal displacements from the ICLcartesianEnvironment here 
            this.NodalDisplacemenets = cartesianEnvironment.NodalDisplacement;

            Dictionary<string, List<Point3d>> neighborNodes = new Dictionary<string, List<Point3d>>();


            if (this.NodalDisplacemenets.Count == 0)
            {
                //neighborNodes = FindNeightbors(columnAgent.Position, this.StartNodalDisplacemenets, out int agentPosNodeIndex);
            }

            if (this.NodalDisplacemenets.Count > 0)
            {
                //neighborNodes = FindNeightbors(columnAgent.Position, this.StartNodalDisplacemenets, out int agentPosNodeIndex);
            }


        }
        //Method add moves 
        //search neighbours 
        public void FindNeighborsSlab(Point3d agentPoistion)
        {
            int agentPositionIndex;
            for (int i = 0; i < this.SlabGeo.TopologyVertices.Count; i++)
            {
                if (agentPoistion.EpsilonEquals(SlabGeo.TopologyVertices[i], 0.001))
                {
                    agentPositionIndex = i;
                }
            }
            //find vertext index from agent position 
            //find connected points
            //for each connected point find its respective nodal displaceemnt value 
            //return a list of lists?
        }
    }
}
