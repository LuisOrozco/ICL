using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core.Agent;

using ICL.Core.Environment;
using ICL.Core.AgentBehaviors;
using ICL.Core.AgentSystem;
using ICL.Core.Agent;


namespace ICL.Core.Solver
{
    public class BeamSolver
    {
        // public attributes 
        public List<ICLagentSystemBase> AgentSystems;

        public int IterationCount = 0;

        //public ICLcartesianEnvironment BeamStructuralData;

        // Method for starting solver from a new list
        public BeamSolver()
        {
            this.AgentSystems = new List<ICLagentSystemBase>();
        }

        ///Method0:
        ///<summary>
        ///Method for starting solver from an exisiting list of agents
        ///</summary>
        public BeamSolver(List<ICLagentSystemBase> agentSystems)
        {
            this.AgentSystems = agentSystems;

            foreach (ICLagentSystemBase agentSystem in this.AgentSystems)
            {
                foreach (ICLagentBase agent in agentSystem.Agents)
                {
                    foreach (ICLbehaviorBase behavior in agent.ICLbehaviors)
                    {
                        behavior.BeamSolver = this;
                    }
                }
            }
        }

        /// <summary>  solver with display conduit</summary>
        public void ExecuteSingleStep()
        {
            //IterationCount++;

            foreach (ICLagentSystemBase agentSystem in this.AgentSystems)
            {
                agentSystem.ICLPreExecute();
            }
            //    the parent class classes ABM methods not ICL.rewrite this part as well


            //    foreach (ICLagentSystemBase agentSystem in AgentSystems)
            //        if (!agentSystem.IsFinished()) agentSystem.Execute();

            //    foreach (ICLagentSystemBase agentSystem in AgentSystems)
            //        if (!agentSystem.IsFinished()) agentSystem.PostExecute();

            //    foreach (ICLcartesianAgentSystem agentSystem in AgentSystems)
            //    {
            //        if (!agentSystem.IsFinished())
            //        {
            //            List<Point3d> updatedAgentPositions = UpdatedPositions(agentSystem);
            //            agentSystem.CartesianEnvironment.AgentStartPositons = updatedAgentPositions;
            //            agentSystem.CartesianEnvironment.UpdateEnvironment();
            //        }
        }

    }

    public List<object> GetDisplayGeometries()
    {
        List<object> displayGeometries = new List<object>();
        foreach (ICLagentSystemBase agentSystem in AgentSystems)
            displayGeometries.AddRange(agentSystem.GetDisplayGeometries());
        return displayGeometries;
    }

    public List<Point3d> UpdatedPositions(ICLagentSystemBase agentSystem)
    {
        List<Point3d> agentPositionsUpdate = new List<Point3d>();

        foreach (ICLcartesianAgent agent in agentSystem.Agents)
        {
            Point3d point = agent.Position;
            agentPositionsUpdate.Add(point);
        }
        return agentPositionsUpdate;
    }

}
}
