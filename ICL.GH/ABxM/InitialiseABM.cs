using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using ICL.Core.Utilities;
using ICL.Core.Environment;
using ICL.Core.StructuralModelling;
using ICL.Core.AgentBehaviors;
using ICL.Core.AgentSystem;
using ICL.Core.ICLsolver;

using Karamba.Materials;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.CrossSections;
using Karamba.Supports;
using Karamba.Loads;
using Karamba.Models;
using Karamba.GHopper;
using Karamba.GHopper.Geometry;
using KarambaCommon;
using Karamba.Utilities;
using System.Linq;

using ABxM.Core;
using ABxM.Core.Behavior;
using ABxM.Core.AgentSystem;
using ABxM.Core.Agent;
using ABxM.Core.Environments;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
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
    private void RunScript(int iNcolumns, Curve iEnvironmentGeo, Mesh iEnvironmentSrfGeo, List<Point3d> iEnvironmentGeoPoints, List<string> iLoads, List<string> iMaterial, ref object oAgentStartPos, ref object oStartNodalDisplacement, ref object oMaxDispBehavior, ref object oCartesianAgents, ref object oEnvironmentStart, ref object CAS)
    {
        //COMPUTE START POSITIONS
        Mesh3 kSlabMesh = iEnvironmentSrfGeo.Convert();
        SlabInitializeAgents initializeAgents = new SlabInitializeAgents(iNcolumns, iEnvironmentGeo, iEnvironmentSrfGeo);
        initializeAgents.ComputeColumnStartPosSlab();
        List<Point3d> agentStartPositions = initializeAgents.ColumnStartPos;

        //COMPUTE NODAL DISPLACEMENTS OF THE ENVIRONMENT & DEFINE ICLSlabcartesianEnvironment
        ICLSlabEnvironment environment = new ICLSlabEnvironment(agentStartPositions, iEnvironmentGeoPoints, kSlabMesh, iLoads, iMaterial);
        environment.Execute();
        Dictionary<int, List<Point3d>> startNodalDisplacements = environment.NodalDisplacement;

        //DEFINE BEHAVIOR
        SlabMaxDisplacementBehavior maxDispBehavior = new SlabMaxDisplacementBehavior(iEnvironmentSrfGeo);
        maxDispBehavior.StartNodalDisplacemenets = startNodalDisplacements;
        List<BehaviorBase> agentBehaviors = new List<BehaviorBase>();
        agentBehaviors.Add(maxDispBehavior);

        //DEFINE CARTESIAN AGENTS
        List<CartesianAgent> cartesianAgents = new List<CartesianAgent>();
        foreach (Point3d pt in agentStartPositions)
        {
            CartesianAgent agent = new CartesianAgent(pt, agentBehaviors);
            cartesianAgents.Add(agent);
        }

        //DEFINE AGENT SYSTEM

        List<AgentSystemBase> casSystems = new List<AgentSystemBase>();
        ICLSlabAgentSystem csSystem = new ICLslabCartesianAgentSystem(cartesianAgents, environment);
        casSystems.Add(csSystem);

        //SOLVER
        SlabSolver testSlabSolver = new SlabSolver(casSystems);
        testSlabSolver.AgentSystems = casSystems;
        testSlabSolver.ICLslabSolverExecute();
        //    testSlabSolver.GetDisplayGeometries();

        //Output
        oAgentStartPos = agentStartPositions;
        oStartNodalDisplacement = startNodalDisplacements;
        oMaxDispBehavior = maxDispBehavior;
        oCartesianAgents = cartesianAgents;
        oEnvironmentStart = environment;
        CAS = casSystems;
        //    otest2 = nodes;
        //    otest3 = iEnvironmentSrfGeo.Vertices.Point3dAt(agentInd[0]);

        //testModel = new Karamba.GHopper.Models.GH_Model(test);

    }

    public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
    {
        throw new NotImplementedException();
    }

    // <Custom additional code> 

    // </Custom additional code> 
}