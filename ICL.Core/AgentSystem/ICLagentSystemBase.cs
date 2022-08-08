﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICD.AbmFramework.Core.AgentSystem;
using ICL.Core.Agent;
using ICL.Core.AgentBehaviors;

namespace ICL.Core.AgentSystem
{
    public abstract class ICLagentSystemBase : AgentSystemBase
    {
        /// <summary>
        /// List of ICLagents.
        /// </summary>
        public List<ICLagentBase> ICLagents = new List<ICLagentBase>();

        ///ICL agent base PreExecute

        public virtual void ICLPreExecute()
        {
            foreach (ICLagentBase agent in Agents)
            {
                agent.ICLPreExecute();
            }
        }


    }
}
