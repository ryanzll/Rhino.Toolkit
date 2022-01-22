using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;
using System;
using System.Collections.Generic;

namespace Rhino.Toolkit.ModelImporter
{
    public class ModelImporterCommand : Command
    {
        public ModelImporterCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static ModelImporterCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "ModelImporter";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(!openFileDialog.ShowOpenDialog())
            {
                return Result.Cancel;
            }

            File3dm file3dm = File3dm.Read(openFileDialog.FileName);
            GeometryUtil geometryUtil = new GeometryUtil();
            BoundingBox boundingBox;
            IList<GeometryBase> geometries = new List<GeometryBase>();
            geometryUtil.TraverseGeometry(file3dm, geometries, out boundingBox);

            GetInsertPoint getInsertPoint = new GetInsertPoint(geometries, boundingBox);
            getInsertPoint.SetCommandPrompt("Select a position");
            GetResult result = getInsertPoint.Get();
            if (getInsertPoint.CommandResult() != Result.Success)
                return getInsertPoint.CommandResult();

            Point3d point = getInsertPoint.Point();
            Transform transform = Transform.Translation(point.X, point.Y, point.Z);
            geometryUtil.AddToDocument(doc, transform);
            doc.Views.Redraw();
            return Result.Success;
        }
    }
}
