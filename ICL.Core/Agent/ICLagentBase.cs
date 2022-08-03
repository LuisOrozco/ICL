using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICD.AbmFramework.Core.Agent;
using ICL.Core.AgentBehaviors;

namespace ICL.Core.Agent
{
    public abstract class ICLagentBase : AgentBase
    {
        /// <summary>
        /// List of ICL behaviors.
        /// </summary>
        public List<ICLbehaviorBase> ICLbehaviors = new List<ICLbehaviorBase>();

        /// <summary>
        /// The ID of the agent, unique within the given system
        /// </summary>
        public int Ids { get; internal set; }
    }
}
