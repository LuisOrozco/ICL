using System;
using System.Linq;
using System.Collections.Generic;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core.Environments;

using ICL.Core.Agent;
using ICL.Core.AgentSystem;
using ICL.Core.Environment;
using Rhino.Geometry;
using Rhino;

namespace ICL.Core.AgentBehaviors
{
    public class MaxDisplacementBehavior : ICLbehaviorBase
    {
        //public variables 
        public Dictionary<int, List<Point3d>> NodalDisplacemenets = new Dictionary<int, List<Point3d>>();
        public double SteppingFactor = 100; //in mm

        /// Method:0
        /// <summary>
        /// Inherited method defines agent Max Displacement search behaviour
        /// </summary>
        public override void Execute(AgentBase agent)
        {
            ICLcartesianAgent columnAgent = (ICLcartesianAgent)agent;
            ICLcartesianAgentSystem cartesianSystem = (ICLcartesianAgentSystem)(columnAgent.AgentSystem);
            ICLcartesianEnvironment cartesianEnvironment = cartesianSystem.CartesianEnvironment;

            //get nodal displacements from the ICLcartesianEnvironment here 
            this.NodalDisplacemenets = cartesianEnvironment.NodalDisplacement; //will this make the first run empty?
            //identify agent's neighbour node with max displacement 
            //int agentPosNodeIndex;
            Dictionary<string, List<Point3d>> neighborNodes = FindNeightbors(columnAgent.Position, out int agentPosNodeIndex);

            //get vector to move towards max displacement node
            if (neighborNodes.Count == 1)
            {
                //neighbor node - support node
                var item = neighborNodes.ElementAt(0);
                Point3d neighbourNode = item.Value[0];

                AddMoves(neighbourNode, columnAgent.Position, columnAgent);
            }
            else if (neighborNodes.Count > 1) //Note this is only suitable for beams or elements with onlz 2 neighbours)reimplement for slab)
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
        public void AddMoves(Point3d neighbour, Point3d node, ICLcartesianAgent agent)
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
            for (int i = 0; i < this.NodalDisplacemenets.Count; i++) //get nodal displacements from environment
            {
                var item = this.NodalDisplacemenets.ElementAt(i);
                Point3d node = item.Value[0];
                Point3d nodalDisp = item.Value[1];
                if ((agentPosition == node) && (i != 0) && (i != this.NodalDisplacemenets.Count - 1))
                {
                    agentPosNodeIndex = i;
                    var itemAncestor = this.NodalDisplacemenets.ElementAt(i - 1);
                    Point3d nodeAncestor = itemAncestor.Value[0];
                    Point3d nodalDispAncestor = itemAncestor.Value[1];
                    neighborNodes.Add("ancestor", new List<Point3d>() { nodeAncestor, nodalDispAncestor });

                    var itemDescendant = this.NodalDisplacemenets.ElementAt(i + 1);
                    Point3d nodeDescendant = itemDescendant.Value[0];
                    Point3d nodalDispDescendant = itemDescendant.Value[1];
                    neighborNodes.Add("descendant", new List<Point3d>() { nodeDescendant, nodalDispDescendant });
                }
                else if ((agentPosition == node) && (i == 0))
                {
                    agentPosNodeIndex = i;
                    var itemDescendant = this.NodalDisplacemenets.ElementAt(i + 1);
                    Point3d nodeDescendant = itemDescendant.Value[0];
                    Point3d nodalDispDescendant = itemDescendant.Value[1];
                    neighborNodes.Add("descendant", new List<Point3d>() { nodeDescendant, nodalDispDescendant });
                }
                else if ((agentPosition == node) && (i == this.NodalDisplacemenets.Count - 1))
                {
                    agentPosNodeIndex = i;
                    var itemAncestor = this.NodalDisplacemenets.ElementAt(i - 1);
                    Point3d nodeAncestor = itemAncestor.Value[0];
                    Point3d nodalDispAncestor = itemAncestor.Value[1];
                    neighborNodes.Add("ancestor", new List<Point3d>() { nodeAncestor, nodalDispAncestor });
                }
            }
            return neighborNodes;
        }
    }
}
