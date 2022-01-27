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
            File3dmObjectTable file3DmObjects = file3dm.Objects;
            IDictionary<Guid, IList<GeometryBase>> insDefGeometries = new Dictionary<Guid, IList<GeometryBase>>();
            boundingBox = BoundingBox.Empty;

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

                if (obj.Geometry is InstanceReferenceGeometry)
                {
                    InstanceReferenceGeometry insRefGeometry = obj.Geometry as InstanceReferenceGeometry;
                    Transform transform = Transform.Identity;

                    BoundingBox insRefGeometryBBox;
                    TraverseInsRefGeometry(insRefGeometry, 
                        transform,
                        insDefGeometries, 
                        drawableGeometries, 
                        out insRefGeometryBBox);
                    UnionBBox(ref boundingBox, insRefGeometryBBox);    
                }
                else
                {
                    drawableGeometries.Add(obj.Geometry);
                    UnionBBox(ref boundingBox, obj.Geometry.GetBoundingBox(false));
                }
            }
        }

        private void TraverseInsRefGeometry(InstanceReferenceGeometry insRefGeometry,
            Transform transform,
            IDictionary<Guid, IList<GeometryBase>> insDefGeometries, 
            IList<GeometryBase> drawableGeometries,
            out BoundingBox boundingBox)
        {
            boundingBox = BoundingBox.Empty;
            Transform globalTransform = Transform.Multiply(transform, insRefGeometry.Xform);
            if (!insDefGeometries.ContainsKey(insRefGeometry.ParentIdefId))
            {
                return;
            }
            IList<GeometryBase> geometries = insDefGeometries[insRefGeometry.ParentIdefId];
            foreach (var geometry in geometries)
            {
                if(geometry is InstanceReferenceGeometry)
                {
                    BoundingBox subInsRefGeometryBBox;
                    TraverseInsRefGeometry(geometry as InstanceReferenceGeometry, globalTransform, insDefGeometries, drawableGeometries, out subInsRefGeometryBBox);
                    UnionBBox(ref boundingBox, subInsRefGeometryBBox);
                }
                else
                {
                    var duplicatedGeometry = geometry.Duplicate();
                    duplicatedGeometry.Transform(globalTransform);
                    drawableGeometries.Add(duplicatedGeometry);
                    UnionBBox(ref boundingBox, duplicatedGeometry.GetBoundingBox(false));
                }
            }
        }

        public void AddToDocument(RhinoDoc rhinoDoc, Transform transform)
        {
            IDictionary<Guid, int> insDefId2IntMap = new Dictionary<Guid, int>();
            foreach(var insDefGeometry in File3dm.AllInstanceDefinitions)
            {
                InstanceDefinition foundInsDef = rhinoDoc.InstanceDefinitions.Find(insDefGeometry.Name);
                if (null != foundInsDef)
                {
                    insDefId2IntMap.Add(insDefGeometry.Id, foundInsDef.Index);
                    continue;
                }
                IList<GeometryBase> geometries = new List<GeometryBase>();
                IList<ObjectAttributes> attributesCollection = new List<ObjectAttributes>();
                var objIds = insDefGeometry.GetObjectIds();
                foreach (Guid objectId in objIds)
                {
                    File3dmObject file3dmObject = File3dm.Objects.FindId(objectId);
                    if (file3dmObject.ComponentType != ModelComponentType.ModelGeometry)
                    {
                        continue;
                    }

                    if (file3dmObject.Geometry is InstanceReferenceGeometry)
                    {
                        InstanceReferenceGeometry insRefGeometry = file3dmObject.Geometry as InstanceReferenceGeometry;
                        Transform globalTransform = Transform.Multiply(transform, insRefGeometry.Xform);
                        AddInsDefinition(File3dm, geometries, attributesCollection, insRefGeometry.Xform, insRefGeometry);
                    }
                    else
                    {
                        geometries.Add(file3dmObject.Geometry);
                        attributesCollection.Add(file3dmObject.Attributes);
                    }
                }
                int index = rhinoDoc.InstanceDefinitions.Add(insDefGeometry.Name, insDefGeometry.Description, new Point3d(0,0,0), geometries, attributesCollection);
                insDefId2IntMap.Add(insDefGeometry.Id, index);
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
                    InstanceReferenceGeometry insRefGeometry = obj.Geometry as InstanceReferenceGeometry;
                    //AddInsRefGeometry(rhinoDoc, File3dm, insDefId2IntMap, transform, insRefGeometry);
                    rhinoDoc.Objects.AddInstanceObject(insDefId2IntMap[insRefGeometry.ParentIdefId], transform);
                }
                else
                {
                    GeometryBase geometry = obj.Geometry;
                    geometry.Transform(transform);
                    rhinoDoc.Objects.Add(geometry);
                }
                
            }
        }

        void AddInsRefGeometry(RhinoDoc rhinoDoc,
            File3dm file3dm,
            IDictionary<Guid, int> insDefId2IntMap,
            Transform globalTranform,
            InstanceReferenceGeometry insRefGeometry)
        {
            if (!insDefId2IntMap.ContainsKey(insRefGeometry.ParentIdefId))
            {
                return;
            }

            Transform transform = Transform.Multiply(globalTranform, insRefGeometry.Xform);
            InstanceDefinitionGeometry instanceDefinition = file3dm.AllInstanceDefinitions.FindId(insRefGeometry.ParentIdefId);
            foreach(var objectId in instanceDefinition.GetObjectIds())
            {
                File3dmObject file3dmObject = file3dm.Objects.FindId(objectId);
                if (file3dmObject.Geometry is InstanceReferenceGeometry)
                {
                    InstanceReferenceGeometry subInsDefGeometry = file3dmObject.Geometry as InstanceReferenceGeometry;
                    if (!insDefId2IntMap.ContainsKey(subInsDefGeometry.ParentIdefId))
                    {
                        continue;
                    }

                    rhinoDoc.Objects.AddInstanceObject(insDefId2IntMap[subInsDefGeometry.ParentIdefId], transform);
                    AddInsRefGeometry(rhinoDoc, file3dm, insDefId2IntMap, transform, subInsDefGeometry);
                }
                else
                {
                    GeometryBase geometry = file3dmObject.Geometry;
                    geometry.Transform(transform);
                    rhinoDoc.Objects.Add(geometry);
                }
            }
        }

        void AddInsDefinition(File3dm file3dm, IList<GeometryBase> geometries,
            IList<ObjectAttributes> attributesCollection,
            Transform transform,
            InstanceReferenceGeometry insRefGeometry)
        {
            InstanceDefinitionGeometry insDefGeometry = file3dm.AllInstanceDefinitions.FindId(insRefGeometry.ParentIdefId);
            var objIds = insDefGeometry.GetObjectIds();
            foreach (Guid objectId in objIds)
            {
                File3dmObject file3dmObject = File3dm.Objects.FindId(objectId);
                if (file3dmObject.ComponentType != ModelComponentType.ModelGeometry)
                {
                    continue;
                }

                if (file3dmObject.Geometry is InstanceReferenceGeometry)
                {
                    InstanceReferenceGeometry subInsRefGeometry = file3dmObject.Geometry as InstanceReferenceGeometry;
                    Transform globalTransform = Transform.Multiply(transform, subInsRefGeometry.Xform);
                    AddInsDefinition(file3dm, geometries, attributesCollection, globalTransform, subInsRefGeometry);
                }
                else
                {
                    geometries.Add(file3dmObject.Geometry);
                    attributesCollection.Add(file3dmObject.Attributes);
                }
            }
        }

        void UnionBBox(ref BoundingBox firstBBox, BoundingBox secondBBox)
        {
            if(!secondBBox.IsValid)
            {
                return;
            }

            if(firstBBox.IsValid)
            {
                firstBBox.Union(secondBBox);
            }
            else
            {
                firstBBox = secondBBox;
            }
        }
    }
}
