using System;
using System.Linq;
using System.Collections.Generic;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.Behavior;
using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core.Environments;
using Rhino.Geometry;
using Rhino;

namespace ICL.Core.AgentBehaviors
{
    public class MaxDisplacementBehavior : BehaviorBase
    {
        //public variables 
        public Dictionary<int, List<Point3d>> NodalDisplacemenets = new Dictionary<int, List<Point3d>>();
        public double SteppingFactor = 100; //in mm
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
            int agentPosNodeIndex;
            Dictionary<string, List<Point3d>> neighborNodes = FindNeightbors(columnAgent.Position, out agentPosNodeIndex);

            //get vector to move towards max displacement node
            if (neighborNodes.Count == 1)
            {
                //neighbor node - support node
                var item = neighborNodes.ElementAt(0);
                Point3d neighbourNode = item.Value[0];

                AddMoves(neighbourNode, columnAgent.Position, columnAgent);
            }
            else if (neighborNodes.Count > 1)
            {
                Point3d ancestorNode = neighborNodes["ancestor"][0];
                Point3d ancestorNodalDisp = neighborNodes["ancestor"][1];

                Point3d descendantNode = neighborNodes["descendant"][0];
                Point3d descendantNodalDisp = neighborNodes["descendant"][1];

                if (ancestorNodalDisp > descendantNodalDisp)
                {
                    AddMoves(ancestorNode, columnAgent.Position, columnAgent);
                    RhinoApp.WriteLine("ancestorNodalDisp");
                }
                else if (descendantNodalDisp > ancestorNodalDisp)
                {
                    AddMoves(descendantNode, columnAgent.Position, columnAgent);
                    RhinoApp.WriteLine("descendantNodalDisp");
                }
            }
        }

        /// Method:1
        /// <summary>
        /// performs vector operations and adds the scaled vector to the agent moves list
        /// is returned
        /// </summary>
        /// <Param> 
        /// Point3d: neighbour node 
        /// Point3d: node
        /// CartesianAgent: column agent
        /// </Param>
        public void AddMoves(Point3d neighbour, Point3d node, CartesianAgent agent)
        {
            Vector3d vec = neighbour - node;
            vec.Unitize();
            Vector3d moveVec = vec * this.SteppingFactor;
            agent.Moves.Add(moveVec);
            /// <summary>
            /// print check 
            /// </summary>
            RhinoApp.WriteLine(moveVec + "moveVec");
            RhinoApp.WriteLine(agent.Moves + "columnAgent.Moves");
        }

        /// Method:2
        /// <summary>
        /// Given Point3d ColumnAgent, the neighbouring node from the NodalDisplacements with the max nodal displacement
        /// is returned
        /// </summary>
        /// <Param> Rhino.Geometry.Point3d columnAgent position</Param>
        public Dictionary<string, List<Point3d>> FindNeightbors(Point3d agentPosition, out int agentPosNodeIndex)
        {
            Dictionary<string, List<Point3d>> neighborNodes = new Dictionary<string, List<Point3d>>();
            agentPosNodeIndex = 0;
            for (int i = 0; i < NodalDisplacemenets.Count; i++)
            {
                var item = NodalDisplacemenets.ElementAt(i);
                Point3d node = item.Value[0];
                Point3d nodalDisp = item.Value[1];
                if ((agentPosition == node) && (i != 0) && (i != NodalDisplacemenets.Count - 1))
                {
                    agentPosNodeIndex = i;
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
                    agentPosNodeIndex = i;
                    var itemDescendant = NodalDisplacemenets.ElementAt(i + 1);
                    Point3d nodeDescendant = itemDescendant.Value[0];
                    Point3d nodalDispDescendant = itemDescendant.Value[1];
                    neighborNodes.Add("descendant", new List<Point3d>() { nodeDescendant, nodalDispDescendant });
                }
                else if ((agentPosition == node) && (i == NodalDisplacemenets.Count - 1))
                {
                    agentPosNodeIndex = i;
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
