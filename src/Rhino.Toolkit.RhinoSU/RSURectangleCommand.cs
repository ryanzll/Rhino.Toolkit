using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;

namespace Rhino.Toolkit.RhinoSU
{
    public class RSURectangleCommand : Command
    {
        public RSURectangleCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static RSURectangleCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "RSURectangle";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetPointAndFace getPointAndFace = new GetPointAndFace();
            getPointAndFace.SetCommandPrompt("Select rectangle's first point");
            GetResult result = getPointAndFace.Get();
            
            if(null == getPointAndFace.Face)
            {
                return Result.Failure;
            }

            Plane plane; 
            if(!getPointAndFace.Face.TryGetPlane(out plane))
            {
                double u;
                double v;
                getPointAndFace.Face.ClosestPoint(getPointAndFace.HitPoint, out u, out v);
                plane = new Plane(getPointAndFace.Face.PointAt(u, v), getPointAndFace.Face.NormalAt(u, v));
            }
            getPointAndFace.View().ActiveViewport.SetConstructionPlane(plane);

            GetRectangle getRectangle = new GetRectangle(plane, getPointAndFace.HitPoint);
            getRectangle.SetCommandPrompt("Select second point");
            result = getRectangle.Get();

            return Result.Success;
        }
    }
}
