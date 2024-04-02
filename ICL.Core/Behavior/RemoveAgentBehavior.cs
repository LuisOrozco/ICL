using ABxM.Core.Agent;
using ABxM.Core.Behavior;
using ICL.Core.AgentSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino;


namespace ICL.Core.Behavior
{
    public class RemoveAgentBehavior : BehaviorBase
    {
        /// <summary>
        /// The field that defines the minimum distance between two agents, within which agents are removed.
        /// </summary>
        public double Distance;

        /// <summary>
        /// The probability that a new agent will be created.
        /// </summary>
        public double Probability;

        /// <summary>
        /// Random item that allows for agents to be created gradually.
        /// </summary>
        private static Random random = new Random();

        /// <summary>
        /// Consructs a new instance of the remove agent behavior.
        /// </summary>
        /// <param name="weight">The behavior's weight.</param>
        /// <param name="distance">The distance within which agents are removed.</param>
        public RemoveAgentBehavior(double weight, double distance, double probability)
        {
            Weight = weight;
            Distance = distance;
            Probability = probability;
        }

        /// <summary>
        /// Method for executing the behavior's rule.
        /// </summary>
        /// <param name="agent">The agent that executes the behavior.</param>
        public override void Execute(AgentBase agent)
        {
            CartesianAgent cartesianAgent = agent as CartesianAgent;
            ICLSlabAgentSystem agentSystem = (ICLSlabAgentSystem)(cartesianAgent.AgentSystem);
            Mesh agentDelaunay = agentSystem.DelaunayMesh;
            double distSquared = Distance * Distance;

            // find the agent delaunay vertex Id of my agent
            int agentId = -1;
            for (int i = 0; i < agentDelaunay.Vertices.Count; i++)
            {
                if (agentDelaunay.Vertices[i].Equals(cartesianAgent.Position))
                {
                    agentId = i;
                    break;
                }
            }
            if (agentId == -1) { return; }
            // find topological neighbor agents
            int[] neighborIndices = agentSystem.DelaunayMesh.TopologyVertices.ConnectedTopologyVertices(agentId);

            foreach (int neighborIndex in neighborIndices)
            {
                // Randomly decide (with given probability) whether to create a new agent
                // This effectively makes the new agents being created gradually rather than all at once at the very first iteration
                // if (random.NextDouble() > Probability) return;
                Point3f neighborF = agentSystem.DelaunayMesh.Vertices[neighborIndex];
                Point3d neighborD = new Point3d(neighborF);
                if (cartesianAgent.Position.DistanceTo(neighborD) < Distance)
                {
                    CartesianAgent neighbor = null;
                    foreach (CartesianAgent otherAgent in agentSystem.Agents)
                    {
                        if (otherAgent.Position == neighborD)
                        {
                            neighbor = otherAgent;
                            break;
                        }
                    }
                    if (neighbor == null) continue;

                    if (cartesianAgent.Id < neighbor.Id)
                    {
                        agentSystem.RemoveAgentList.Add(cartesianAgent);
                    }
                    else
                    {
                        agentSystem.RemoveAgentList.Add(neighbor);
                    }
                    return;
                }
            }

        }
    }
}
