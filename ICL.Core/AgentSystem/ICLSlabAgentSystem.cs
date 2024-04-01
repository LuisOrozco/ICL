﻿using System;
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
using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Geometry.Delaunay;
using Grasshopper.Kernel.Geometry.Voronoi;
using GH_IO;
using GH_IO.Serialization;

using ABxM.Core;
using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;
using ABxM.Core.Utilities;

using ICL.Core;
using ICL.Core.Behavior;
using ICL.Core.AgentSystem;
using ICL.Core.Environments;
using ICL.Core;
using ICL.Core.Utilities;
using ICL.Core.Utilities;
using ICL.Core.Utilities;

using Karamba;
using Karamba.CrossSections;
using Karamba.Elements;
using Karamba.Geometry;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Models;
using Karamba.Supports;
using Karamba.Utilities;
using KarambaCommon;
using Karamba.Results;

namespace ICL.Core.AgentSystem
{
    public class ICLSlabAgentSystem : CartesianAgentSystem
    {
        public Model KarambaModel;
        public List<BuilderElement> ModelElements;
        public List<Support> ConstantSupports;
        public List<Support> AllSupports = new List<Support>();
        public List<Load> Loads;
        public List<Curve> ExclusionCurves = new List<Curve>();
        public Mesh DelaunayMesh;
        public List<CartesianAgent> AddAgentList = new List<CartesianAgent>();
        public List<CartesianAgent> RemoveAgentList = new List<CartesianAgent>();

        /// <inheritdoc />
        public ICLSlabAgentSystem(List<CartesianAgent> agents, CartesianEnvironment cartesianEnvironment) : base(agents, cartesianEnvironment)
        {
            AllSupports = new List<Support>();
            ExclusionCurves = new List<Curve>();
        }
        
        /// <inheritdoc />
        public override void Reset()
        {
            base.Reset();
            KarambaModel = null;
            AllSupports.Clear();
            CartesianEnvironment.CustomData.Clear();
            Dictionary<string, object> displDict = this.RunKaramba();
            CartesianEnvironment.CustomData = displDict;
            //DelaunayMesh = this.ComputeDelaunayMesh();
            AddAgentList.Clear();
            RemoveAgentList.Clear() ;
        }

        /// <inheritdoc />
        public override void PreExecute()
        {
            if (CartesianEnvironment.CustomData == null)
            {
                CartesianEnvironment.CustomData = new Dictionary<string, object>();
            }
            if(DelaunayMesh == null) { DelaunayMesh = new Mesh();}
            // Check if CustomData is empty and displDict is not null or empty
            if (CartesianEnvironment.CustomData.Count == 0)
            {
                Dictionary<string, object> displDict = this.RunKaramba();
                if (displDict != null && displDict.Count > 0)
                {
                    CartesianEnvironment.CustomData = displDict;
                }
            }
            if(DelaunayMesh.Vertices.Count == 0)
            {
                DelaunayMesh = this.ComputeDelaunayMesh();

            }
            AddAgentList.Clear();
            RemoveAgentList.Clear();
            base.PreExecute();
        }

        public override void PostExecute()
        {
            base.PostExecute();
            Dictionary<string, object> displDict = this.RunKaramba();
            CartesianEnvironment.CustomData.Clear();
            CartesianEnvironment.CustomData = displDict;
            // make Delaunay Graph
            DelaunayMesh = this.ComputeDelaunayMesh();
            List<CartesianAgent> cleanAddAgentList = RemoveDuplicates(AddAgentList);
            foreach (CartesianAgent agent in cleanAddAgentList) { this.AddAgent(agent); }
            List<CartesianAgent> cleanRemoveAgentList = RemoveDuplicates(RemoveAgentList);
            foreach (CartesianAgent agent in cleanRemoveAgentList) { this.RemoveAgent(agent); }
        }

        public Mesh ComputeDelaunayMesh()
        {
            Node2List nodes = new Node2List();
            foreach (CartesianAgent agent in this.Agents)
                nodes.Append(new Node2(agent.Position.X, agent.Position.Y));
            List<Face> faces = new List<Face>();
            Mesh delMesh = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Mesh(nodes, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, ref faces);
            return delMesh;
        }

        /// <summary>
        /// remove duplicate agents from a list of agents
        /// </summary>
        /// <param name="oldAgentList">The list of agents inlcuding duplicates</param>
        /// <returns></returns>
        public List<CartesianAgent> RemoveDuplicates(List<CartesianAgent> oldAgentList)
        {
            List<CartesianAgent> noDuplicatesList = new List<CartesianAgent>();
            foreach (var agent in oldAgentList)
            {
                bool isDuplicate = false;
                foreach (var uniqueAgent in noDuplicatesList)
                {
                    if (agent.Position == uniqueAgent.Position)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate)
                {
                    noDuplicatesList.Add(agent);
                }
            }
            return noDuplicatesList;
        }

        /// <summary>
        /// Assemble Karamba Model, analyze it, and compute displacements
        /// </summary>
        /// <returns>Returns the dictionary of vertex indices and diplacements.</returns>
        public Dictionary<string, object> RunKaramba()
        {
            var k3d = new KarambaCommon.Toolkit();
            // Create support conditions at each agent poistion
            List<bool> columnSupportCondition = new List<bool>() { true, true, true, false, false, false };
            List<Support> columnSupports = Agents.Select(agent =>
                k3d.Support.Support(new Point3(((CartesianAgent)agent).Position.X, ((CartesianAgent)agent).Position.Y, ((CartesianAgent)agent).Position.Z), columnSupportCondition)
            ).ToList();
            AllSupports.Clear();
            AllSupports.AddRange(ConstantSupports);
            AllSupports.AddRange(columnSupports);

            // Build Karamba Model using Assemble
            Model model = k3d.Model.AssembleModel
                (
                    ModelElements,
                    AllSupports,
                    Loads,
                    out string info,
                    out double mass,
                    out Point3 cog,
                    out info,
                    out bool flag
                );

            // Check for Karamba3D License
            bool validLicense;
            string licenseMessage;

            validLicense = Karamba.Licenses.License.license_is_valid(model, out licenseMessage);
            if (!validLicense)
            {
                throw new Exception("License not valid: " + licenseMessage + "\n" + "License Path: " + Karamba.Licenses.License.licensePath());
            }

            // Analyze the Karamba Model
            List<double> max_disp;
            List<double> out_g;
            List<double> out_comp;
            string analyzeMessage;

            model = k3d.Algorithms.AnalyzeThI(model, out max_disp, out out_g, out out_comp, out analyzeMessage);

            // Update Environment CustomData with new displacements
            // get all the verices from the environment mesh
            List<Point3> nodes = new List<Point3>();
            if (ModelElements.Count > 1) throw new Exception("Too many Shells");
            else
            {
                Mesh3 mesh = ((BuilderShell)ModelElements[0]).mesh as Mesh3;
                if (mesh == null) throw new Exception("not a Mesh3");
                else
                {
                    nodes = new List<Point3>(mesh.Vertices);
                }
            }
            // Calculate Nodal Displacements
            var trans = new List<List<Vector3>>();
            var rotat = new List<List<Vector3>>();
            Model clonedModel = model.Clone();
            clonedModel.cloneElements();
            string lc_ind = "0";
            List<int> ids = Enumerable.Range(0, nodes.Count).ToList();
            NodalDisp.solve(clonedModel, lc_ind, ids, out trans, out rotat);
            // Scale Displacement Vectors, and Convert resulting translations into Rhino Vector3d
            double c = Math.Pow(10, 7);
            List<Vector3d> dispVectors = trans[0].Select(v => new Vector3d(v.X, v.Y, v.Z / c)).ToList();
            // Compute the displacement distances
            List<double> displacementDistances = dispVectors.Select(vec => vec.Length).ToList();
            Dictionary<string, object> nodalDisplacements = new Dictionary<string, object>();

            for (int i = 0; i < displacementDistances.Count; i++)
            {
                nodalDisplacements.Add(i.ToString(), displacementDistances[i]);
            }

            KarambaModel = clonedModel;
            return nodalDisplacements;
        }

    }
}
