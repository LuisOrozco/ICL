using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using ICL.Core.Solver;

namespace ICL.GH
{
    public class GHBeamSolver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GHBeamSolver()
          : base("BeamSolver", "solver",
              "Description",
              "Curve", "Primitive")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("testString", "S", "test String", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Display Test", "T", "Display Text", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string testString = "a";

            if (!DA.GetData(0, ref testString)) return;

            BeamSolver outString = new BeamSolver("hello");
            string outTest = outString.ConcatString("hello");

            DA.SetData(0, outTest);
            //outString.ConcatString("hello");

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8C3E769A-AB91-426B-AEE3-2F203A2233AF"); }
        }
    }
}