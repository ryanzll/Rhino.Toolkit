using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Toolkit.Common.Geometry;
using System;
using System.Collections.Generic;

namespace Rhino.Toolkit.RhinoSU
{
    public class PickClosedCurveAndFace : BaseGeometryWalker
    {
        GeometryParser GeometryParser { get; set; }

        PickContext PickContext { get; set; }

        public Point3d? SelectedPoint { get; set; }

        public BrepFace SelectedBrepFace { get; set; }

        public Mesh SelectedMesh { get; set; }

        public Vector3f? MeshFaceNormal { get; set; }

        double MaxDepth { get; set; }

        public Guid? SelectedObjectId { get; set; }

        Guid? CurObjectId { get; set; }

        public PickClosedCurveAndFace()
        {
            GeometryParser = new GeometryParser(this);
        }

        public void Update(PickContext pickContext, IList<RhinoObject> rhinoObjects)
        {
            PickContext = pickContext;
            GeometryParser.Parse(rhinoObjects);
        }

        public override void Reset()
        {
            MaxDepth = double.NegativeInfinity;
            SelectedPoint = null;
            SelectedBrepFace = null;
            SelectedMesh = null;
            MeshFaceNormal = null;
            CurObjectId = null;
        }

        public override void EnterObject(RhinoObject rhinoObject)
        {
            CurObjectId = rhinoObject.Id;
        }

        public override void EnterExtrusion(Extrusion extrusion)
        {
            double t;
            double depth;
            double distance;
            Point3d hitPoint = Point3d.Unset;
            PickContext.MeshHitFlag hitFlag;
            int hitIndex;

            Mesh mesh = extrusion.GetMesh(MeshType.Any);

            for(int meshFaceIndex = 0; meshFaceIndex < mesh.Faces.Count; meshFaceIndex ++)
            {
                var meshFace = mesh.Faces[meshFaceIndex];
                var meshFaceNormal = mesh.FaceNormals[meshFaceIndex];
                int vertexCount = 3;
                if(meshFace.IsQuad)
                {
                    vertexCount = 4;
                }
                Curve[] curves = new Curve[vertexCount];
                PolylineCurve polylineCurve = null;
                for (int vertextIndex = 0; vertextIndex < vertexCount - 1; vertextIndex++)
                {
                    polylineCurve = new PolylineCurve(new Point3d[] { mesh.Vertices[meshFace[vertextIndex]], mesh.Vertices[meshFace[vertextIndex + 1]] });
                    curves[vertextIndex] = polylineCurve;
                }
                polylineCurve = new PolylineCurve(new Point3d[] { mesh.Vertices[meshFace[vertexCount - 1]], mesh.Vertices[meshFace[0]] });
                curves[vertexCount - 1] = polylineCurve;

                Mesh newSubMesh = Mesh.CreateFromLines(curves, 4, 0.0001);
                
                if (PickContext.PickFrustumTest(newSubMesh,
                PickContext.MeshPickStyle.ShadedModePicking,
                out hitPoint,
                out depth,
                out distance,
                out hitFlag,
                out hitIndex))
                {
                    if (MaxDepth < depth)
                    {
                        MaxDepth = depth;
                        SelectedMesh = newSubMesh;
                        MeshFaceNormal = meshFaceNormal;
                        SelectedPoint = hitPoint;
                    }

                    if (SelectedMesh == newSubMesh)
                    {
                        SelectedPoint = hitPoint;
                    }
                }
            }
        }

        public override void EnterBrepFace(BrepFace brepFace)
        {
            double t;
            double depth;
            double distance;
            Point3d hitPoint = Point3d.Unset;
            PickContext.MeshHitFlag hitFlag;
            int hitIndex;

            Mesh mesh = brepFace.GetMesh(MeshType.Any);
            if (null == mesh)
            {
                return;
            }
            if (PickContext.PickFrustumTest(mesh,
                PickContext.MeshPickStyle.ShadedModePicking,
                out hitPoint,
                out depth,
                out distance,
                out hitFlag,
                out hitIndex))
            {
                if (MaxDepth < depth)
                {
                    MaxDepth = depth;
                    SelectedBrepFace = brepFace;
                    SelectedPoint = hitPoint;
                    SelectedObjectId = CurObjectId;
                    SelectedMesh = mesh;
                }

                if (null == SelectedObjectId ||
                    SelectedBrepFace == brepFace ||
                    SelectedBrepFace.Id == brepFace.Id)
                {
                    SelectedPoint = hitPoint;
                }
            }
        }

        public override void EnterPolylineCurve(PolylineCurve polylineCurve)
        {
            if(!polylineCurve.IsClosed || !polylineCurve.IsPlanar())
            {
                return;
            }
            double t;
            double depth;
            double distance;
            Point3d hitPoint = Point3d.Unset;
            PickContext.MeshHitFlag hitFlag;
            int hitIndex;

            if (polylineCurve.IsPolyline())
            {
                Polyline polyline = polylineCurve.ToPolyline();
                for (int lineIndex = 0; lineIndex < polyline.Count; lineIndex++)
                {
                    Line line = polyline.SegmentAt(lineIndex);
                    if (PickContext.PickFrustumTest(line,
                                                out t,
                                                out depth,
                                                out distance))
                    {
                        if (MaxDepth < depth)
                        {
                            MaxDepth = depth;
                            SelectedPoint = hitPoint;
                        }

                        //if (SelectedLine == line)
                        //{
                        //    SelectedPoint = hitPoint;
                        //}
                    }
                }
            }
            
        }
    }
}
