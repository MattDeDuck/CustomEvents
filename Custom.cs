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
                Plugin.EventGoals[goalName][goalDesc] = true;
                Managers.Player.AddExperience(xpAmount);
                Notification.ShowText("Custom Event", this.goalName + " Goal Completed!", Notification.TextType.EventText);
                Plugin.EventObjects.Remove(this.gameObject);
                Destroy(this.gameObject);
            }
        }
    }
}
