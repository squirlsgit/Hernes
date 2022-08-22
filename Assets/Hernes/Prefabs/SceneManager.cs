using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using TMPro;
using System;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Hernes.SceneManager))]
public class TestGame : Editor
{
    public string interfaceName = "squirrel";
    public override void OnInspectorGUI()
    {
        var gm = (Hernes.SceneManager)target;
        GUILayout.Space(20f);
        GUILayout.Label("Test Game");
        if (GUILayout.Button("Start Game"))
        {
            gm.StartGame();
        }
        if (GUILayout.Button("Set Player Data"))
        {
            gm.PlayerManager.InitPlayerData(true);
        }
        if (GUILayout.Button("End Game"))
        {
            gm.EndGame();
        }
        if (GUILayout.Button("Set Name"))
        {
            gm.SetName(interfaceName);
        }
        if (EditorApplication.isPlaying)
        {
            var name = GUILayout.TextField(interfaceName);
            if (name != interfaceName)
            {
                interfaceName = name;
                gm.SetName(name);
            }
        }
        base.OnInspectorGUI();
    }
}
#endif
namespace Hernes
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager instance;
        public PlayerStore PlayerDataStore;
        public FirebaseStore PlayerEventsStore;
        public FirebaseStore PlayerScoreStore;
        public PlayerManager PlayerManager;
        public FirebaseStoreManager PlayersStore;
        public FirebaseStoreManager NpcsStore;
        public FirebaseStoreManager ItemsStore;
        public ItemSpawner ItemSpawner;
        public TMP_InputField UsernameInput;
        public TMP_Text ScoreCard_ScoreText;
        public TMP_Text ScoreCard_NameText;
        public GameObject PlayerData;
        public enum GameState
        {
            Playing, Paused, Ended
        }
        public GameState _state = GameState.Ended;
        public GameState State
        {
            get
            {
                return _state;
            }
            set
            {
                switch (value)
                {
                    case GameState.Playing:
                        break;
                    case GameState.Paused:
                        break;
                    case GameState.Ended:
                        break;
                    default:
                        Debug.LogWarning($"Unknown State Value {value}");
                        break;
                }
                _state = value;
            }
        }
        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            var pm = GameObject.FindGameObjectWithTag("Player");
            PlayerManager = pm.GetComponent<PlayerManager>();
            PlayerEventsStore = PlayerData.GetComponent<FirebaseStore>();
            PlayerDataStore = PlayerData.GetComponent<PlayerStore>();
            PlayerScoreStore = (GameObject.FindGameObjectWithTag("UserScore")).GetComponent<FirebaseStore>();
            PlayersStore = (GameObject.FindGameObjectWithTag("PlayersStore")).GetComponent<FirebaseStoreManager>();
            NpcsStore = (GameObject.FindGameObjectWithTag("NpcsStore")).GetComponent<FirebaseStoreManager>();
            ItemsStore = (GameObject.FindGameObjectWithTag("ItemsStore")).GetComponent<FirebaseStoreManager>();
            ItemSpawner = (GameObject.FindGameObjectWithTag("ItemsStore")).GetComponent<ItemSpawner>();
            if (PlayerPrefs.HasKey("Name"))
            {
                SetName(PlayerPrefs.GetString("Name"));
                PlayerManager.Name = PlayerPrefs.GetString("Name");
                UsernameInput.text = PlayerManager.Name;
                StartGame();
            }
            //DontDestroyOnLoad(pm);
            //DontDestroyOnLoad(pmd);
            //DontDestroyOnLoad(PlayersStore.gameObject);
            //DontDestroyOnLoad(NpcsStore.gameObject);
            //DontDestroyOnLoad(ItemsStore.gameObject);
            //DontDestroyOnLoad(ItemSpawner.gameObject);
        }
        public string scorecardtimeformat = @"{0:dd} day{1} {0:hh\:ss\:mm\.fff}";
        public string GetTimeSpan(TimeSpan ts)
        {
            if (ts.TotalDays >= 1)
            {
                return String.Format(@"{0:%d} day{1} {0:%h\:mm\:ss}", ts, ts.TotalDays == 1 ? "" : "s");
            }
            else if (ts.TotalHours >= 1)
            {
                return String.Format(@"{0:%h} hour{1} {0:%m\:ss}", ts, ts.TotalHours == 1 ? "" : "s");
            }
            else if (ts.TotalMinutes >= 1)
            {
                return String.Format(@"{0:%m} minute{1} {0:%s} second{2}", ts, ts.TotalMinutes == 1 ? "" : "s", ts.Seconds == 1 ? "" : "s");
            }
            else
            {
                return ts.ToString("u");
            }
        }
        private void FixedUpdate()
        {
            if (State == GameState.Ended)
            {
                if (PlayerScoreStore?.Data != null && PlayerScoreStore.Data.TryGetValue("time", out var t))
                {
                    var time = (string)t;
                    if (time != null)
                    {
                        ScoreCard_ScoreText.text = GetTimeSpan(TimeSpan.Parse(time));
                    }
                }
                if (PlayerDataStore?.Data != null && PlayerDataStore.Data.TryGetValue("name", out var n))
                {
                    var name = (string)n;
                    if (name != null)
                    {
                        ScoreCard_NameText.text = name;
                    }
                }
            }
        }
        public async void StartGame()
        {
            try
            {
                Debug.Log("Start Game");
                await PlayerManager.SpawnPlayer();
                State = GameState.Playing;
                OnStarted.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                State = GameState.Ended;
            }
        }
        public void EndGame()
        {
            State = GameState.Ended;
            try
            {
                Debug.Log("End Game");
                PlayerManager.DespawnPlayer();
                State = GameState.Ended;
                OnEnded.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                State = GameState.Playing;
            }
            // Pause Spawners
        }
        // On Player Headset Removed
        GameState previousState;
        public void OnHeadsetRemoved()
        {
            previousState = State;
            State = GameState.Paused;
            OnPaused.Invoke();
        }
        public void OnHeadsetOn()
        {
            State = previousState;
        }
        public void SetName(string name)
        {
            PlayerManager.Name = name;
            UsernameInput.text = name;
        }

        public UnityEvent OnPaused = new UnityEvent();
        public UnityEvent OnStarted = new UnityEvent();
        public UnityEvent OnEnded = new UnityEvent();

    }


}