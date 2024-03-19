using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;

using Rhino.Collections;
using Rhino.DocObjects;
using GH_IO;
using GH_IO.Serialization;

using ABxM.Core;
using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;
using ABxM.Core.Utilities;

using ICL.Core;
using ICL.Core.AgentBehaviors;
using ICL.Core.AgentSystem;
using ICL.Core.Environment;
using ICL.Core.ICLsolver;
using ICL.Core.StructuralAnalysis;
using ICL.Core.StructuralModelling;
using ICL.Core.Utilities;

using Karamba;
using Karamba.CrossSections;
using Karamba.Elements;
using Karamba.Geometry;
using Karamba.GHopper;
using Karamba.GHopper.Geometry;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Models;
using Karamba.Supports;
using Karamba.Utilities;
using KarambaCommon;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class InitialiseEnv : GH_ScriptInstance
{
    #region Utility functions
    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
    /// <param name="text">String to print.</param>
    private void Print(string text) { /* Implementation hidden. */ }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { /* Implementation hidden. */ }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { /* Implementation hidden. */ }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private readonly RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private readonly GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private readonly IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private readonly int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments,
    /// Output parameters as ref arguments. You don't have to assign output parameters,
    /// they will have a default value.
    /// </summary>
    private void RunScript(List<Point3d> iBoundaryCorners, List<Point3d> iColumnStartPos, Mesh iEnvMesh, List<string> iLoads, List<string> iMaterial, ref object oCartEnv, ref object oMeshEnv)
    {
        CartesianEnvironment cartEnv = new CartesianEnvironment(iBoundaryCorners);
        MeshEnvironment meshEnv = new MeshEnvironment(iEnvMesh);
        Mesh3 envMesh = iEnvMesh.Convert();
        // Create an instance of the SlabFEM class
        SlabFEM femModel = new SlabFEM(envMesh, iColumnStartPos);

        // Compute the FEM and note the model before analysis
        List<Point3> nodes = new List<Point3>();
        Model slabModel = femModel.ComputeSlabFEM(ref nodes);

        // Create an instance of the FEA class
        FEA slabEnvironmentFEA = new FEA(slabModel, nodes);

        // Compute nodal displacements
        List<Point3d> nodalDisplacements = slabEnvironmentFEA.ComputeNodalDisplacements();

        // Convert Point3 nodes to Point3d
        List<Point3d> rhinoNodes = slabEnvironmentFEA.ConvertPt3ToPt3d(nodes);

        List<Dictionary<string, object>> dictList = new List<Dictionary<string, object>>();
        Dictionary<string, object> originalPoints = new Dictionary<string, object>();
        Dictionary<string, object> displacedPoints = new Dictionary<string, object>();
        Dictionary<string, object> tupleDict = new Dictionary<string, object>();

        for (int i = 0; i < nodalDisplacements.Count; i++)
        {
            originalPoints.Add(i.ToString(), rhinoNodes[i]);
            displacedPoints.Add(i.ToString(), nodalDisplacements[i]);
            Tuple<Point3d, Point3d> nodeData = new Tuple<Point3d, Point3d>(rhinoNodes[i], nodalDisplacements[i]);
            tupleDict.Add(i.ToString(), nodeData);

        }
        dictList.Add(originalPoints);
        dictList.Add(displacedPoints);

        cartEnv.CustomData = tupleDict;
        meshEnv.CustomData = displacedPoints;

        oCartEnv = cartEnv;
        oMeshEnv = meshEnv;
    }

    public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
    {
        throw new NotImplementedException();
    }

    // <Custom additional code> 

    // </Custom additional code> 
}