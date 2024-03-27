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
using ICL.Core.AgentBehaviors;

namespace ICL.GH
{
    public class GhcMaxDisplBeh : GH_Component
    {
        SlabMaxDisplacementBehavior behavior = null;

        public GhcMaxDisplBeh()
          : base(
              "Define Column Maximum Displacement Behavior", 
              "ICL Max Displ",
              "Define a column's maximum displacement behaviour",
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
        public override Guid ComponentGuid => new Guid("{9CC599EA-E901-4054-86E7-2638C53ABAFB}");


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Weight", "W", "Weight", GH_ParamAccess.item, 1.0);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Max Displ Behavior", "B", "Max Displ Behavior", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double iWeight = double.NaN;
            DA.GetData("Weight", ref iWeight);

            if (behavior == null)
                behavior = new SlabMaxDisplacementBehavior();
            else
            {
                behavior.Weight = iWeight;
            }

            DA.SetData("Max Displ Behavior", behavior);
        }
    }
}