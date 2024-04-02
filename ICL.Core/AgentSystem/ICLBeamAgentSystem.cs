using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Geometry.Delaunay;
using Grasshopper.Kernel.Geometry.Voronoi;

using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;

using ICL.Core.Environments;

using Rhino;
using Rhino.Geometry;
using System.Collections.Generic;

namespace ICL.Core.AgentSystem
{
    public class ICLBeamAgentSystem : AgentSystemBase
    {
        /// <summary>
        /// The list of Voronoi cells associated with each agent.
        /// </summary>
        public List<Cell2> VoronoiCells = new List<Cell2>();
        /// <summary>
        /// The connectivity diagram, i.e. interaction topology, of the agent system.
        /// </summary>
        public Connectivity diagram = null;
        /// <summary>
        /// The field to access the Cartesian environment of this agent system.
        /// </summary>
        public ICLBeamEnvironment CartesianEnvironment;
        /// <summary>
        /// Boolean toggle to determine if Voronoi diagram should be computed for this system.
        /// </summary>
        public bool ComputeVoronoiCells = false;
        /// <summary>
        /// Boolean toggle to determine if only the connectivity diagram should be computed for this system.
        /// </summary>
        public bool ComputeDelaunayConnectivity = false;

        /// <summary>
        /// Construct a new cartesian agent system
        /// </summary>
        public ICLBeamAgentSystem(List<CartesianAgent> agents, ICLBeamEnvironment cartesianEnvironment)
        {
            this.CartesianEnvironment = cartesianEnvironment;
            this.Agents = new List<AgentBase>();
            for (int i = 0; i < agents.Count; ++i)
            {
                //agents[i].Id = i;
                agents[i].AgentSystem = this;
                this.Agents.Add((AgentBase)agents[i]);
            }
            this.IndexCounter = agents.Count;
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override void PreExecute()
        {
            if (ComputeVoronoiCells)
            {
                Node2List nodes = new Node2List();
                foreach (CartesianAgent agent in this.Agents)
                    nodes.Append(new Node2(agent.Position.X, agent.Position.Y));
                diagram = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(nodes, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);
                List<Node2> node2List = new List<Node2>();
                foreach (Point3d boundaryCorner in this.CartesianEnvironment.EnvironmentBoundary)
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
