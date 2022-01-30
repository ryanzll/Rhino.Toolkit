using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Toolkit.Common.Geometry
{
    public class GeometryParser
    {
        IGeometryWalker GeometryWalker { get; set; }

        public GeometryParser(IGeometryWalker geometryWalker)
        {
            GeometryWalker = geometryWalker;
        }

        public void Parse(IList<RhinoObject> rhinoObjects)
        {
            if (null != GeometryWalker)
            {
                GeometryWalker.Reset();
            }
            foreach (RhinoObject rhinoObject in rhinoObjects)
            {
                if(null != GeometryWalker)
                {
                    GeometryWalker.EnterObject(rhinoObject);
                }
                switch(rhinoObject.Geometry.ObjectType)
                {
                    case ObjectType.Extrusion:
                        Extrusion extrusion = rhinoObject.Geometry as Extrusion;
                        ParseExtrusion(extrusion);
                        break;
                    case ObjectType.Brep:
                        Brep brep = rhinoObject.Geometry as Brep;
                        ParseBrep(brep);
                        break;
                    case ObjectType.Curve:
                        if(rhinoObject.Geometry is PolylineCurve)
                        {
                            ParsePolylineCurve(rhinoObject.Geometry as PolylineCurve);
                        }
                        break;
                }
                if (null != GeometryWalker)
                {
                    GeometryWalker.LeaveObject(rhinoObject);
                }
            }
        }

        void ParseExtrusion(Extrusion extrusion)
        {
            if (null != GeometryWalker)
            {
                GeometryWalker.EnterExtrusion(extrusion);
            }

            if (null != GeometryWalker)
            {
                GeometryWalker.LeaveExtrusion(extrusion);
            }
        }

        void ParseNurbsSurface(NurbsSurface nurbsSurface)
        {
            if (null != GeometryWalker)
            {
                GeometryWalker.EnterNurbsSurface(nurbsSurface);
            }

            Brep brep = nurbsSurface.ToBrep();
            if(null != brep)
            {
                ParseBrep(brep);
            }

            if (null != GeometryWalker)
            {
                GeometryWalker.LeaveNurbsSurface(nurbsSurface);
            }
        }

        void ParseBrep(Brep brep)
        {
            if (null != GeometryWalker)
            {
                GeometryWalker.EnterBrep(brep);
            }

            foreach (BrepFace brepface in brep.Faces)
            {
                ParseBrepFace(brepface);
            }

            if (null != GeometryWalker)
            {
                GeometryWalker.LeaveBrep(brep);
            }
        }

        void ParseBrepFace(BrepFace brepface)
        {
            if (null != GeometryWalker)
            {
                GeometryWalker.EnterBrepFace(brepface);
            }

            if (null != GeometryWalker)
            {
                GeometryWalker.LeaveBrepFace(brepface);
            }
        }

        void ParsePolylineCurve(PolylineCurve polylineCurve)
        {
            if (null != GeometryWalker)
            {
                GeometryWalker.EnterPolylineCurve(polylineCurve);
            }

            if (polylineCurve.IsPolyline())
            {
                Polyline polyline = polylineCurve.ToPolyline();
                for (int lineIndex = 0; lineIndex < polyline.Count; lineIndex++)
                {
                    Line line = polyline.SegmentAt(lineIndex);
                    ParseLine(line);
                }
            }

            if (null != GeometryWalker)
            {
                GeometryWalker.LeavePolylineCurve(polylineCurve);
            }
        }

        void ParseLine(Line line)
        {
            if (null != GeometryWalker)
            {
                GeometryWalker.EnterLine(line);
            }

            if (null != GeometryWalker)
            {
                GeometryWalker.LeaveLine(line);
            }
        }
    }
}
