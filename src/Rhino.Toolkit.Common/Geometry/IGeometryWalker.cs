using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Toolkit.Common.Geometry
{
    public interface IGeometryWalker
    {
        void EnterObject(RhinoObject rhinoObject);
        void LeaveObject(RhinoObject rhinoObject);

        void EnterExtrusion(Extrusion extrusion);
        void LeaveExtrusion(Extrusion extrusion);

        void EnterNurbsSurface(NurbsSurface nurbsSurface);
        void LeaveNurbsSurface(NurbsSurface nurbsSurface);

        void EnterBrep(Brep brep);
        void LeaveBrep(Brep brep);

        void EnterBrepFace(BrepFace brepFace);
        void LeaveBrepFace(BrepFace brepFace);

        void EnterPolylineCurve(PolylineCurve polylineCurve);
        void LeavePolylineCurve(PolylineCurve polylineCurve);

        void EnterPolyline(Polyline polyline);
        void LeavePolyline(Polyline polyline);

        void EnterLine(Line line);
        void LeaveLine(Line line);
    }
}
