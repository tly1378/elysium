using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetcodePlus
{
    /// <summary>
    /// Object will be spawned/despawned based on distance to SNetworkPlayer
    /// </summary>

    [RequireComponent(typeof(SNetworkObject))]
    public class SNetworkOptimizer : MonoBehaviour
    {
        [Header("Optimization")]
        public bool static_object = false; //Turn on for objects that never move, for optimization
        public float active_range = 50f; //If farther than this, will be disabled for optim
        public bool always_run_scripts = false; //Set to true to have other scripts still run when this one is not active
        public bool turn_off_gameobject = false; //Set to true if you want the gameobject to be SetActive(false) when far away

        private SNetworkObject nobj;
        private Transform transf;
        private Rigidbody rigid;
        private bool is_active = true;

        private List<MonoBehaviour> scripts = new List<MonoBehaviour>();
        private List<Animator> animators = new List<Animator>();

        private static LinkedList<SNetworkOptimizer> active_list = new LinkedList<SNetworkOptimizer>();
        private LinkedListNode<SNetworkOptimizer> active_node;

        private static LinkedList<SNetworkOptimizer> opt_list_static = new LinkedList<SNetworkOptimizer>();
        private static LinkedList<SNetworkOptimizer> opt_list_dynamic = new LinkedList<SNetworkOptimizer>();
        private LinkedListNode<SNetworkOptimizer> node; //Reference to node so that Remove() function is O(1)

        void Awake()
        {
            if (static_object)
                node = opt_list_static.AddLast(this);
            else
                node = opt_list_dynamic.AddLast(this);

            active_node = new LinkedListNode<SNetworkOptimizer>(this);
            transf = transform;
            nobj = GetComponent<SNetworkObject>();
            rigid = GetComponentInChildren<Rigidbody>(true);
            scripts.AddRange(GetComponentsInChildren<MonoBehaviour>(true));
            animators.AddRange(GetComponentsInChildren<Animator>(true));
        }

        void OnDestroy()
        {
            if(gameObject.scene.isLoaded)
            {
                RemoveFromZones();

                if (node.List != null)
                {
                    if (static_object)
                        opt_list_static.Remove(node);
                    else
                        opt_list_dynamic.Remove(node);
                }

                if (active_node != null && active_node.List != null)
                    active_list.Remove(active_node);
            }
        }

        public void SetActive(bool visible)
        {
            if (is_active != visible)
            {
                is_active = visible;

                if (!always_run_scripts && !turn_off_gameobject)
                {
                    if (rigid != null)
                    {
                        rigid.detectCollisions = visible;
                        if (!rigid.isKinematic && !visible)
                            rigid.linearVelocity = Vector3.zero;
                    }

                    foreach (MonoBehaviour script in scripts)
                    {
                        if (script != null && script != this && script != nobj)
                            script.enabled = visible;
                    }
                }

                if (!turn_off_gameobject || always_run_scripts)
                {
                    foreach (Animator anim in animators)
                    {
                        if (anim != null)
                            anim.enabled = visible;
                    }
                }

                if (visible)
                    active_list.AddLast(active_node);
                else if (active_node.List != null)
                    active_list.Remove(active_node);

                if (visible)
                    NetObject.Spawn();
                else
                    NetObject.Despawn();

                if (turn_off_gameobject)
                    gameObject.SetActive(visible);
            }
        }

        public void AddToZone()
        {
            if (static_object)
            {
                NetworkOptimizationZone zone = NetworkGame.Get().GetZone(transform.position, true);
                zone.AddObject(this);
            }
        }

        public void RemoveFromZones()
        {
            if (static_object)
            {
                NetworkOptimizationZone zone = NetworkGame.Get().GetZone(transform.position);
                if (zone != null)
                    zone.RemoveObject(this);
            }
        }

        public Vector3 GetPos()
        {
            return transf.position;
        }

        public bool IsActive()
        {
            return is_active;
        }

        public SNetworkObject NetObject { get { return nobj; } }

        public static void ClearAll()
        {
            opt_list_static.Clear();
            opt_list_dynamic.Clear();
            active_list.Clear();
        }

        public static LinkedList<SNetworkOptimizer> GetAllActive()
        {
            return active_list;
        }

        public static LinkedList<SNetworkOptimizer> GetAllStatic()
        {
            return opt_list_static;
        }

        public static LinkedList<SNetworkOptimizer> GetAllDynamic()
        {
            return opt_list_dynamic;
        }
    }
}
