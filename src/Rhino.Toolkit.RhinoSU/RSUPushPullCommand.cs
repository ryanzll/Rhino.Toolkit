using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Rhino.Toolkit.RhinoSU
{
    public class RSUPushPullCommand : Command
    {
        public RSUPushPullCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static RSUPushPullCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "RSURectangle";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetClosedCurveAndFace getClosedCurveAndFace = new GetClosedCurveAndFace();
            getClosedCurveAndFace.SetCommandPrompt("Select rectangle's first point");
            GetResult result = getClosedCurveAndFace.Get();

            //BrepFace brepFace = getClosedCurveAndFace.Face;
            //if(null == brepFace || null == brepFace.Id)
            //{
            //    return Result.Failure;
            //}
            if(getClosedCurveAndFace.SelectedObjectId.HasValue)
            {
                doc.Objects.Select(getClosedCurveAndFace.SelectedObjectId.Value);
            }
            RhinoApp.RunScript("_ExtrudeSrf", false);
            
            return Result.Success;
        }
    }
}
