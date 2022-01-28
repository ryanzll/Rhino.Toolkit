using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Toolkit.RhinoSU
{
    public class GetPointAndFace:GetPoint
    {
        public BrepFace Face { get; set; }

        public Point3d HitPoint { get; set; }

        public GetPointAndFace():base()
        {
            this.ClearSnapPoints();
            this.EnableObjectSnapCursors(false);
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

            double maxDepth = double.NegativeInfinity;
            BrepFace selectedFace = null;
            double t;
            double depth;
            double distance;
            Point3d hitPoint = Point3d.Unset;
            PickContext.MeshHitFlag hitFlag;
            int hitIndex;
            foreach (var objRef in objRefs)
            {
                var rhinoObject = objRef.Object();
                if (rhinoObject is ExtrusionObject)
                {
                    ExtrusionObject extrusionObject = rhinoObject as ExtrusionObject;
                    Extrusion extrusion = extrusionObject.Geometry as Extrusion;
                    Brep brep = extrusion.ToBrep();
                    NurbsSurface nurbsSurface = extrusion.ToNurbsSurface();

                    foreach (BrepFace face in brep.Faces)
                    {
                        Mesh mesh = face.GetMesh(MeshType.Default);
                        if (null == mesh)
                        {
                            continue;
                        }
                        if (pick_context.PickFrustumTest(mesh,
                            PickContext.MeshPickStyle.ShadedModePicking,
                            out hitPoint,
                            out depth,
                            out distance,
                            out hitFlag,
                            out hitIndex))
                        {

                        }
                    }
                }

                if (rhinoObject is BrepObject)
                {
                    BrepObject brepObject = rhinoObject as BrepObject;
                    Brep brep = brepObject.Geometry as Brep;
                    foreach (Surface surface in brep.Surfaces)
                    {
                        NurbsSurface nurbsSurface = surface as NurbsSurface;
                    }
                    foreach (BrepFace face in brep.Faces)
                    {
                        Mesh mesh = face.GetMesh(MeshType.Default);
                        if (null == mesh)
                        {
                            continue;
                        }
                        if (pick_context.PickFrustumTest(mesh,
                            PickContext.MeshPickStyle.ShadedModePicking,
                            out hitPoint,
                            out depth,
                            out distance,
                            out hitFlag,
                            out hitIndex))
                        {
                            if (maxDepth < depth)
                            {
                                maxDepth = depth;
                                selectedFace = face;
                                HitPoint = hitPoint;
                            }

                            if(selectedFace == face)
                            {
                                HitPoint = hitPoint;
                            }
                        }
                    }
                }
            }

            Face = selectedFace;
            //if (null != selectedFace && hitPoint != Point3d.Unset)
            //{
               
            //}
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            if(null == Face)
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

            e.Display.DrawPoint(HitPoint);
        }
    }
}
