using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace CustomEvent
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }
        public static string pluginLoc = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static GameObject AudioM;
        public static AudioClip CurrentClip;
        public static List<GameObject> EventObjects = new List<GameObject>();

        public static Dictionary<string, Dictionary<string, bool>> EventGoals = new Dictionary<string, Dictionary<string, bool>>();

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RoomManager), "OnLoad")]
        public static void OnLoad_Postfix()
        {
            CreateEvent();
            foreach (GameObject eo in EventObjects)
            {
                string name = eo.name;
                GameObject inact = GameObject.Find("Room Meeting");
                GameObject inactObj = inact.transform.Find(name).gameObject;
                eo.SetActive(false);
                inactObj.SetActive(true);
            }
        }

        public static void CreateEvent()
        {
            // Create audio holder for sounds
            if (GameObject.Find("AudioHolder") == null)
            {
                AudioM = new GameObject("AudioHolder");
                AudioM.AddComponent<AudioSource>();
                AudioM.SetActive(true);
                Log.LogInfo("AudioHolder object created");
            }
            else
            {
                Log.LogInfo("AudioHolder is already created!");
            }

            // Load objects into dictionary
            Dictionary<string, Data.EventObject> custObj;
            custObj = GetObjectDictionary();

            // Loop through dictionary
            foreach (var item in custObj)
            {
                string name = item.Key;
                var co = item.Value;
                string room = co.Room;
                string sprite = co.Sprite;
                float rotation = co.Rotation;
                Vector3 location = new Vector3(co.Location[0], co.Location[1], co.Location[2]);
                float xp = co.XP;
                string goalName = co.Goal[0];
                string goalDesc = co.Goal[1];
                string sound = co.Sound;

                CreateCustomObject(name, room, sprite, rotation, location, xp, goalName, goalDesc, sound);
            }
        }

        public static async void PlayClip(string sndlocation)
        {
            // Load the sound from file
            CurrentClip = await LoadClip(pluginLoc + sndlocation);
        }

        public static async Task<AudioClip> LoadClip(string path)
        {
            AudioClip clip = null;
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
            {
                uwr.SendWebRequest();

                // wrap tasks in try/catch, otherwise it'll fail silently
                try
                {
                    while (!uwr.isDone) await Task.Delay(5);

                    if (uwr.result == UnityWebRequest.Result.ConnectionError) Debug.Log($"{uwr.error}");
                    else
                    {
                        clip = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                }
                catch (Exception err)
                {
                    Debug.Log($"{err.Message}, {err.StackTrace}");
                }
            }

            return clip;
        }

        public static Dictionary<string, Data.EventObject> GetObjectDictionary()
        {
            // Turn JSON file into a JSON string
            string jsonString = File.ReadAllText(pluginLoc + "/objects.json");

            // Convert it into a dictionary
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, Data.EventObject>>(jsonString);

            // Return the dictionary
            return dictionary;
        }

        public static void CreateCustomObject(string name, string room, string textureLocation, float angle, Vector3 position, float xp, string goalName, string goalDescription, string sound)
        {
            // Load texture and create the sprite
            Texture2D tex = LoadTextureFromFile(pluginLoc + textureLocation);
            Sprite coSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            // Set the new object with (name)
            GameObject customObj = new GameObject(name);

            // Add the transform parent in (room)
            var parent = GameObject.Find(room).transform;
            customObj.transform.parent = parent;

            // Set the (sprite)
            var sr = customObj.AddComponent<SpriteRenderer>();
            sr.sprite = coSprite;

            // Update collider
            customObj.AddComponent<PolygonCollider2D>();
            Destroy(customObj.GetComponent<PolygonCollider2D>());
            customObj.AddComponent<PolygonCollider2D>();

            // Add the sorting group and adjust layer ID
            var sg = customObj.AddComponent<SortingGroup>();
            sg.sortingLayerID = -2145782171;

            // Load the sound
            PlayClip(sound);
            
            // Add our custom behaviour
            customObj.AddComponent<Custom.CustomClick>();

            // Update settings
            Custom.CustomClick ccSettings = customObj.GetComponent<Custom.CustomClick>();
            ccSettings.xpAmount = xp;
            ccSettings.goalName = goalName;
            ccSettings.goalDesc = goalDescription;

            // Check for existing goals
            if(!EventGoals.ContainsKey(goalName))
            {
                EventGoals.Add(goalName, new Dictionary<string, bool>());
                EventGoals[goalName].Add(goalDescription, false);
            }

            // Set the (rotation)
            customObj.transform.eulerAngles = new Vector3(0f, 0f, angle);

            // Set the (position)
            customObj.transform.localPosition = position;

            // Activate the new object
            customObj.SetActive(true);

            Debug.Log(name + " object created at " + customObj.transform.localPosition + " in " + room);
            EventObjects.Add(customObj.gameObject);
        }

        public static Texture2D LoadTextureFromFile(string filePath)
        {
            var data = File.ReadAllBytes(filePath);

            var tex = new Texture2D(0, 0, TextureFormat.ARGB32, false, false)
            {
                filterMode = FilterMode.Bilinear,
            };

            if (!tex.LoadImage(data))
            {
                throw new Exception($"Failed to load image from file at \"{filePath}\".");
            }

            return tex;
        }
    }
}
