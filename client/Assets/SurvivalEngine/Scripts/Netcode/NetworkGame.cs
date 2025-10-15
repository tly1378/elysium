using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace NetcodePlus
{
    /// <summary>
    /// Network manager that get instantiated in the game scene only, unlike TheNetwork, it doesn't persist between scenes
    /// </summary>
    /// 
    [DefaultExecutionOrder(-5)]
    public class NetworkGame : MonoBehaviour
    {
        private NetworkSpawner spawner;
        private NetworkChat chat;
        private float update_timer = 0f;
        private float status_timer = 0f;
        private float total_timer = 0f;
        private bool keep_valid = false;

        private const float spawn_refresh_rate = 0.5f; //In seconds, interval at which SNetworkObjects are spawned/despawned
        private const float status_refresh_rate = 10f; //Every 10 seconds, send refresh to lobby to keep the game listed
        private const int optim_loop_segments = 10; //Will split the optimization loop across 10 frames instead of processing all in 1 frame and causing spikes
        private const float wait_time_min = 30f; //Minimum wait time before it can self-shutdown

        private Dictionary<Vector2Int, NetworkOptimizationZone> optim_zones = new Dictionary<Vector2Int, NetworkOptimizationZone>();
        private LinkedList<NetworkOptimizationZone> all_zones = new LinkedList<NetworkOptimizationZone>();
        private LinkedListNode<NetworkOptimizationZone> node_static;
        private LinkedListNode<SNetworkOptimizer> node_dynamic;

        private List<SNetworkOptimizer> temp_list = new List<SNetworkOptimizer>();

        private static NetworkGame instance;

        private void Awake()
        {
            instance = this;
            spawner = new NetworkSpawner();
            chat = new NetworkChat();
        }

        private void Start()
        {
            TheNetwork network = TheNetwork.Get();
            network.onTick += TickUpdate;

            Messaging.ListenMsg("action", ReceiveAction);
            Messaging.ListenMsg("variable", ReceiveVariable);
            Messaging.ListenMsg("spawn", ReceiveSpawnList);
            Messaging.ListenMsg("despawn", ReceiveDespawnList);
            Messaging.ListenMsg("change_owner", ReceiveChangeList);
            InitLobbyKeep();
            spawner.Init();
            chat.Init();
        }

        private void OnDestroy()
        {
            instance = null;

            TheNetwork network = TheNetwork.GetIfExist();
            if (network != null)
            {
                network.onTick -= TickUpdate;

                Messaging.UnListenMsg("action");
                Messaging.UnListenMsg("variable");
                Messaging.UnListenMsg("spawn");
                Messaging.UnListenMsg("despawn");
                Messaging.UnListenMsg("change_owner");
                chat.Clear();
            }

            SNetworkActions.ClearAll();
            SNetworkVariableBase.ClearAll(); 
            SNetworkObject.ClearAll();
            SNetworkOptimizer.ClearAll();
            NetworkAction.ClearAll();
            optim_zones.Clear();
            all_zones.Clear();
        }

        private void Update()
        {
            UpdateVisibility();
            UpdateStatus();
        }

        private void TickUpdate()
        {
            if (IsServer && IsReady)
            {
                spawner.TickUpdate();
            }

            if (IsReady)
            {
                SNetworkVariableBase.TickAll();
            }
        }

        private void InitLobbyKeep()
        {
            if (!IsOnline)
                return; //Not online

            if (NetworkData.Get().lobby_type != LobbyType.Dedicated)
                return; //Not dedicated lobby

            WebClient client = WebClient.Get();
            client?.SetDefaultUrl(NetworkData.Get().lobby_host, NetworkData.Get().lobby_port);

            LobbyGame game = TheNetwork.Get().GetLobbyGame(); //Make sure we are playing a lobby game
            if (client != null && game != null)
            {
                LobbyPlayer player = game.GetPlayer(TheNetwork.Get().UserID);
                if (player != null)
                    client.SetClientID(player.client_id);

                keep_valid = player != null || IsServer;
            }
        }

        //Spawn Despawn objects based on distance
        private void UpdateVisibility()
        {
            if (!IsServer || !IsReady)
                return;

            //Slow update
            update_timer += Time.deltaTime;
            if (update_timer < spawn_refresh_rate)
                return;

            update_timer = 0f;

            //Turn off active
            UnityEngine.Profiling.Profiler.BeginSample("Optimization Turn Off Active");
            temp_list.Clear();
            foreach (SNetworkOptimizer obj in SNetworkOptimizer.GetAllActive())
            {
                if (obj != null)
                {
                    float dist = SNetworkPlayer.GetNearestDistance(obj.GetPos());
                    if (dist > obj.active_range)
                        temp_list.Add(obj);
                }
            }

            foreach (SNetworkOptimizer obj in temp_list)
                obj.SetActive(false);
            UnityEngine.Profiling.Profiler.EndSample();

            //Optimization Loop (dynamic)
            UnityEngine.Profiling.Profiler.BeginSample("Optimization Loop Dynamic");
            LinkedList<SNetworkOptimizer> objs = SNetworkOptimizer.GetAllDynamic();
            int nb_to_do = objs.Count / optim_loop_segments + Mathf.Min(objs.Count, 1);
            if (node_dynamic == null || node_dynamic.Value == null || node_dynamic.List != objs)
                node_dynamic = objs.First;

            for (int i = 0; i < nb_to_do; i++)
            {
                SNetworkOptimizer obj = node_dynamic.Value;
                if (obj != null && obj.enabled)
                {
                    float dist = SNetworkPlayer.GetNearestDistance(obj.GetPos());
                    obj.SetActive(dist < obj.active_range);
                }
                node_dynamic = node_dynamic.Next != null ? node_dynamic.Next : objs.First;
            }
            UnityEngine.Profiling.Profiler.EndSample();

            //Optimization Loop (static)
            UnityEngine.Profiling.Profiler.BeginSample("Optimization Loop Static");
            nb_to_do = all_zones.Count / optim_loop_segments + Mathf.Min(all_zones.Count, 1);
            if (node_static == null || node_static.Value == null || node_static.List != all_zones)
                node_static = all_zones.First;

            for (int i = 0; i < nb_to_do; i++)
            {
                NetworkOptimizationZone zone = node_static.Value;
                Vector3 center = NetworkOptimizationZone.CoordToZonePos(zone.coord);
                float cdist = SNetworkPlayer.GetNearestDistance(center);
                if (cdist < NetworkOptimizationZone.optimization_zone_size)
                {
                    foreach (KeyValuePair<ulong, SNetworkOptimizer> opair in zone.GetObjects())
                    {
                        SNetworkOptimizer obj = opair.Value;
                        if (obj != null && obj.enabled)
                        {
                            float dist = SNetworkPlayer.GetNearestDistance(obj.GetPos());
                            obj.SetActive(dist < obj.active_range);
                        }
                    }
                }

                node_static = node_static.Next != null ? node_static.Next : all_zones.First;
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
        
        private void UpdateStatus()
        {
            //Slow update
            total_timer += Time.deltaTime;
            status_timer += Time.deltaTime;
            if (status_timer < status_refresh_rate)
                return;

            status_timer = 0f;

            KeepAliveLobby();
            KeepAliveLobbyList();
            CheckForShutdown();
        }

        //Send a keep alive to the lobby, to keep the current game listed on the lobby (otherwise it will get deleted if inactivity)
        private async void KeepAliveLobby()
        {
            if (!keep_valid || IsServer)
                return; //Client only

            WebClient client = WebClient.Get();
            if (client != null)
            {
                await client.Send("keep");
            }
        }

        //Send a keep alive to the lobby, to keep the current game listed on the lobby (otherwise it will get deleted if inactivity)
        //Servers sends a bit more info (like list of connected players)
        private async void KeepAliveLobbyList()
        {
            if (!keep_valid || !IsServer)
                return; //Server only

            WebClient web = WebClient.Get();
            LobbyGame game = TheNetwork.Get().GetLobbyGame();
            if (web != null && game != null)
            {
                int index = 0;
                string[] list = new string[TheNetwork.Get().CountClients()];
                foreach (KeyValuePair<ulong, ClientData> pair in TheNetwork.Get().GetClientsData())
                {
                    ClientData client = pair.Value;
                    if (client != null && index < list.Length)
                    {
                        list[index] = client.user_id;
                        index++;
                    }
                }

                if (list.Length > 0)
                {
                    KeepMsg msg = new KeepMsg(game.game_id, list);
                    await web.Send("keep_list", msg);
                }
            }
        }

        //Check if no one is connected, if so shutdown
        private void CheckForShutdown()
        {
            LobbyGame game = TheNetwork.Get().GetLobbyGame(); //Make sure we are playing a dedicated lobby game
            if (IsServer && game != null && game.type == ServerType.DedicatedServer && !game.permanent)
            {
                int connected = TheNetwork.Get().CountClients();
                bool can_shutdown = total_timer > wait_time_min;
                if (can_shutdown && connected == 0)
                {
                    Application.Quit();
                }
            }
        }

        private void ReceiveAction(ulong client_id, FastBufferReader reader)
        {
            if (client_id != TheNetwork.Get().ClientID)
            {
                reader.ReadValueSafe(out ulong object_id);
                reader.ReadValueSafe(out ushort behaviour_id);
                reader.ReadValueSafe(out ushort type);
                reader.ReadValueSafe(out ushort delivery);
                SNetworkActions handler = SNetworkActions.GetHandler(object_id, behaviour_id);
                handler?.ReceiveAction(client_id, type, reader, (NetworkDelivery)delivery);
            }
        }

        private void ReceiveVariable(ulong client_id, FastBufferReader reader)
        {
            if (client_id != TheNetwork.Get().ClientID)
            {
                reader.ReadValueSafe(out ulong object_id);
                reader.ReadValueSafe(out ushort behaviour_id);
                reader.ReadValueSafe(out ushort variable_id);
                reader.ReadValueSafe(out ushort delivery);
                SNetworkVariableBase handler = SNetworkVariableBase.GetVariable(object_id, behaviour_id, variable_id);
                handler?.ReceiveVariable(client_id, reader, (NetworkDelivery)delivery);
            }
        }

        private void ReceiveSpawnList(ulong client_id, FastBufferReader reader)
        {
            //Is not server and the sender is the server
            if (!IsServer && client_id == TheNetwork.Get().ServerID)
            {
                reader.ReadNetworkSerializable(out NetSpawnList list);
                foreach (NetSpawnData data in list.data)
                {
                    spawner.SpawnClient(data);
                }
            }
        }

        private void ReceiveDespawnList(ulong client_id, FastBufferReader reader)
        {
            //Is not server and the sender is the server
            if (!IsServer && client_id == TheNetwork.Get().ServerID)
            {
                reader.ReadNetworkSerializable(out NetDespawnList list);
                foreach (NetDespawnData data in list.data)
                {
                    spawner.DespawnClient(data.network_id, data.destroy);
                }
            }
        }

        private void ReceiveChangeList(ulong client_id, FastBufferReader reader)
        {
            //Is not server and the sender is the server
            if (!IsServer && client_id == TheNetwork.Get().ServerID)
            {
                reader.ReadNetworkSerializable(out NetChangeList list);
                foreach (NetChangeData data in list.data)
                {
                    spawner.ChangeOwnerClient(data.network_id, data.owner);
                }
            }
        }

        public NetworkOptimizationZone GetZone(Vector3 wpos, bool create = false)
        {
            Vector2Int coord = NetworkOptimizationZone.ZonePosToCoord(wpos);
            return GetZone(coord, create);
        }

        public NetworkOptimizationZone GetZone(Vector2Int coord, bool create = false)
        {
            if (optim_zones.ContainsKey(coord))
                return optim_zones[coord];
            if (create)
            {
                NetworkOptimizationZone zone = new NetworkOptimizationZone();
                zone.coord = coord;
                optim_zones[coord] = zone;
                all_zones.AddLast(zone);
                return zone;
            }
            return null;
        }

        public NetworkSpawner Spawner { get { return spawner; } }
        public NetworkChat Chat { get { return chat; } }
        public NetworkMessaging Messaging { get { return TheNetwork.Get().Messaging; } }

        public bool IsOnline { get { return TheNetwork.Get().IsOnline; } }
        public bool IsServer { get { return TheNetwork.Get().IsServer; } }
        public bool IsClient { get { return TheNetwork.Get().IsClient; } }
        public bool IsReady { get { return TheNetwork.Get().IsReady(); } }

        public static NetworkGame Get()
        {
            if (instance == null)
                instance = FindObjectOfType<NetworkGame>();
            return instance;
        }
    }
}
