using Rhino;
using Rhino.Commands;
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
            GetObject go = new GetObject();
            go.EnablePreSelect(true, false);
            go.GeometryFilter = DocObjects.ObjectType.Surface | DocObjects.ObjectType.MeshFace;
            go.SetCommandPrompt("Select first cornor of rectangle");
            var gr = go.GetMultiple(0, 1);
            if(gr == Input.GetResult.Nothing)
            {
                return Result.Nothing;
            }
            return Result.Success;
        }
    }
}
