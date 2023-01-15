using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Vapok.Common.Managers.PieceManager
{
    [PublicAPI]
    public class SnapPointMaker
    {
        static SnapPointMaker()
        {
            _objectsToApplySnaps = new List<GameObject>();
        }
        private static List<GameObject> _objectsToApplySnaps;
        
        
        public static void AddObjectForSnapPoints(GameObject obj)
        {
            _objectsToApplySnaps.Add(obj);
        }
        public static void ApplySnapPoints()
        {
            foreach (var gameObject in _objectsToApplySnaps)
            {
                GrabVerticesAssignSnaps(gameObject);
            }
        }
        private static void GrabVerticesAssignSnaps(GameObject obj)
        {
            var vertices = GetColliderVertexPosRotated(obj);
            AttachSnapPoints(obj, vertices);
        }
        private static Vector3[] GetColliderVertexPosRotated(GameObject obj)
        {
            Vector3[] vertices = new Vector3[8];
            BoxCollider col = obj.GetComponentInChildren<BoxCollider>();
            if (col == null) return vertices;
            var trans = obj.transform;
            var min = col.center - col.size * 0.5f;
            var max = col.center + col.size * 0.5f;
            vertices[0] = trans.TransformPoint(new Vector3(min.x, min.y, min.z));
            vertices[1] = trans.TransformPoint(new Vector3(min.x, min.y, max.z));
            vertices[2] = trans.TransformPoint(new Vector3(min.x, max.y, min.z));
            vertices[3] = trans.TransformPoint(new Vector3(min.x, max.y, max.z));
            vertices[4] = trans.TransformPoint(new Vector3(max.x, min.y, min.z));
            vertices[5] = trans.TransformPoint(new Vector3(max.x, min.y, max.z));
            vertices[6] = trans.TransformPoint(new Vector3(max.x, max.y, min.z));
            vertices[7] = trans.TransformPoint(new Vector3(max.x, max.y, max.z));

            return vertices;
        }
        private static void AttachSnapPoints(GameObject objecttosnap, Vector3[] vector3S)
        {
            foreach (var vector in vector3S)
            {
                GameObject snappoint = new GameObject();
                snappoint.name = "_snappoint";
                snappoint.tag = "snappoint";
                snappoint.layer = 10;
                var temp = Object.Instantiate(snappoint, vector, Quaternion.identity, objecttosnap.transform);
                temp.SetActive(false);
            }
        }
        
    }
}