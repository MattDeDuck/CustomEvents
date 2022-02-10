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

namespace CustomEvent
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }
        public static string pluginLoc = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static List<GameObject> EventObjects = new List<GameObject>();
        public static Dictionary<string, Dictionary<string, bool>> EventGoals = new Dictionary<string, Dictionary<string, bool>>();

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InputManager), "Update")]
        public static void Init_Postfix()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Log.LogInfo(EventObjects.Count);
            }
            if(Input.GetKeyDown(KeyCode.M))
            {
                Dictionary<string, Data.EventObject> custObj;
                custObj = GetObjectDictionary();
                
                foreach (var item in custObj)
                {
                    var key = item.Key;
                    var cObject = item.Value;

                    CreateCustomObject(key, cObject.Room, cObject.Sprite, cObject.Rotation, new Vector3(cObject.Location[0], cObject.Location[1], cObject.Location[2]), cObject.XP, cObject.Goal[0], cObject.Goal[1]);
                }
                
            }
        }

        public static Dictionary<string, Data.EventObject> GetObjectDictionary()
        {
            string jsonString = File.ReadAllText(pluginLoc + "/objects.json");
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, Data.EventObject>>(jsonString);
            return dictionary;
        }

        public static void CreateCustomObject(string name, string room, string textureLocation, float angle, Vector3 position, float xp, string goalName, string goalDescription)
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

            Debug.Log(name + " object created at " + customObj.transform.localPosition);
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
