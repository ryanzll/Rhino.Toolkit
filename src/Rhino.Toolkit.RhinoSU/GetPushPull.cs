using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace Rhino.Toolkit.RhinoSU
{
    public class GetPushPull:GetPoint
    {
        BrepFace BrepFace { get; set; }

        Point3d StartPoint { get; set; }

        public GetPushPull(BrepFace brepFace, Point3d startPoint)
        {
            BrepFace = brepFace;
            StartPoint = startPoint;
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            //PolylineCurve polylineCurve = new PolylineCurve()
            //Surface surface = BrepFace.CreateExtrusion();
            //Rectangle3d rectangle = new Rectangle3d(Plane, FirstPoint, Point());
            //e.Display.DrawPolyline(rectangle.ToPolyline(), System.Drawing.Color.Red);
        }
    }
}
