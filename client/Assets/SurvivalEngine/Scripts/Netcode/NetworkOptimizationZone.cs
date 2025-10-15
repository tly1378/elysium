using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetcodePlus
{

    public class NetworkOptimizationZone
    {
        public Vector2Int coord;
        private Dictionary<ulong, SNetworkOptimizer> object_list = new Dictionary<ulong, SNetworkOptimizer>();

        public const float optimization_zone_size = 100f;

        public void AddObject(SNetworkOptimizer obj)
        {
            object_list[obj.NetObject.network_id] = obj;
        }

        public void RemoveObject(SNetworkOptimizer obj)
        {
            object_list.Remove(obj.NetObject.network_id);
        }

        public Dictionary<ulong, SNetworkOptimizer> GetObjects()
        {
            return object_list;
        }

        public static Vector2Int ZonePosToCoord(Vector3 wpos)
        {
            float size = optimization_zone_size;
            int x = Mathf.RoundToInt(wpos.x / size);
            int y = Mathf.RoundToInt(wpos.z / size);
            return new Vector2Int(x, y);
        }

        public static Vector3 CoordToZonePos(Vector2Int coord)
        {
            float size = optimization_zone_size;
            float x = coord.x * size;
            float z = coord.y * size;
            return new Vector3(x, 0f, z);
        }
    }
}