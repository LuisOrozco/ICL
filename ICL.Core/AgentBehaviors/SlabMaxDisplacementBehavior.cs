using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ABxM.Core.Behavior;
using ABxM.Core.Agent;
using ABxM.Core.Environments;

using ICL.Core.AgentSystem;
using ICL.Core.Environment;
using Rhino.Geometry;
using Rhino;
using Karamba.GHopper.Geometry;
using Karamba.Elements;
using Karamba.Geometry;

namespace ICL.Core.AgentBehaviors
{
    public class SlabMaxDisplacementBehavior : BehaviorBase
    {
        //public variables 
        public Dictionary<int, double> NodalDisplacements = new Dictionary<int, double>();
        public Dictionary<int, List<Point3d>> StartNodalDisplacemenets = new Dictionary<int, List<Point3d>>();
        public List<Point3d> VertexNeighbours = new List<Point3d>();
        public Mesh SlabGeo;

        public SlabMaxDisplacementBehavior()
        {
            Weight = 1.0;
        }
        // Method:0
        //Execute 
        public override void Execute(AgentBase agent)
        {
            CartesianAgent cartesianAgent = (CartesianAgent)agent;
            ICLSlabAgentSystem cartesianSystem = (ICLSlabAgentSystem)(cartesianAgent.AgentSystem);
            CartesianEnvironment cartesianEnvironment = (CartesianEnvironment)cartesianSystem.CartesianEnvironment;
            SlabGeo = ((Mesh3)((BuilderShell)cartesianSystem.ModelElements[0]).mesh).Convert();

            //get nodal displacements from the ICLcartesianEnvironment here 
            this.NodalDisplacements = cartesianEnvironment.CustomData.ToDictionary(kvp => int.Parse(kvp.Key), kvp => (double)kvp.Value);

            //// Find the index of the agent's position in the mesh's topology vertices
            // Convert Point3d to Point3f
            Point3f agentPositionF = new Point3f((float)cartesianAgent.Position.X, (float)cartesianAgent.Position.Y, (float)cartesianAgent.Position.Z);
            // Define a tolerance for comparing points
            float tolerance = 0.01f;
            // Find the index of the agent's position in the mesh's topology vertices
            int agentPositionIndex = -1;

            for (int i = 0; i < SlabGeo.TopologyVertices.Count; i++)
            {
                Point3f vertex = SlabGeo.TopologyVertices[i];
                if (vertex.DistanceTo(agentPositionF) < tolerance)
                {
                    agentPositionIndex = i;
                    break;
                }
            }

            if (agentPositionIndex == -1) return;

            // find mesh neighbors in topology
            int[] meshNeighborIndexes = SlabGeo.TopologyVertices.ConnectedTopologyVertices(agentPositionIndex);

            Point3d targetVertex = Point3d.Unset;
            double maxZ = double.MinValue;

            foreach (int meshNeighborIndex in meshNeighborIndexes)
            {
                double thisDisplacement = Math.Abs(NodalDisplacements[meshNeighborIndex]);
                if (thisDisplacement > maxZ)
                {
                    maxZ = thisDisplacement;
                    targetVertex = SlabGeo.Vertices[meshNeighborIndex];
                }
            }

            if (targetVertex != Point3d.Unset)
            {
                // Create a vector that points from the agent's current position to the target vertex
                Vector3d moveVec = targetVertex - cartesianAgent.Position;
                // Apply the moveVec to the agent's position
                cartesianAgent.Moves.Add(moveVec);
                cartesianAgent.Weights.Add(Weight);
            }


        }

    }
}
