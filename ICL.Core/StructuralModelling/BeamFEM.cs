using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Karamba;
using Karamba.Geometry;
using KarambaCommon;
using Karamba.Supports;
using Karamba.CrossSections;
using Karamba.Utilities;
using Rhino;

namespace ICL.Core.StructuralModelling
{
    public class BeamFEM
    {
        ///public attributes

        public double divisionLength = 0.5;//in mm
        public List<Point3d> BeamLinePoints = new List<Point3d>();
        public List<Point3d> columnPositions = new List<Point3d>();
        public List<string> BeamLoadTypes = new List<string>();
        public Dictionary<int, Point3d> NodalDisplacement = new Dictionary<int, Point3d>();
        public string Material;
        ///Beam element 
        ///material
        ///load
        ///support
        ///model
        public BeamFEM(List<Point3d> beamLinePoints, List<Point3d> columnPositions, List<string> beamLoadTypes, string material)
        {
            this.BeamLinePoints = beamLinePoints;
            this.columnPositions = columnPositions;
            this.BeamLoadTypes = beamLoadTypes;
            this.Material = material;
        }
        ///create Beam element 

        public List<Support> computeFEM()
        {
            ///KarambaLine==========================================================================
            List<Line3> beamLineList = new List<Line3>();

            Point3 pStart = new Point3(this.BeamLinePoints[0][0], this.BeamLinePoints[0][1], this.BeamLinePoints[0][2]);
            Point3 pEnd = new Point3(this.BeamLinePoints[1][0], this.BeamLinePoints[1][1], this.BeamLinePoints[1][2]);
            Line3 beamLine = new Line3(pStart, pEnd);
            beamLineList.Add(beamLine);

            double[] tParams = FindBeamCurveParameters();

            //Material definition
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

            ///CrossSection=========================================================================
            List<CroSec> croSecList = new List<CroSec>();
            CroSec_Trapezoid trapCroSec = new CroSec_Trapezoid(
            "Beam",
            "BeamCrossSection",
            null,
            null,
            materials,
            30,
            100,
            100);
            croSecList.Add(trapCroSec);

            ///Build LineToBeam element=============================================================
            var k3d = new KarambaCommon.Toolkit();
            var logger = new MessageLogger();
            var nodes = new List<Point3>();
            var elems = k3d.Part.LineToBeam(beamLineList, new List<string>() { "Be0" }, croSecList, logger, out nodes);

            ///Supports & Loads
            List<List<bool>> supportConditions = CreateSupportCondition();
            List<int> TparamIndexOfColumnPos = ComputeTparamIndexOfColumnPos(tParams);
            List<Support> supports = new List<Support>();
            for (int i = 0; i < supportConditions.Count; i++)
            {
                Support support = k3d.Support.Support(TparamIndexOfColumnPos[i], supportConditions[i]);
                supports.Add(support);
            }
            ///find where column is get param & idex of param on paramlist(function)
            ///create supports for every column param 
            ///make supports list 

            return supports;
            //make line
            //List<tParam> divide curev and get the paramters of the curve division not the points 
            //Material definition
            ///KarambaSupport points
            //based on agent position get the tParam(make be this is a function)
            ///make KarambaLine to Curve
            ///make nodes list 
            ///divide the line and get the param of the dividsion to input into the BeamDisplacements component
            ///make BeamToLine
            ///Material 

        }
        public List<int> ComputeTparamIndexOfColumnPos(double[] beamTparamList)
        {
            List<double> columnPosParam = new List<double>();
            foreach (Point3d pt in this.columnPositions)
            {
                double t;
                Line l = new Line(this.BeamLinePoints[0], this.BeamLinePoints[1]);
                bool curveParam = l.ToNurbsCurve().ClosestPoint(pt, out t);
                if (curveParam == true)
                {
                    columnPosParam.Add(t);
                }
                else
                {
                    RhinoApp.WriteLine("Column not connected to beam");
                }
            }
            List<int> columnPosParamLocation = new List<int>();
            foreach (double t in columnPosParam)
            {
                for (var i = 0; i < beamTparamList.Length; i++)
                {
                    if (t == beamTparamList[i])
                    {
                        columnPosParamLocation.Add(i);
                    }
                }
            }
            return columnPosParamLocation;
        }


        public List<List<bool>> CreateSupportCondition()
        {
            List<List<bool>> conditions = new List<List<bool>>();
            foreach (Point3d pt in this.columnPositions)
            {
                List<bool> cond = new List<bool>() { false, true, true, false, false, false };
                conditions.Add(cond);
            }
            return conditions;
        }
        public double[] FindBeamCurveParameters()
        {

            Line line = new Line(this.BeamLinePoints[0], this.BeamLinePoints[1]);
            double lineLength = line.Length;
            int nCount = Convert.ToInt32(Math.Round(lineLength / divisionLength));
            double[] tParams = line.ToNurbsCurve().DivideByCount(nCount, true);
            return tParams;
        }
        ///material definition
        ///load definition
        ///support definition
        ///support definition
        ///create model 
        ///return model
    }
}
