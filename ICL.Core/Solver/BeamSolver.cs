using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;

namespace ICL.Core.Solver
{
    internal class NamespaceDoc { }

    public class BeamSolver
    {
        public string test;

        public BeamSolver(string names)
        {
            test = names;
        }

        public string ConcatString(string name)
        {
            string test2 = name + "test";
            return test2;
        }

        static void Main(string[] args)
        {
            BeamSolver testA = new BeamSolver("Hello");
            RhinoApp.WriteLine(testA + "check Output");
        }
        //call maxdisplacementBehaviour(get string)
        //call FEA (get string)
        //call beam (get string)
        //return (concatinated strings)
    }
}
