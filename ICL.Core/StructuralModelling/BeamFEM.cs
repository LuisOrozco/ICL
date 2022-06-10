using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Karamba;
using KarambaCommon;

namespace ICL.Core.StructuralModelling
{
    public class BeamFEM
    {
        ///public attributes

        public List<Point3d> BeamLinePoints = new List<Point3d>();
        public List<Point3d> columnPositions = new List<Point3d>();
        public List<string> BeamLoadTypes = new List<string>();
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
        ///material definition
        ///load definition
        ///support definition
        ///support definition
        ///create model 
        ///return model
    }
}
