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

        public double[] computeFEM()
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

            return tParams;
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
