using ABxM.Core.Agent;
using ABxM.Core.Behavior;
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
        /// Consructs a new instance of the remove agent behavior.
        /// </summary>
        /// <param name="weight">The behavior's weight.</param>
        /// <param name="distance">The distance within which agents are removed.</param>
        public RemoveAgentBehavior(double weight, double distance)
        {
            Weight = weight;
            Distance = distance;
        }

        /// <summary>
        /// Method for executing the behavior's rule.
        /// </summary>
        /// <param name="agent">The agent that executes the behavior.</param>
        public override void Execute(AgentBase agent)
        {
            throw new NotImplementedException();
        }
    }
}
