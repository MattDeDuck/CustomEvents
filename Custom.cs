using UnityEngine;

namespace CustomEvent
{
    public class Custom
    {
        public class CustomClick : InteractiveItem
        {
            public float xpAmount;
            public string goalName;
            public string goalDesc;

            public void OnMouseDown()
            {
                Debug.Log("Object name: " + this.gameObject.name + " found!");
                Debug.Log(this.xpAmount + " XP rewarded!");
                Debug.Log("Goal: " + this.goalName + " - " + this.goalDesc);
                
                // Set the goal as completed
                Plugin.EventGoals[goalName][goalDesc] = true;

                // Find the AudioHolder object
                GameObject obj = GameObject.Find("AudioHolder");
                AudioSource aud = obj.GetComponent<AudioSource>();

                // Create AudioClip with the loaded sound
                AudioClip snd = Plugin.CurrentClip;

                // Play the sound
                aud.PlayOneShot(snd, 0.8f);

                // Award the XP to the player
                Managers.Player.AddExperience(xpAmount);

                // Show the goal completion notification
                Notification.ShowText("Custom Event", this.goalName + " Goal Completed! (+" + this.xpAmount + "XP)" , Notification.TextType.EventText);

                // Remove object from events list
                Plugin.EventObjects.Remove(this.gameObject);

                // Destroy the object
                Destroy(this.gameObject);
            }
        }
    }
}
