using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Toolkit.ModelImporter
{
    internal class GetInsertPoint : GetPoint
    {
        IList<GeometryBase> Geometries { get; set; }

        BoundingBox BoundingBox { get; set; }

        DisplayMaterial Material { get; set; }

        Point3d PrePoint { get; set; }

        public GetInsertPoint(IList<GeometryBase> geometries, BoundingBox boundingBox)
        {
            Geometries = geometries;
            BoundingBox = boundingBox;
            Material = new DisplayMaterial();
            Material.Diffuse = System.Drawing.Color.OrangeRed;
            Material.IsTwoSided = true;
            Material.Shine = 0.8;
            PrePoint = new Point3d(0, 0, 0);

            SetDefaultColor(System.Drawing.Color.OrangeRed);
        }

        protected override void OnMouseMove(GetPointMouseEventArgs e)
        {
            Transform transform = Transform.Translation(e.Point.X - PrePoint.X,
                e.Point.Y - PrePoint.Y, e.Point.Z - PrePoint.Z);
            PrePoint = e.Point;

            foreach (var geometry in Geometries)
            {
                bool result = geometry.Transform(transform);
            }

            BoundingBox = transform.TransformBoundingBox(BoundingBox);
            //bool result1 = BoundingBox.Transform(transform);
            base.OnMouseMove(e);
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            foreach (var geometry in Geometries)
            {
                switch (geometry.ObjectType)
                {
                    case ObjectType.Extrusion:
                        e.Display.DrawExtrusionWires(geometry as Extrusion, Color());
                        break;
                    case ObjectType.Mesh:
                    case ObjectType.MeshEdge:
                    case ObjectType.MeshFace:
                    case ObjectType.MeshVertex:
                        e.Display.DrawMeshShaded(geometry as Mesh, Material);
                        break;
                    case ObjectType.Brep:
                        e.Display.DrawBrepWires(geometry as Brep, System.Drawing.Color.OrangeRed);
                        break;
                }
            }
            e.Display.DrawBox(BoundingBox, System.Drawing.Color.OrangeRed);
            base.OnDynamicDraw(e);
        }
    }
}
