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
            GetPointAndPlane getPointAndFace = new GetPointAndPlane();
            getPointAndFace.SetCommandPrompt("Select rectangle's first point");
            GetResult result = getPointAndFace.Get();
            Plane plane = getPointAndFace.GetPlane();
            if (null == plane)
            {
                return Result.Failure;
            }
            getPointAndFace.View().ActiveViewport.SetConstructionPlane(plane);

            GetRectangle getRectangle = new GetRectangle(plane, getPointAndFace.HitPoint.Value);
            getRectangle.SetCommandPrompt("Select second point");
            result = getRectangle.Get();
            Point3d secondPoint = getRectangle.Point();

            Rectangle3d rectangle = new Rectangle3d(plane, getPointAndFace.HitPoint.Value, secondPoint);
            doc.Objects.AddRectangle(rectangle);

            return Result.Success;
        }
    }
}
