using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

using ABxM.Core.AgentSystem;
using ABxM.Core;
using ABxM.Core.Agent;

using ICL.Core.Environments;
using ICL.Core.Behavior;
//using ABxM.Core.Solver;

using ICL.Core.AgentSystem;


namespace ICL.Core
{
    public class SlabSolver : Solver
    {

        /// <summary>  solver with display conduit</summary>
        //public List<Point3d> tempDisp = new List<Point3d>();

        public SlabSolver(List<AgentSystemBase> agentSystems) : base(agentSystems)
        {
        }

        public void ICLslabSolverExecute()
        {
            IterationCount++;

            //Check update
            //foreach (ICLSlabAgentSystem agentSystem in this.AgentSystems)
            //{

            //    Dictionary<int, List<Point3d>> nodalDisp = agentSystem.CartesianEnvironment.NodalDisplacement;
            //    List<double> displacementsZ = new List<double>();
            //    foreach (List<Point3d> pts in nodalDisp.Values)
            //    {
            //        displacementsZ.Add(pts[1][2]);
            //        //(pts[1][2] + "RESTART_NODALDISP");
            //    }
            //}
            //PreExecute===============================================================================================
            foreach (AgentSystemBase agentSystem in AgentSystems)
                if (!agentSystem.IsFinished()) agentSystem.PreExecute();

            //Execute===============================================================================================
            foreach (AgentSystemBase agentSystem in AgentSystems)
                if (!agentSystem.IsFinished()) agentSystem.Execute();

            //PostExecute===============================================================================================
            foreach (AgentSystemBase agentSystem in AgentSystems)
                if (!agentSystem.IsFinished()) agentSystem.PostExecute();

            ///Environment Update===============================================================================================
            foreach (ICLSlabAgentSystem agentSystem in AgentSystems)
            {
                if (!agentSystem.IsFinished())
                {
                    //List<Point3d> updatedAgentPositions = agentSystem.Agents.OfType<CartesianAgent>().Select(agent => agent.Position).ToList();
                    ((ICLSlabEnvironment)agentSystem.CartesianEnvironment).UpdateEnvironment();

                    // set "temporary display" points
                    //Dictionary<int, List<Point3d>> testupdate = agentSystem.CartesianEnvironment.NodalDisplacement;
                    //foreach (List<Point3d> points in testupdate.Values)
                    //{
                    //    this.tempDisp.Add(points[1]);
                    //}
                }
            }
            ///Print Test===============================================================================================
            //foreach (ICLSlabAgentSystem agentSystem in this.AgentSystems)
            //{
            //    foreach (CartesianAgent pos in agentSystem.Agents)
            //    {
            //        //(pos.Position + "RESET_POSITION");
            //    }

            //    Dictionary<int, List<Point3d>> nodalDisp = agentSystem.CartesianEnvironment.NodalDisplacement;
            //    List<double> displacementsZ = new List<double>();
            //    foreach (List<Point3d> pts in nodalDisp.Values)
            //    {
            //        displacementsZ.Add(pts[1][2]);
            //        //(pts[1][2] + "RESET_NODALDISP");
            //    }
            //}
        }
    }
}

