using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rhino.Toolkit.Common.Geometry
{
    public class BaseGeometryWalker : IGeometryWalker
    {
        public virtual void Reset()
        {

        }

        public virtual void EnterObject(RhinoObject rhinoObject)
        {
        }

        public virtual void LeaveObject(RhinoObject rhinoObject)
        {
        }

        public virtual void LeaveBrepFace(BrepFace brepFace)
        {
        }

        public virtual void EnterExtrusion(Extrusion extrusion)
        {
        }

        public virtual void LeaveExtrusion(Extrusion extrusion)
        {
        }

        public virtual void EnterNurbsSurface(NurbsSurface nurbsSurface)
        {

        }

        public virtual void LeaveNurbsSurface(NurbsSurface nurbsSurface)
        {

        }

        public virtual void EnterBrep(Brep brep)
        {
        }

        public virtual void LeaveBrep(Brep brep)
        {
        }

        public virtual void EnterBrepFace(BrepFace brepFace)
        {
        }

        public virtual void EnterPolylineCurve(PolylineCurve polylineCurve)
        {
        }

        public virtual void LeavePolylineCurve(PolylineCurve polylineCurve)
        {
        }

        public virtual void EnterPolyline(Polyline polyline)
        {

        }

        public virtual void LeavePolyline(Polyline polyline)
        {

        }

        public virtual void EnterLine(Line line)
        {

        }

        public virtual void LeaveLine(Line line)
        {

        }
    }
}
