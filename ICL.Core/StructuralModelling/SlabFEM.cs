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

        public List<BuilderBeam> ComputeSlabFEM(ref List<Point3d> dispNodes)
        {
            ///Slab geometry modelling =============================================================
            //List<Point3> vertices = new List<Point3>();
            //List<Face3> faces = new List<Face3>();

            //Mesh3 karambaMesh = new Mesh3();

            foreach (MeshFace face in this.SlabGeo.Faces)
            {
                int v0 = face.A;

            }
            foreach (Point3d node in this.SlabGeo.Vertices)
            {
                dispNodes.Add(node);
            }

            //Mesh3 karambaSlabMesh = new Mesh3()
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

            ///CrossSectionBeam=========================================================================
            List<double> eccentricities = new List<double>();
            List<double> heights = new List<double>();
            List<FemMaterial> slabMaterial = new List<FemMaterial>();
            slabMaterial.Add(materials);
            foreach (MeshFace mf in this.SlabGeo.Faces)
            {
                eccentricities.Add(0.0);
                heights.Add(25); //beam thickness here
            }
            CroSec_Shell croSec_Shell = new CroSec_Shell(
            "Slab",
            "SlabCrossSection",
            "Germany",
            null,
            slabMaterial,
            eccentricities,
            heights
            );

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
            List<BuilderBeam> elems = k3d.Part.LineToBeam(columnLineElements, elmIds, croSecList, logger, out nodes);

            ///Build MeshToShell element============================================================
            //var elems = k3d.Part.MeshToShell()
            ///Supports & Loads=====================================================================
            ///Model==========================================================================
            ///Analyse==========================================================================

            return elems;
        }
    }
}
