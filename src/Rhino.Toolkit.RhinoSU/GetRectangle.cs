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
    public class GetRectangle:GetPoint
    {
        Point3d FirstPoint { get; set; }

        Plane Plane { get; set; }

        public GetRectangle(Plane plane, Point3d firstPoint)
        {
            Plane = plane;
            FirstPoint = firstPoint;
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            Rectangle3d rectangle = new Rectangle3d(Plane, FirstPoint, Point());
            e.Display.DrawPolyline(rectangle.ToPolyline(), System.Drawing.Color.Red);
        }
    }
}
