using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;


using ABxM.Core;
using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;
using ICL.Core.Behavior;

namespace ICL.GH.GhComponents
{
    public class GhcAddAgentBeh : GH_Component
    {
        AddAgentBehavior behavior = null;

        public GhcAddAgentBeh()
          : base(
              "Define Add Column Agent Behavior", 
              "Add Column Beh.",
              "Define the system's add column behavior",
              "ABxM",
              "ICL")
        {
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override Guid ComponentGuid => new Guid("{38CAD9B1-AFD9-46B0-B145-420F2C067ED3}");


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight", "W", "Weight", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Displ", "D", "Maximum Displacement", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("Probability", "P", "Probability of creating a new agent", GH_ParamAccess.item, 0.001);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Add Agent Behavior", "B", "Add Agent Behavior", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double iWeight = double.NaN;
            DA.GetData(0, ref iWeight);

            double iDisplacement = double.NaN;
            DA.GetData(1, ref iDisplacement);

            double iProbability = double.NaN;
            DA.GetData(2, ref iProbability);

            if (behavior == null)
                behavior = new AddAgentBehavior(iWeight, iDisplacement, iProbability);
            else
            {
                behavior.Weight = iWeight;
                behavior.Displacement = iDisplacement;
                behavior.Probability = iProbability;
            }

            DA.SetData("Add Agent Behavior", behavior);
        }
    }
}