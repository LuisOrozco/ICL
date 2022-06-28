using System;
using System.Linq;
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

        /// Method:0
        /// <summary>
        /// Inherited method defines agent Max Displacement search behaviour
        /// </summary>
        public override void Execute(AgentBase agent)
        {
            CartesianAgent columnAgent = (CartesianAgent)agent;
            CartesianAgentSystem cartesianSystem = (CartesianAgentSystem)(columnAgent.AgentSystem);
            CartesianEnvironment cartesianEnvironment = cartesianSystem.CartesianEnvironment;

            //identify agent's neighbour node with max displacement 

            //get vector to move towards max displacement node

            //move agent by magnitue of node divisions and direction of max displacement

            //throw new NotImplementedException();
        }


        /// Method:1
        /// <summary>
        /// Given Point3d ColumnAgent, the neighbouring node from the NodalDisplacements with the max nodal displacement
        /// is returned
        /// </summary>
        /// <Param> Rhino.Geometry.Point3d columnAgent position</Param>
        public Dictionary<string, List<Point3d>> FindNeightbors(Point3d agentPosition)
        {
            Dictionary<string, List<Point3d>> neighborNodes = new Dictionary<string, List<Point3d>>();
            for (int i = 0; i < NodalDisplacemenets.Count; i++)
            {
                var item = NodalDisplacemenets.ElementAt(i);
                Point3d node = item.Value[0];
                Point3d nodalDisp = item.Value[1];
                if ((agentPosition == node) && (i != 0) && (i != NodalDisplacemenets.Count - 1))
                {
                    var itemAncestor = NodalDisplacemenets.ElementAt(i - 1);
                    Point3d nodeAncestor = itemAncestor.Value[0];
                    Point3d nodalDispAncestor = itemAncestor.Value[1];
                    neighborNodes.Add("ancestor", new List<Point3d>() { nodeAncestor, nodalDispAncestor });

                    var itemDescendant = NodalDisplacemenets.ElementAt(i + 1);
                    Point3d nodeDescendant = itemDescendant.Value[0];
                    Point3d nodalDispDescendant = itemDescendant.Value[1];
                    neighborNodes.Add("descendant", new List<Point3d>() { nodeDescendant, nodalDispDescendant });
                }
                else if ((agentPosition == node) && (i == 0))
                {
                    var itemDescendant = NodalDisplacemenets.ElementAt(i + 1);
                    Point3d nodeDescendant = itemDescendant.Value[0];
                    Point3d nodalDispDescendant = itemDescendant.Value[1];
                    neighborNodes.Add("descendant", new List<Point3d>() { nodeDescendant, nodalDispDescendant });
                }
                else if ((agentPosition == node) && (i == NodalDisplacemenets.Count - 1))
                {
                    var itemAncestor = NodalDisplacemenets.ElementAt(i - 1);
                    Point3d nodeAncestor = itemAncestor.Value[0];
                    Point3d nodalDispAncestor = itemAncestor.Value[1];
                    neighborNodes.Add("ancestor", new List<Point3d>() { nodeAncestor, nodalDispAncestor });
                }
            }
            return neighborNodes;
        }
    }
}
