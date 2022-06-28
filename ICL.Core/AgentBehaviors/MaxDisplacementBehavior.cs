using System;
using System.Collections.Generic;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.Behavior;
using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core.Environments;
using Rhino.Geometry;

namespace ICL.Core.AgentBehaviors
{
    public class MaxDisplacementBehavior : BehaviorBase
    {
        //public variables 
        public Dictionary<int, List<Point3d>> NodalDisplacemenets = new Dictionary<int, List<Point3d>>();

        /// <summary>
        /// Constructs a new instance of the Boid cohesion behavior.
        /// </summary>
        public MaxDisplacementBehavior(Dictionary<int, List<Point3d>> nodalDisplacemenets)
        {
            this.NodalDisplacemenets = nodalDisplacemenets;
        }
        public override void Execute(AgentBase agent)
        {
            CartesianAgent columnAgent = (CartesianAgent)agent;
            CartesianAgentSystem cartesianSystem = (CartesianAgentSystem)(columnAgent.AgentSystem);
            CartesianEnvironment cartesianEnvironment = cartesianSystem.CartesianEnvironment;

            // define cartesian agent 
            // define cartesian agent system 
            // define cartesian environment 

            //identify agent's neighbour node with max displacement 
            //get vector to move towards max displacement node

            //move agent by magnitue of node divisions and direction of max displacement

            throw new NotImplementedException();
        }
    }
}
