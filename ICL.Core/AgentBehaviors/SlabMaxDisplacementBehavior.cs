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
                List<int> agentPosNodeIndex = new List<int>();
                neighborNodes = FindNeighborsSlab(columnAgent.Position, this.SlabGeo, StartNodalDisplacemenets, ref agentPosNodeIndex);
            }

            if (this.NodalDisplacemenets.Count > 0)
            {
                List<int> agentPosNodeIndex = new List<int>();
                neighborNodes = FindNeighborsSlab(columnAgent.Position, this.SlabGeo, this.NodalDisplacemenets, ref agentPosNodeIndex);
            }


        }
        //Method add moves 
        //search neighbours 
        public Dictionary<string, List<Point3d>> FindNeighborsSlab(Point3d agentPoistion, Mesh mesh, Dictionary<int, List<Point3d>> displacementsDict, ref List<int> agentPosInd)
        {
            List<int> agentPositionIndex = new List<int>();
            for (int i = 0; i < this.SlabGeo.TopologyVertices.Count; i++)
            {
                if (agentPoistion.EpsilonEquals(SlabGeo.TopologyVertices[i], 0.001))
                {
                    agentPositionIndex.Add(i);

                }
            }
            agentPosInd.Add(agentPositionIndex[0]);
            int[] neighborInd = mesh.TopologyVertices.ConnectedTopologyVertices(agentPositionIndex[0]);
            List<Point3d> connectedVertices = new List<Point3d>();
            foreach (int i in neighborInd)
            {
                Point3d pt = mesh.Vertices.Point3dAt(i);
                connectedVertices.Add(pt);
            }

            Dictionary<string, List<Point3d>> neighborNodes = new Dictionary<string, List<Point3d>>();
            for (int i = 0; i < connectedVertices.Count; i++)
            {
                foreach (var item in displacementsDict.Values)
                {
                    if (connectedVertices[i] == item[0])
                    {
                        //neighborNodalDisp.Add(item[1]);
                        neighborNodes.Add(i.ToString(), new List<Point3d>() { item[0], item[1] });
                    }
                }
            }

            return neighborNodes;
            //find vertext index from agent position 
            //find connected points
            //for each connected point find its respective nodal displaceemnt value 
            //return a list of lists?
        }
    }
}
