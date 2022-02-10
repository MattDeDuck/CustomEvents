
# Custom Events
Custom Events is a mod made with BepInEx for use with the Potion Craft game. It allows players to make their own hidden object type game event.

### How it works
To create event objects in whichever room you would like, you will need to create a JSON file called `objects.json`

For the objects you can specify the following:
- Name
- Room They Spawn
- The sprite/image they have
- The rotation of the object
- Location on the screen
- How much XP is rewarded when found
- Goal Name
- Goal Description

### Example `objects.json` file
```
{
  "Object name": {
    "Room" : "Room Meeting",
    "Sprite" : "/objectimage.png",
    "Rotation" : "30",
    "Location" : [ 
      "0.5",
      "0.5",
      "0" 
    ],
    "XP" : "100",
    "Goal" : [
      "Example Goal",
      "This is the goal description"
    ]
  }
 }
```