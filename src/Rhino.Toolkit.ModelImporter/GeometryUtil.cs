using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Toolkit.ModelImporter
{
    public class GeometryUtil
    {
        File3dm File3dm { get; set; }

        public void TraverseGeometry(File3dm file3dm, 
            IList<GeometryBase> drawableGeometries,
            out BoundingBox boundingBox)
        {
            File3dm = file3dm;
            File3dmInstanceDefinitionTable instanceDefinitions = file3dm.AllInstanceDefinitions;
            IList<GeometryBase> normalGeometries = new List<GeometryBase>();
            BoundingBox? bbox = null;
            File3dmObjectTable file3DmObjects = file3dm.Objects;
            IDictionary<Guid, IList<GeometryBase>> insDefGeometries = new Dictionary<Guid, IList<GeometryBase>>();

            foreach (var instanceDefinition in instanceDefinitions)
            {
                IList<GeometryBase> geometries = new List<GeometryBase>();
                var objIds = instanceDefinition.GetObjectIds();
                foreach(Guid objectId in objIds)
                {
                    File3dmObject file3dmObject = file3DmObjects.FindId(objectId);
                    if(file3dmObject.ComponentType != ModelComponentType.ModelGeometry)
                    {
                        continue;
                    }
                    geometries.Add(file3dmObject.Geometry);
                }
                insDefGeometries.Add(instanceDefinition.Id, geometries);
            }
            

            foreach (var obj in file3DmObjects)
            {
                if (obj.ComponentType != ModelComponentType.ModelGeometry ||
                    obj.Attributes.IsInstanceDefinitionObject)
                {
                    continue;
                }

                if(obj.Geometry is InstanceReferenceGeometry)
                {
                    InstanceReferenceGeometry insRefGeometry = obj.Geometry as InstanceReferenceGeometry;
                    Transform transform = insRefGeometry.Xform;
                    if(!insDefGeometries.ContainsKey(insRefGeometry.ParentIdefId))
                    {
                        continue;
                    }
                    IList<GeometryBase> geometries = insDefGeometries[insRefGeometry.ParentIdefId];
                    foreach(var geometry in geometries)
                    {
                        var duplicatedGeometry = geometry.Duplicate();
                        duplicatedGeometry.Transform(transform);
                        drawableGeometries.Add(duplicatedGeometry);
                        if (!bbox.HasValue)
                        {
                            bbox = duplicatedGeometry.GetBoundingBox(false);
                        }
                        else
                        {
                            bbox.Value.Union(duplicatedGeometry.GetBoundingBox(false));
                        }
                    }
                }
                else
                {
                    drawableGeometries.Add(obj.Geometry);
                    if (!bbox.HasValue)
                    {
                        bbox = obj.Geometry.GetBoundingBox(false);
                    }
                    else
                    {
                        bbox.Value.Union(obj.Geometry.GetBoundingBox(false));
                    }
                }
            }

            boundingBox = bbox.Value;
        }

        public void AddToDocument(RhinoDoc rhinoDoc, Transform transform)
        {
            IDictionary<Guid, int> insDefId2IntMap = new Dictionary<Guid, int>();
            foreach(var insDef in File3dm.AllInstanceDefinitions)
            {
                InstanceDefinition foundInsDef = rhinoDoc.InstanceDefinitions.Find(insDef.Name);
                if (null != foundInsDef)
                {
                    insDefId2IntMap.Add(insDef.Id, foundInsDef.Index);
                    continue;
                }
                IList<GeometryBase> geometries = new List<GeometryBase>();
                IList<ObjectAttributes> attributesCollection = new List<ObjectAttributes>();
                var objIds = insDef.GetObjectIds();
                foreach (Guid objectId in objIds)
                {
                    File3dmObject file3dmObject = File3dm.Objects.FindId(objectId);
                    if (file3dmObject.ComponentType != ModelComponentType.ModelGeometry)
                    {
                        continue;
                    }
                    geometries.Add(file3dmObject.Geometry);
                    attributesCollection.Add(file3dmObject.Attributes);
                }
                int index = rhinoDoc.InstanceDefinitions.Add(insDef.Name, insDef.Description, new Point3d(0,0,0), geometries, attributesCollection);
                insDefId2IntMap.Add(insDef.Id, index);
            }

            foreach (var obj in File3dm.Objects)
            {
                if (obj.ComponentType != ModelComponentType.ModelGeometry ||
                    obj.Attributes.IsInstanceDefinitionObject)
                {
                    continue;
                }

                if(obj.Geometry is InstanceReferenceGeometry)
                {
                    InstanceReferenceGeometry insDefGeometry = obj.Geometry as InstanceReferenceGeometry;
                    if(!insDefId2IntMap.ContainsKey(insDefGeometry.ParentIdefId))
                    {
                        continue;
                    }

                    rhinoDoc.Objects.AddInstanceObject(insDefId2IntMap[insDefGeometry.ParentIdefId], transform);
                }
                else
                {
                    GeometryBase geometry = obj.Geometry;
                    geometry.Transform(transform);
                    rhinoDoc.Objects.Add(geometry);
                }
                
            }
        }
    }
}
