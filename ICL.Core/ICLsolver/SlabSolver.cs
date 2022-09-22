using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

using ICD.AbmFramework.Core.AgentSystem;
using ICD.AbmFramework.Core;
using ICD.AbmFramework.Core.Agent;

using ICL.Core.AgentBehaviors;
//using ICD.AbmFramework.Core.Solver;

using ICL.Core.AgentSystem;


namespace ICL.Core.ICLsolver
{
    public class SlabSolver : Solver
    {

        /// <summary>  solver with display conduit</summary>
        public List<Point3d> tempDisp = new List<Point3d>();

        public SlabSolver(List<AgentSystemBase> agentSystems) : base(agentSystems)
        {
        }

        public void ICLslabSolverExecute()
        {
            IterationCount++;

            //Check update
            foreach (ICLslabCartesianAgentSystem agentSystem in this.AgentSystems)
            {
                foreach (CartesianAgent pos in agentSystem.Agents)
                {
                    RhinoApp.WriteLine(pos.Position + "RESTART_POSITION");
                }

                Dictionary<int, List<Point3d>> nodalDisp = agentSystem.CartesianEnvironment.NodalDisplacement;
                List<double> displacementsZ = new List<double>();
                foreach (List<Point3d> pts in nodalDisp.Values)
                {
                    displacementsZ.Add(pts[1][2]);
                    RhinoApp.WriteLine(pts[1][2] + "RESTART_NODALDISP");
                }
            }
            //PreExecute===============================================================================================
            foreach (AgentSystemBase agentSystem in this.AgentSystems)
                if (!agentSystem.IsFinished()) agentSystem.PreExecute();

            //Execute===============================================================================================
            foreach (AgentSystemBase agentSystem in AgentSystems)
            {
                if (!agentSystem.IsFinished())
                {
                    agentSystem.Execute();
                }
            }
            //PostExecute===============================================================================================
            foreach (AgentSystemBase agentSystem in AgentSystems)
            {
                if (!agentSystem.IsFinished())
                {
                    agentSystem.PostExecute();
                }
            }
            ///Environment Update===============================================================================================
            foreach (ICLslabCartesianAgentSystem agentSystem in AgentSystems)
            {
                if (!agentSystem.IsFinished())
                {
                    List<Point3d> updatedAgentPositions = SlabUpdatedPositions(agentSystem);
                    agentSystem.CartesianEnvironment.AgentPositions = updatedAgentPositions;
                    agentSystem.CartesianEnvironment.UpdateEnvironment();
                    Dictionary<int, List<Point3d>> testupdate = agentSystem.CartesianEnvironment.NodalDisplacement;
                    foreach (List<Point3d> points in testupdate.Values)
                    {
                        this.tempDisp.Add(points[1]);
                    }
                }
            }
            ///Print Test===============================================================================================
            foreach (ICLslabCartesianAgentSystem agentSystem in this.AgentSystems)
            {
                foreach (CartesianAgent pos in agentSystem.Agents)
                {
                    RhinoApp.WriteLine(pos.Position + "RESET_POSITION");
                }

                Dictionary<int, List<Point3d>> nodalDisp = agentSystem.CartesianEnvironment.NodalDisplacement;
                List<double> displacementsZ = new List<double>();
                foreach (List<Point3d> pts in nodalDisp.Values)
                {
                    displacementsZ.Add(pts[1][2]);
                    RhinoApp.WriteLine(pts[1][2] + "RESET_NODALDISP");
                }
            }
        }

        public List<Point3d> SlabUpdatedPositions(ICLslabCartesianAgentSystem agentSystem)
        {
            List<Point3d> agentPositionsUpdate = new List<Point3d>();

            foreach (CartesianAgent agent in agentSystem.Agents)
            {
                Point3d point = agent.Position;
                agentPositionsUpdate.Add(point);
            }
            return agentPositionsUpdate;
        }

    }
}

