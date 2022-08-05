using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICD.AbmFramework.Core.Environments;
using ICL.Core.AgentSystem;

namespace ICL.Core.Environment
{
    public abstract class ICLenvironmentBase : EnvironmentBase
    {
        public List<ICLagentSystemBase> ICLagentSystems = new List<ICLagentSystemBase>();
    }
}
