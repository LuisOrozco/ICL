using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICD.AbmFramework.Core.Behavior;
using ICL.Core.Solver;

namespace ICL.Core.AgentBehaviors
{
    abstract public class ICLbehaviorBase: BehaviorBase
    {
        /// <summary>
        /// The field that provides access to the ICL BeamSolver.
        /// </summary>
        public BeamSolver BeamSolver;
    }
}
