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
    public class GhcSimpleAddBeh : GH_Component
    {
        SimpleAddBehavior behavior = null;

        public GhcSimpleAddBeh()
          : base(
              "Define Simple Add Agent Behavior", 
              "Simple Add Beh.",
              "Define a simplified add behavior",
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
        public override Guid ComponentGuid => new Guid("{58B3CAA5-DDC8-4EE0-A756-05C778A8F52D}");


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight", "W", "Weight", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Displ", "D", "Maximum Displacement", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("Probability", "P", "Probability of creating a new agent", GH_ParamAccess.item, 0.01);
            pManager.AddBooleanParameter("Reset", "R", "Reset agent count", GH_ParamAccess.item, false);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Simple Add Behavior", "B", "Simple Add Behavior", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double iWeight = double.NaN;
            DA.GetData(0, ref iWeight);

            double iDisplacement = double.NaN;
            DA.GetData(1, ref iDisplacement);

            double iProbability = double.NaN;
            DA.GetData(2, ref iProbability);

            bool iReset = false;
            DA.GetData(3, ref iReset);

            if (behavior == null)
                behavior = new SimpleAddBehavior(iWeight, iDisplacement, iProbability);
            else
            {
                behavior.Weight = iWeight;
                behavior.Displacement = iDisplacement;
                behavior.Probability = iProbability;
            }

            DA.SetData(0, behavior);
        }
    }
}