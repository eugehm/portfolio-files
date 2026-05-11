using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    static NetworkManager instance;

    WebSocket web_socket;

    const string server_url = @"wss://eecs198.net:8198";

    ClientMultiplayerPayload current_local_player_payload;

    string local_client_id = "???????";

    public static float server_time = -1.0f;
    public static float GetServerTime()
    {
        return server_time;
    }

    static Dictionary<string, PlayerState> network_players = new Dictionary<string, PlayerState>();
    public static bool DoesPlayerExist(string id)
    {
        return network_players.ContainsKey(id);
    }
    public static PlayerState GetPlayerState(string id)
    {
        return network_players[id];
    }

    public static List<Vector3> GetAllNetworkPlayerPositions() // Does not include the local player.
    {
        if (instance == null)
            return new List<Vector3>();

        List<Vector3> result = new List<Vector3>();
        foreach(NetworkPlayerView view in instance.network_player_views.Values)
        {
            result.Add(view.GetPosition());
        }

        return result;
    }

    public static int GetNumberOfNetworkPlayers()
    {
        return network_players.Count;
    }

    Dictionary<string, NetworkPlayerView> network_player_views = new Dictionary<string, NetworkPlayerView>();

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ArborCoroutineManager.ArborStartCoroutine(MonitorNetwork());
    }

    float multiplayer_update_timer = 1.0f / 10.0f;

    private void Update()
    {
        server_time += Time.deltaTime;

        if (SceneManager.GetActiveScene().name == "main_menu")
            return;

        ConsiderSendingLocalPlayerUpdate();

#if !UNITY_WEBGL || UNITY_EDITOR
        web_socket.DispatchMessageQueue();
#endif

        UpdateNetworkPlayers();
    }

    void ConsiderSendingLocalPlayerUpdate()
    {
        if (PlayerController.instance == null)
            return;

        multiplayer_update_timer -= Time.deltaTime;
        if (multiplayer_update_timer <= 0.0f)
        {
            SendWebSocketMessage();
            multiplayer_update_timer = 1.0f / 10.0f;
        }
    }

    async void SendWebSocketMessage()
    {
        if (PlayerController.GetPlayerGameobject() == null)
            return;

        if (web_socket != null && web_socket.State == WebSocketState.Open)
        {
            Vector3 player_pos = PlayerController.GetPlayerPosition();
            Character current_char = Character.GetCurrentSelectedCharacter();
            string current_char_name = (current_char != null) ? current_char.GetCharacterName() : "???";


            ClientMultiplayerPayload new_payload =
                new ClientMultiplayerPayload()
                {
                    msg_type = "update",
                    x = player_pos.x,
                    y = player_pos.y,
                    z = player_pos.z,
                    level = SceneManager.GetActiveScene().name,
                    state = PlayerController.GetCurrentState(),
                    character = current_char_name,
                    room_key = GetRoomKey(),
                    api_version = "0.9"
                };

            if (HasOutgoingPayloadSignificantlyChanged(new_payload))
            {
                string string_payload = JsonConvert.SerializeObject(new_payload);
                await web_socket.SendText(string_payload);

                current_local_player_payload = new_payload;
            }
        }
    }

    bool HasOutgoingPayloadSignificantlyChanged(ClientMultiplayerPayload new_outgoing_payload)
    {
        if (new_outgoing_payload.character.Equals(current_local_player_payload.character) == false ||
            new_outgoing_payload.level.Equals(current_local_player_payload.level) == false ||
            new_outgoing_payload.state.Equals(current_local_player_payload.state) == false ||
            Mathf.Abs(new_outgoing_payload.x - current_local_player_payload.x) > 0.2f ||
            Mathf.Abs(new_outgoing_payload.y - current_local_player_payload.y) > 0.2f ||
            Mathf.Abs(new_outgoing_payload.z - current_local_player_payload.z) > 0.2f)
        {
            return true;
        }

        return false;
    }

    string GetRoomKey()
    {
        return Application.productName + "-" + Application.companyName;
    }

    void UpdateNetworkPlayers()
    {
        /* Update existing network players */
        foreach(string player_id in network_players.Keys)
        {
            // No need to update ourselves.
            if (player_id.Equals(local_client_id))
                continue;

            if(!network_player_views.ContainsKey(player_id))
            {
                network_player_views[player_id] = new NetworkPlayerView(player_id);
            }

            network_player_views[player_id].Update();
        }

        /* Conclude / destroy the views of disconnected network players */
        List<string> player_ids_to_remove = new List<string>();
        foreach(string player_id in network_player_views.Keys)
        {
            if (network_players.ContainsKey(player_id) == false)
            {
                ArborCoroutineManager.ArborStartCoroutine(network_player_views[player_id].DoDestruction());
                player_ids_to_remove.Add(player_id);
            }
        }

        foreach (string player_id_to_remove in player_ids_to_remove)
            network_player_views.Remove(player_id_to_remove);
    }

    IEnumerator MonitorNetwork()
    {
        Debug.Log("[NetworkManager] Now monitoring network");

        while (true)
        {
            try
            {
                if (web_socket == null || web_socket.State == WebSocketState.Closed)
                {
                    InitConnection();
                }
            }
            catch (Exception e) 
            {
                Debug.LogError("[NetworkManager] Encountered error [" + e.Message + "]");
            }

            yield return new WaitForSecondsRealtime(2.0f);
        }
    }

    async void InitConnection()
    {
        Debug.Log($"[NetworkManager] Initializing Connection to {server_url}...");

        web_socket = new WebSocket(server_url);

        web_socket.OnOpen += () =>
        {
            Debug.Log("[NetworkManager] Connection open!");
        };

        web_socket.OnError += (e) =>
        {
            Debug.Log("[NetworkManager] Error! " + e);
        };

        web_socket.OnClose += (e) =>
        {
            Debug.Log("[NetworkManager] Connection closed!");
        };

        web_socket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);

            if (message.Contains("initial"))
            {
                ServerMultiplayerPayloadInitial data = JsonConvert.DeserializeObject<ServerMultiplayerPayloadInitial>(message);
                local_client_id = data.player_id;
                server_time = data.server_time;
                Debug.Log("[NetworkManager] The local player client ID is [" + local_client_id + "] and server time (sec) is " + data.server_time);
            }
            else if (message.Contains("player_states"))
            {
                ServerMultiplayerPayloadPlayerStates data = JsonConvert.DeserializeObject<ServerMultiplayerPayloadPlayerStates>(message);

                if(data.players != null)
                    network_players = data.players;
            }
        };

        // Keep sending messages at every 0.3s
        InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        // waiting for messages
        await web_socket.Connect();
    }

    async void OnDestroy()
    {
        if (web_socket != null)
        {
            await web_socket.Close();
        }
    }

    async void OnApplicationQuit()
    {
        if (web_socket != null)
        {
            await web_socket.Close();
        }
    }
}

[System.Serializable]
public struct ClientMultiplayerPayload
{
    public string msg_type;
    public float x;
    public float y;
    public float z;
    public string level;
    public string state;
    public string character;
    public string room_key;
    public string api_version;
}

[System.Serializable]
public struct ServerMultiplayerPayloadInitial
{
    public string message_type;
    public string player_id;
    public float server_time;
}

[System.Serializable]
public struct ServerMultiplayerPayloadPlayerStates
{
    public string message_type;
    public Dictionary<string, PlayerState> players;
}

[System.Serializable]
public struct PlayerState
{
    public float x;
    public float y;
    public float z;
    public string level;
    public string state;
    public string character;
}

public class NetworkPlayerView
{
    string player_id;
    GameObject view_object;
    float size = 0.0f;
    bool is_valid = false;

    HasCharacterView has_character_view = null;

    public NetworkPlayerView(string _player_id)
    {
        player_id = _player_id;
    }

    public string GetNetworkPlayerState()
    {
        PlayerState state = NetworkManager.GetPlayerState(player_id);
        return state.state;
    }

    public void Update()
    {
        if (NetworkManager.DoesPlayerExist(player_id) == false)
            return;

        PlayerState state = NetworkManager.GetPlayerState(player_id);
        Vector3 desired_position = new Vector3(state.x, state.y, state.z);

        if (is_valid == false)
        {
            if (desired_position.x != 0 || desired_position.y != 0 || desired_position.z != 0)
            {
                is_valid = true;
            }

            return;
        }

        if (view_object == null)
        {
            view_object = GameObject.Instantiate(Resources.Load<GameObject>("NetworkPlayer"));
            view_object.transform.position = desired_position;
            view_object.transform.localScale = Vector3.zero;
            size = 0.0f;

            has_character_view = view_object.GetComponent<HasCharacterView>();
        }

        if (size < 1.0f)
        {
            size += Time.deltaTime;
            if (size > 1.0f)
                size = 1.0f;
        }
        else
        {
            size = 1.0f;
        }

        view_object.transform.localScale = Vector3.one * Mathf.Min(size, 1.0f);

        if(has_character_view != null)
            has_character_view.network_player_info = this;

        /* If player is in a special state (continue, game over, victory, etc) don't move their transforms even if they zoom away to a special part of the map. */
        bool should_update_transform = true;
        if (Mathf.Abs(desired_position.x) > 900 || Mathf.Abs(desired_position.y) > 900 || Mathf.Abs(desired_position.z) > 900 || state.state == "game_over" || state.state == "retry" || state.state == "continue" || state.state == "victory")
            should_update_transform = false;

        if(should_update_transform)
        {
            Vector3 delta = desired_position - view_object.transform.position;
            if (delta.magnitude > 0.01 && delta.magnitude < 5.0f)
            {
                Vector3 xz_direction = (new Vector3(delta.x, 0, delta.z)).normalized;
                view_object.transform.position = Vector3.Lerp(view_object.transform.position, desired_position, 0.1f);

                if (xz_direction.magnitude > 0)
                {
                    Quaternion desired_rotation = Quaternion.LookRotation(xz_direction, Vector3.up);
                    view_object.transform.rotation = Quaternion.Slerp(view_object.transform.rotation, desired_rotation, 0.1f);
                }
            }
            else if (delta.magnitude > 5.0f) // Distance too far. We should just warp instead.
            {
                Vector3 xz_direction = (new Vector3(delta.x, 0, delta.z)).normalized;
                view_object.transform.position = desired_position;
                Quaternion desired_rotation = Quaternion.LookRotation(xz_direction, Vector3.up);
                view_object.transform.rotation = desired_rotation;
            }
        }

        /* Character changes */
        if (has_character_view != null)
            has_character_view.EstablishCharacterViewByName(state.character);
    }

    public IEnumerator DoDestruction()
    {
        while(view_object.transform.localScale.x > 0.0f)
        {
            view_object.transform.localScale -= Vector3.one * Time.deltaTime;
            yield return null;
        }

        GameObject.Destroy(view_object);
    }

    public Vector3 GetPosition()
    {
        if (view_object == null)
            return Vector3.zero;

        return view_object.transform.position;
    }

    public Transform GetTransform()
    {
        if (view_object == null)
            return null;

        return view_object.transform;
    }
}