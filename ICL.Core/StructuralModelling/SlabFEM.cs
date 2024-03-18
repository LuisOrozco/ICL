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
using Karamba.Supports;
using Karamba.Loads;



namespace ICL.Core.StructuralModelling
{
    public class SlabFEM
    {
        ///public attributes 

        public double MeshRes = 100;
        public Mesh3 SlabGeo;
        public List<Point3d> ColumnPositions = new List<Point3d>();
        public Dictionary<int, Point3d> NodalDisplacement = new Dictionary<int, Point3d>();

        ///initialize class
        public SlabFEM(Mesh3 slabGeo, List<Point3d> columnPositions)
        {
            this.SlabGeo = slabGeo;
            this.ColumnPositions = columnPositions;
        }

        public Model ComputeSlabFEM(ref List<Point3> dispNodes)
        {
            ///Slab geometry modelling =============================================================
            List<Mesh3> slabMeshElements = new List<Mesh3>();
            slabMeshElements.Add(this.SlabGeo);
            foreach (Point3 v in this.SlabGeo.Vertices)
            {
                dispNodes.Add(v);
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

            ///CrossSectionSlab=========================================================================
            List<double> eccentricities = new List<double>();
            List<double> heights = new List<double>();
            List<FemMaterial> slabMaterial = new List<FemMaterial>();
            slabMaterial.Add(materials);
            foreach (Face3 mf in this.SlabGeo.Faces)
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
            foreach (Face3 mf in this.SlabGeo.Faces)
            {
                slabCroSecList.Add(croSec_Shell);
            }

            ///Build MeshToShell element============================================================
            var k3d = new KarambaCommon.Toolkit();
            var logger = new MessageLogger();
            List<string> elmIds = new List<string>();
            List<Point3> slabNodes = new List<Point3>();
            List<string> slabElmIds = new List<string>();
            for (var i = 0; i < this.SlabGeo.Faces.Count; i++)
            {
                string id = "se" + i.ToString();
                elmIds.Add(id);
            }
            List<BuilderShell> slabElems = k3d.Part.MeshToShell(slabMeshElements, slabElmIds, slabCroSecList, logger, out slabNodes);

            ///Supports & Loads=====================================================================

            ///column top support 
            List<List<bool>> supportConditions = CreateSupportCondition();
            List<Support> supports = new List<Support>();
            for (int i = 0; i < this.ColumnPositions.Count; i++)
            {
                Point3 nodePt = new Point3(this.ColumnPositions[i][0], this.ColumnPositions[i][1], this.ColumnPositions[i][2]);
                Support support = k3d.Support.Support(nodePt, supportConditions[i]);
                supports.Add(support);
            }

            ///Loads===============================================================================
            var gLoad = k3d.Load.GravityLoad(new Vector3(0, 0, -1));
            List<Vector3> vecList = new List<Vector3>();
            Vector3 vec = new Vector3(0, 0, -1);
            vecList.Add(vec);
            var meshUdl = k3d.Load.MeshLoad(vecList, this.SlabGeo);
            List<Load> loads = new List<Load>() { gLoad, meshUdl };

            ///Model===============================================================================
            List<BuilderElement> modelElements = new List<BuilderElement>
            {
                slabElems[0],
            };
            Model model = k3d.Model.AssembleModel(
            modelElements,
            supports,
            loads,
            out string info,
            out double mass,
            out Point3 cog,
            out info,
            out bool flag
            );

            ///License=============================================================================
            ///License check from Clements
            bool validLicense;
            string message;

            validLicense = Karamba.Licenses.License.license_is_valid(model, out message);
            if (!validLicense)
            {
                throw new Exception("License not valid: " + message + "\n" + "License Path: " + Karamba.Licenses.License.licensePath());
            }
            ///Analyse
            List<double> max_disp;
            List<double> out_g;
            List<double> out_comp;
            model = k3d.Algorithms.AnalyzeThI(model, out max_disp, out out_g, out out_comp, out message);

            return model;
        }
        /// Method:0
        /// <summary>
        /// Method creating Kasramba support conditions
        /// </summary>
        /// <returns> List<List<bool>> </returns>
        public List<List<bool>> CreateSupportCondition()
        {
            List<List<bool>> conditions = new List<List<bool>>();
            for (int i = 0; i < this.ColumnPositions.Count; i++)
            {
                if (i == 0)
                {
                    List<bool> cond0 = new List<bool>() { false, false, true, false, false, true };
                    conditions.Add(cond0);
                }
                else if (i == 1)
                {
                    List<bool> cond1 = new List<bool>() { true, true, true, false, false, true };
                    conditions.Add(cond1);
                }
                else
                {
                    var random = new Random();
                    List<bool> cond = new List<bool>() { (random.Next(2) == 1), (random.Next(2) == 1), true, false, false, true };
                    conditions.Add(cond);
                }
            }
            return conditions;
        }
    }
}
