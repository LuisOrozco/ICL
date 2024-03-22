﻿using System;
using System.Linq;
using System.Collections.Generic;

using ABxM.Core.Agent;
using ABxM.Core.Behavior;
using ABxM.Core.AgentSystem;

using ICL.Core.AgentSystem;
using ICL.Core.Environment;
using Rhino.Geometry;
using Rhino;

namespace ICL.Core.AgentBehaviors
{
    public class MaxDisplacementBehavior : BehaviorBase
    {
        //public variables 
        public Dictionary<int, double> NodalDisplacemenets = new Dictionary<int, double>();
        public Dictionary<int, List<Point3d>> StartNodalDisplacemenets = new Dictionary<int, List<Point3d>>();
        public double SteppingFactor = 5; //in mm
        public Point3d AncestorNodalDisp;
        public Point3d DescendantNodalDisp;

        /// Method:0
        /// <summary>
        /// Inherited method defines agent Max Displacement search behaviour
        /// </summary>
        public override void Execute(AgentBase agent)
        {
            CartesianAgent columnAgent = (CartesianAgent)agent;
            ICLcartesianAgentSystem cartesianSystem = (ICLcartesianAgentSystem)(columnAgent.AgentSystem);
            ICLBeamEnvironment cartesianEnvironment = cartesianSystem.CartesianEnvironment;

            //get nodal displacements from the ICLcartesianEnvironment here 
            this.NodalDisplacemenets = cartesianEnvironment.NodalDisplacement; //will this make the first run empty?
            //identify agent's neighbour node with max displacement 
            Dictionary<string, List<Point3d>> neighborNodes = new Dictionary<string, List<Point3d>>();
            //check to pass startdict or dict
            neighborNodes = FindNeightbors(columnAgent.Position, this.NodalDisplacemenets, out int agentPosNodeIndex);

            if (this.NodalDisplacemenets.Count == 0)
            {
                neighborNodes = FindNeightbors(columnAgent.Position, this.StartNodalDisplacemenets, out int agentPosNodeIndex);
            }

            else if (this.NodalDisplacemenets.Count > 0)
            {
                neighborNodes = FindNeightbors(columnAgent.Position, this.NodalDisplacemenets, out int agentPosNodeIndex);
            }

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
                this.AncestorNodalDisp = neighborNodes["ancestor"][1];

                Point3d descendantNode = neighborNodes["descendant"][0];
                this.DescendantNodalDisp = neighborNodes["descendant"][1];

                if (Math.Abs(this.AncestorNodalDisp.Z) > Math.Abs(this.DescendantNodalDisp.Z))
                {
                    AddMoves(ancestorNode, columnAgent.Position, columnAgent);
                }
                else if (Math.Abs(this.DescendantNodalDisp.Z) > Math.Abs((this.AncestorNodalDisp.Z)))
                {
                    AddMoves(descendantNode, columnAgent.Position, columnAgent);
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
            double weight = 2; //make it parametric
            agent.Weights.Add(weight);
            /// <summary>
            /// print check 
            /// </summary>
            //(moveVec + "moveVec");
            //(agent.Moves + "columnAgent.Moves");
        }

        /// Method:2
        /// <summary>
        /// Given Point3d ColumnAgent, the neighbouring node from the NodalDisplacements with the max nodal displacement
        /// is returned
        /// </summary>
        /// <Param> Rhino.Geometry.Point3d columnAgent position</Param>
        public Dictionary<string, List<Point3d>> FindNeightbors(Point3d agentPosition, Dictionary<int, List<Point3d>> displacementsDict, out int agentPosNodeIndex)
        {
            Dictionary<string, List<Point3d>> neighborNodes = new Dictionary<string, List<Point3d>>();
            agentPosNodeIndex = 0;
            for (int i = 0; i < displacementsDict.Count; i++) //get nodal displacements from environment
            {
                var item = displacementsDict.ElementAt(i);
                Point3d node = item.Value[0];
                Point3d nodalDisp = item.Value[1];
                if ((agentPosition == node) && (i != 0) && (i != displacementsDict.Count - 1))
                {
                    agentPosNodeIndex = i;
                    var itemAncestor = displacementsDict.ElementAt(i - 1);
                    Point3d nodeAncestor = itemAncestor.Value[0];
                    Point3d nodalDispAncestor = itemAncestor.Value[1];
                    neighborNodes.Add("ancestor", new List<Point3d>() { nodeAncestor, nodalDispAncestor });

                    var itemDescendant = displacementsDict.ElementAt(i + 1);
                    Point3d nodeDescendant = itemDescendant.Value[0];
                    Point3d nodalDispDescendant = itemDescendant.Value[1];
                    neighborNodes.Add("descendant", new List<Point3d>() { nodeDescendant, nodalDispDescendant });
                }
                else if ((agentPosition == node) && (i == 0))
                {
                    agentPosNodeIndex = i;
                    var itemDescendant = displacementsDict.ElementAt(i + 1);
                    Point3d nodeDescendant = itemDescendant.Value[0];
                    Point3d nodalDispDescendant = itemDescendant.Value[1];
                    neighborNodes.Add("descendant", new List<Point3d>() { nodeDescendant, nodalDispDescendant });
                }
                else if ((agentPosition == node) && (i == displacementsDict.Count - 1))
                {
                    agentPosNodeIndex = i;
                    var itemAncestor = displacementsDict.ElementAt(i - 1);
                    Point3d nodeAncestor = itemAncestor.Value[0];
                    Point3d nodalDispAncestor = itemAncestor.Value[1];
                    neighborNodes.Add("ancestor", new List<Point3d>() { nodeAncestor, nodalDispAncestor });
                }
            }
            return neighborNodes;
        }
    }
}
