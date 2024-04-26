using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;
using ICL.Core.AgentSystem;
using Karamba.Elements;
using Karamba.Geometry;
using Karamba.GHopper.Geometry;
using Rhino.Geometry;
using Rhino;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Geometry.Delaunay;

namespace ICL.Core.Behavior
{
    public class SimpleAddBehavior : BehaviorBase
    {
        /// <summary>
        /// The field that defines the maximum displacement between agents before another is created.
        /// </summary>
        public double Displacement;

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
        /// <param name="displacement">The maximum allowable displacement.</param>
        /// <param name="probability">The probability that a new agent will be created</param>
        public SimpleAddBehavior(double weight, double displacement, double probability)
        {
            Weight = weight;
            Displacement = displacement;
            Probability = probability;
        }

        /// <summary>
        /// Method for executing the behavior's rule.
        /// </summary>
        /// <param name="agent">The agent that executes the behavior.</param>
        public override void Execute(AgentBase agent)
        {
            CartesianAgent cartesianAgent = (CartesianAgent)agent;
            ICLSlabAgentSystem agentSystem = (ICLSlabAgentSystem)(cartesianAgent.AgentSystem);
            Mesh mesh = ((Mesh3)((BuilderShell)agentSystem.ModelElements[0]).mesh).Convert();


            // Randomly decide (with probability 0.001) whether to create a new agent and request to add it to the system
            // This effectively makes the new agents being created gradually rather than all at once at the very first iteration
            if (random.NextDouble() > Probability || agentSystem.Agents.Count > 149) return;

            Point3d newAgentPosition = cartesianAgent.Position + new Vector3d(0.1, 0, 0); // new agent is located right next to the original agent

            double minDist = double.MaxValue;
            Point3d newMeshAgentPosition = Point3d.Unset;
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                double verDist = newAgentPosition.DistanceToSquared(mesh.Vertices[i]);
                if (verDist < minDist)
                {
                    minDist = verDist;
                    newMeshAgentPosition = mesh.Vertices[i];
                }

            }

            List<BehaviorBase> newAgentBehaviors = cartesianAgent.Behaviors; // the new agent has the same behaviors as other original agent
            CartesianAgent newAgent = new CartesianAgent(newMeshAgentPosition, newAgentBehaviors);

            agentSystem.AddAgent(newAgent);
        }

    }
}
