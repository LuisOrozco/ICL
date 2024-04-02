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
    public class GhcRemoveAgentBeh : GH_Component
    {
        RemoveAgentBehavior behavior = null;

        public GhcRemoveAgentBeh()
          : base(
              "Define Remove Column Agent Behavior", 
              "Remove Column Beh.",
              "Define the system's remove column behavior",
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
        public override Guid ComponentGuid => new Guid("{3D81704C-EEC6-4F2C-9F70-BD58DD643089}");


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight", "W", "Weight", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Dist", "D", "Minimum Distance", GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("Probability", "P", "Probability of creating a new agent", GH_ParamAccess.item, 0.001);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Remove Agent Behavior", "B", "Remove Agent Behavior", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double iWeight = double.NaN;
            DA.GetData(0, ref iWeight);

            double iDistance = double.NaN;
            DA.GetData(1, ref iDistance);

            double iProbability = double.NaN;
            DA.GetData(2, ref iProbability);

            if (behavior == null)
                behavior = new RemoveAgentBehavior(iWeight, iDistance, iProbability);
            else
            {
                behavior.Weight = iWeight;
                behavior.Distance = iDistance;
                behavior.Probability = iProbability;
            }

            DA.SetData("Remove Agent Behavior", behavior);
        }
    }
}