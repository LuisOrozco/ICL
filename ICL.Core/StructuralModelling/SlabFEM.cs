using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Rhino;
using Karamba.Models;
using KarambaCommon;
using Karamba.Materials;
using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Utilities;
using Karamba.Elements;



namespace ICL.Core.StructuralModelling
{
    public class SlabFEM
    {
        //public attributes 

        public double MeshRes = 100;
        public Mesh SlabGeo;
        public List<Point3d> ColumnPositions = new List<Point3d>();
        public Dictionary<int, Point3d> NodalDisplacement = new Dictionary<int, Point3d>();

        //initialize class
        public SlabFEM(Mesh slabGeo, List<Point3d> columnPositions)
        {
            this.SlabGeo = slabGeo;
            this.ColumnPositions = columnPositions;
        }

        public List<BuilderShell> ComputeSlabFEM(ref List<Point3d> dispNodes)
        {
            ///Slab geometry modelling =============================================================

            List<Mesh3> slabMeshElements = new List<Mesh3>();
            Mesh3 kSlabMesh = new Mesh3();
            slabMeshElements.Add(kSlabMesh);

            foreach (MeshFace f in this.SlabGeo.Faces)
            {
                kSlabMesh.AddFace(f.A, f.B, f.C);
            }
            foreach (Point3d v in this.SlabGeo.Vertices)
            {
                kSlabMesh.AddVertex(new Point3(v.X, v.Y, v.Z));
                dispNodes.Add(v);
            }

            ///Columns geometry modelling =============================================================
            List<Line3> columnLineElements = new List<Line3>();
            foreach (Point3d pt in ColumnPositions)
            {
                Line3 line = new Line3(new Point3(pt.X, pt.Y, pt.Z), new Point3(pt.X, pt.Y, pt.Z - 300));
                columnLineElements.Add(line);
            }

            ///Material definition==================================================================
            Karamba.Materials.FemMaterial_Isotrop materials = new Karamba.Materials.FemMaterial_Isotrop(
            "family",
            "name",
            1155,
            69,
            57,
            0.0,
            23.5,
            -23.5,
            Karamba.Materials.FemMaterial.FlowHypothesis.mises,
            1e-4,
            null);

            ///CrossSectionColumn=========================================================================
            List<CroSec> croSecList = new List<CroSec>();
            CroSec_Trapezoid trapCroSec = new CroSec_Trapezoid(
            "Column",
            "ColumnCrossSection",
            null,
            null,
            materials,
            10000,//has to be parameterised
            10000,
            10000);
            foreach (Point3d pt in ColumnPositions)
            {
                croSecList.Add(trapCroSec);
            }

            ///CrossSectionSlab=========================================================================
            List<double> eccentricities = new List<double>();
            List<double> heights = new List<double>();
            List<FemMaterial> slabMaterial = new List<FemMaterial>();
            slabMaterial.Add(materials);
            foreach (MeshFace mf in this.SlabGeo.Faces)
            {
                eccentricities.Add(0.0);
                heights.Add(25); //beam thickness here
            }
            List<CroSec> slabCroSecList = new List<CroSec>();
            CroSec_Shell croSec_Shell = new CroSec_Shell(
            "Slab",
            "SlabCrossSection",
            "Germany",
            null,
            slabMaterial,
            eccentricities,
            heights
            );
            slabCroSecList.Add(croSec_Shell);

            ///Build LineToBeam element=============================================================
            var k3d = new KarambaCommon.Toolkit();
            var logger = new MessageLogger();
            var nodes = new List<Point3>();
            List<string> elmIds = new List<string>();
            for (var i = 0; i < this.ColumnPositions.Count; i++)
            {
                string id = "e" + i.ToString();
                elmIds.Add(id);
            }
            List<BuilderBeam> columnElems = k3d.Part.LineToBeam(columnLineElements, elmIds, croSecList, logger, out nodes);

            ///Build MeshToShell element============================================================
            List<Point3> slabNodes = new List<Point3>();
            List<string> slabElmIds = new List<string>();
            for (var i = 0; i < kSlabMesh.Faces.Count; i++)
            {
                string id = "se" + i.ToString();
                elmIds.Add(id);
            }
            List<BuilderShell> slabElems = k3d.Part.MeshToShell(slabMeshElements, slabElmIds, slabCroSecList, logger, out slabNodes);


            ///Supports & Loads=====================================================================
            ///Model==========================================================================
            ///Analyse==========================================================================

            return slabElems;
        }
    }
}
