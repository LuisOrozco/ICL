using ABxM.Core.Agent;
using ABxM.Core.Behavior;
using ICL.Core.AgentSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICL.Core.Behavior
{
    public class RemoveAgentBehavior : BehaviorBase
    {
        /// <summary>
        /// The field that defines the distance within which agents are removed.
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

            // find topological neighbor agents
            List<CartesianAgent> neighborList = agentSystem.FindTopologicalNeighbors(cartesianAgent);

            foreach (CartesianAgent neighbor in neighborList)
            {
                // Randomly decide (with given probability) whether to create a new agent
                // This effectively makes the new agents being created gradually rather than all at once at the very first iteration
                if (random.NextDouble() > Probability) return;

                if (cartesianAgent.Position.DistanceTo(neighbor.Position) < Distance)
                {
                    agentSystem.RemoveAgent(cartesianAgent);
                    return;
                }
            }

        }
    }
}
