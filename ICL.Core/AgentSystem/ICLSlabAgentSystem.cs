using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;

using Rhino.Collections;
using Rhino.DocObjects;
using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Geometry.Delaunay;
using Grasshopper.Kernel.Geometry.Voronoi;
using GH_IO;
using GH_IO.Serialization;

using ABxM.Core;
using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;
using ABxM.Core.Utilities;

using ICL.Core;
using ICL.Core.AgentBehaviors;
using ICL.Core.AgentSystem;
using ICL.Core.Environment;
using ICL.Core.ICLsolver;
using ICL.Core.StructuralAnalysis;
using ICL.Core.StructuralModelling;
using ICL.Core.Utilities;

using Karamba;
using Karamba.CrossSections;
using Karamba.Elements;
using Karamba.Geometry;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Models;
using Karamba.Supports;
using Karamba.Utilities;
using KarambaCommon;

namespace ICL.Core.AgentSystem
{
    public class ICLSlabAgentSystem : CartesianAgentSystem
    {


        public ICLSlabAgentSystem(List<CartesianAgent> agents, CartesianEnvironment cartesianEnvironment) : base(agents, cartesianEnvironment)
        {
        }

        /// <summary>
        /// Construct a new cartesian agent system
        /// </summary>
        //public ICLSlabAgentSystem(List<CartesianAgent> agents, ICLSlabEnvironment cartesianEnvironment):base(agents, cartesianEnvironment)
        //{
        //    this.CartesianEnvironment = cartesianEnvironment;
        //    this.Agents = new List<AgentBase>();
        //    for (int i = 0; i < agents.Count; ++i)
        //    {
        //        //agents[i].Id = i;
        //        agents[i].AgentSystem = this;
        //        this.Agents.Add((AgentBase)agents[i]);
        //    }
        //    this.IndexCounter = agents.Count;
        //}

        public override void PreExecute()
        {
            if (ComputeVoronoiCells)
            {
                Node2List nodes = new Node2List();
                foreach (CartesianAgent agent in this.Agents)
                    nodes.Append(new Node2(agent.Position.X, agent.Position.Y));
                diagram = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(nodes, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);
                List<Node2> node2List = new List<Node2>();
                foreach (Point3d boundaryCorner in this.CartesianEnvironment.BoundaryCorners)
                    node2List.Add(new Node2(boundaryCorner.X, boundaryCorner.Y));
                this.VoronoiCells = Grasshopper.Kernel.Geometry.Voronoi.Solver.Solve_Connectivity(nodes, diagram, (IEnumerable<Node2>)node2List);
            }

            if (ComputeDelaunayConnectivity)
            {
                Node2List nodes = new Node2List();
                foreach (CartesianAgent agent in this.Agents)
                    nodes.Append(new Node2(agent.Position.X, agent.Position.Y));
                diagram = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(nodes, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);
            }

            foreach (AgentBase agent in this.Agents)
                agent.PreExecute();

        }

        /// <summary>
        /// Method for collecting the geometry that should be displayed.
        /// </summary>+
        /// <returns>Returns null. Needs to be overridden.</returns>
        public override List<object> GetDisplayGeometries()
        {
            return new List<object>();
        }

        /// <summary>
        /// Find all agents that are within the given straight-line distance of the given agent
        /// </summary>
        /// <param> The agent to search from.</param>
        /// <param> The search distance.</param>
        /// <returns>Returns a list containing all neighboring agents within the search distance.</returns>
        public List<CartesianAgent> FindNeighbors(CartesianAgent agent, double distance)
        {
            List<CartesianAgent> cartesianAgentList = new List<CartesianAgent>();
            foreach (CartesianAgent otherAgent in this.Agents)
            {
                if (agent != otherAgent && agent.Position.DistanceTo(otherAgent.Position) < distance)
                    cartesianAgentList.Add(otherAgent);
            }

            return cartesianAgentList;
        }

        /// <summary>
        /// Find all agents that are topologically connected to a given agent
        /// </summary>
        /// <param>The agent to search from.</param>
        /// <returns>Returns the list of topologically connected neighboring agents.</returns>
        public List<CartesianAgent> FindTopologicalNeighbors(CartesianAgent agent)
        {
            List<CartesianAgent> cartesianAgentList = new List<CartesianAgent>();

            List<int> connections = diagram.GetConnections(agent.Id);

            foreach (int index in connections)
            {
                cartesianAgentList.Add((CartesianAgent)(this.Agents[index]));
            }
            return cartesianAgentList;
        }

    }
}
