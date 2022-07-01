using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using ICD.AbmFramework.Core.AgentSystem;
using ICL.Core.Environment;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.Behavior;

namespace ICL.Core.Solver
{
    internal class NamespaceDoc { }

    public class BeamSolver
    {
        // public attributes 
        public List<AgentSystemBase> AgentSystems;

        public int IterationCount = 0;

        public BeamEnvironmentNodalDisplacement BeamStructuralData;

        // Method for starting solver from a new list
        public BeamSolver()
        {
            this.AgentSystems = new List<AgentSystemBase>();
        }

        ///Method0:
        ///<summary>
        ///Method for starting solver from an exisiting list of agents
        ///</summary>
        public BeamSolver(List<AgentSystemBase> agentSystems)
        {
            this.AgentSystems = agentSystems;

            foreach (AgentSystemBase agentSystem in this.AgentSystems)
            {
                foreach (AgentBase agent in agentSystem.Agents)
                {
                    foreach (BehaviorBase behavior in agent.Behaviors)
                    {
                        behavior.Solver = BeamSolver; //solve from here //may be put the file outside the folder?
                    }
                }
            }



        }
        ///List<Point3d> agentPositionsUpdate =  List<Point3d>()
        ///foreach (CartesianAgent agent in this.AgentSystems)
        ///agentPositionsUpdate.Add(agent.Positions)
        /// this.beamEnvNodalDisps.AgentPositions = agentPositionsUpdate
        /// this.beamEnvNodalDisps.PreExecute
        /// this.beamEnvNodalDisps.Execute
        /// foreach agentsystem in agentSystem
        /// foreach agent in agent system
        /// foreach behavor in agent.behavior
        /// behavior.nodalsiap = this.beamEnvNodalDisps.Nodaldisp

    }
}
