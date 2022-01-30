using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Toolkit.Common.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhino.Toolkit.RhinoSU
{
    public class GetClosedCurveAndFace: GetPoint
    {
        public BrepFace Face { get; set; }

        public Line? Line { get; set; }

        public Point3d? HitPoint { get; set; }

        public Mesh SelectedMesh { get; set; }

        public Guid? SelectedObjectId { get { return PickGeometryWalker.SelectedObjectId; } }

        public Vector3f? MeshFaceNormal { get; set; }

        PickClosedCurveAndFace PickGeometryWalker { get; set; }

        public GetClosedCurveAndFace():base()
        {
            this.ClearSnapPoints();
            this.EnableObjectSnapCursors(false);

            PickGeometryWalker = new PickClosedCurveAndFace();
        }

        public Plane GetPlane()
        {
            Plane plane = Plane.Unset;
            if(null != Face)
            {
                if (!Face.TryGetPlane(out plane))
                {
                    double u;
                    double v;
                    Face.ClosestPoint(HitPoint.Value, out u, out v);
                    plane = new Plane(Face.PointAt(u, v), Face.NormalAt(u, v));
                }
            }
            else if(null != SelectedMesh)
            {
                plane = new Plane(SelectedMesh.Vertices[0], new Vector3d(MeshFaceNormal.Value));
            }

            return plane;
        }

        protected override void OnMouseDown(GetPointMouseEventArgs e)
        {
            Update(e);
        }

        protected override void OnMouseMove(GetPointMouseEventArgs e)
        {
            Update(e);
        }

        void Update(GetPointMouseEventArgs e)
        {
            Face = null;
            HitPoint = null;

            SelectedMesh = null;
            MeshFaceNormal = null;

            var pick_context = new PickContext
            {
                View = e.Viewport.ParentView,
                PickStyle = PickStyle.PointPick,
            };

            var xform = e.Viewport.GetPickTransform(e.WindowPoint);
            pick_context.SetPickTransform(xform);

            Line pick_line;
            e.Viewport.GetFrustumLine(e.WindowPoint.X, e.WindowPoint.Y, out pick_line);

            pick_context.PickLine = pick_line;
            pick_context.UpdateClippingPlanes();

            ObjRef[] objRefs = RhinoDoc.ActiveDoc.Objects.PickObjects(pick_context);
            if (null == objRefs || !objRefs.Any())
            {
                return;
            }

            IList<RhinoObject> rhinoObjects = objRefs.Select(or => or.Object()).ToList();
            PickGeometryWalker.Update(pick_context, rhinoObjects);
            Face = PickGeometryWalker.SelectedBrepFace;
            HitPoint = PickGeometryWalker.SelectedPoint;

            SelectedMesh = PickGeometryWalker.SelectedMesh;
            MeshFaceNormal = PickGeometryWalker.MeshFaceNormal;
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            if(HitPoint.HasValue)
            {
                e.Display.DrawPoint(HitPoint.Value);
            }

            if (Line.HasValue)
            {
                e.Display.DrawLine(Line.Value, System.Drawing.Color.Red);
            }

            if(null != SelectedMesh)
            {
                e.Display.DrawMeshWires(SelectedMesh, System.Drawing.Color.Red);
            }

            if (null == Face)
            {
                return;
            }

            foreach(var loop in Face.Loops)
            {
                Curve curve = loop.To3dCurve();
                if (curve is PolyCurve)
                {
                    PolyCurve polyCurve = curve as PolyCurve;
                    for (int curveIndex = 0; curveIndex < polyCurve.SegmentCount; curveIndex++)
                    {
                        Curve subCurve = polyCurve.SegmentCurve(curveIndex);
                        e.Display.DrawCurve(subCurve, System.Drawing.Color.Red);
                    }
                }
            }
        }
    }
}
