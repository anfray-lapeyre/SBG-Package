SaltButter Games Package

Contains
- An Input System using an Observer/Subject design pattern and a Command pattern
- A custom UI system that will bypass Unity UI bugs when updating on runtime navigation (works well with keyboard & controllers, but not with mouse)
- A Dialogue System with a visual editor (this works only with I2.Localization)
- A Quest System that can be used with the Dialogue System
- A Saving System that will automatically save any object using a script inheriting the ISaveable interface


How to use :
Input System : 
-Add simply the Persistant Object Prefab to your starting scene.
-If you want to make your custom listener to the input system, make your script inherit the Observer class (SaltButter.Core)
 *** then put GameObject.FindGameObjectWithTag("PersistantHandler").GetComponentInChild<InputHandler>().addObserver(this); -> You must set the tag for the Persistant Handler as "PersistantHandler"
 *** to stop listening add subect.removeObserver(this);
 *** to handle the different inputs, override the OnNotify virtual function of the Observer class
 *** modify the GameInput file to change the bindings to each Command
 *** Check in the Script/Input/Commands to see how every command work

 Custom UI : 
 Simply use the Prefabs in Prefab/UI to be sure to use everything correctly :)
 It is relatively simple to use, but that way you will not waste time

 Saving System :
 -Like the Input System, simply add the Persistant Object Prefab to your starting scene.
 -For each Object that you want to save, add the SaveableEntity script to your gameObject. This will give a unique identifier to your object.
 *** Then add the ISaveable interface to the data you want to save and implement the needed methods
 *** You will certainly have to make a subclass for your data to be Serializable and to wrap/unwrap it.

 Dialogue System :
 -To create a Dialogue, simply Right Click in your Project Window and click on Dialogue.
 -Open it and you will access the editor. Simply add nodes, and don't forget to save.
 -Conditions can be used with specific terms that will be reminded to you if you don't put the right format
 ***In conditions, you can use boolean logic with the following terms (...),||,&&,!
 ***To add an exposed Property to your text, simply add {YourExposedPropertyName} to your text.
 Example - "Hi {username}, how are you ?" 

 Translation :
 ***Add a term to your Localization Handler, make it an "Object" type and put your Dialogue in the corresponding language.
 ***Simply duplicate your Dialogue object and translate them in the other languages, then add it to their corresponding language on the same term

 -To use them in game : (Works only with I2 Localization for now)
 ***Add to your player the Player Conversant Script
 ***Add to your NPC the AI Conversant Script (this will enable him to talk with you) enter its name and the key corresponding to the translation
 ***Add the DialogueUI Prefab in your canvas
 
 -To use triggers in game : 
 ***Add to your NPC conversant the DialogueTrigger Script, enter the name of the Action and the event you want to trigger.

 -To use conditions in game :
 ***You must have a script inheriting the IPredicateSolver interface and implement the HandleSinglePredicate method
 ***You can find an implementation of this in the QuestList script
 ***If you want to add a term to the list of condition terms that can be used, you will have to add it in the PredicateType class in SaltButter.Core

 Quest System :
-To create a quest, simply Right Click in your Project Window and click on Quest.
-You will be able to edit it in the inspector

Translation :
 ***Add a term to your Localization Handler, make it an "Object" type and put your Quest in the corresponding language.
 ***Simply duplicate your Quest object and translate them in the other languages, then add it to their corresponding language on the same term
 ***We highly suggest to keep the same references for each translation, this will make everything a lot easier for you in the long run

 Add the Quest System in Game : 
 -Add the Quest List script to your player. This script contains a number of conditions that it can handle, you can find exactly which in the Quest List script.
 -Add the QuestUI Prefab to your Canvas

 To give a quest : 
 -Add the Quest Giver Script to an NPC, you can call the GiveQuest yourself or call it with a Dialogue Trigger
 ***Add the Quest term (from I2 Localization) to the script

 To Complete an Objective or A Quest : 
  -Add the Quest Complete Script to an NPC, you can call the CompleteObjective yourself or call it with a Dialogue Trigger
  ***Add the Quest term (from I2 Localization) and the objective reference to the script
  ***When a Quest has all objectives complete, it will be considered completed.

  How to save Quest progess : 
  -Simply add the SaveableEntity script to the player object that contains the Quest List script. Quest List implements the ISaveable interface, it will automatically handle saving of all quest's progress

