using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace ICL.GH
{
    public class ICL_GHInfo : GH_AssemblyInfo
    {
        public override string Name => "ICL.GH";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Grasshopper plug in of ICL method";

        public override Guid Id => new Guid("D0676205-F469-47A6-8F52-2BEBB175BA2E");

        //Return a string identifying you or your company.
        public override string AuthorName => "Keerthana Udaykumar";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "u.keerthana@gmail.com";
    }
}