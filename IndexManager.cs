using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace FFLParaw
{
    internal class IndexManager
    {
        private static bool updateSave;
        private static bool updateLegacySave;

        public static Dictionary<string, int> loadedIndexMap = new Dictionary<string, int>();
        public static Dictionary<string, int> indexMap = new Dictionary<string, int>();

        public static string loadedVersion;

        //PATCHES
        [HarmonyPriority(400)]  //this should make sure Manager() runs before SaveCleaner()
        private static void Manager(int backupIndex)
        {   //runs the necessary methods in order when LoadGame() is called
            Debug.LogWarning("FFLParaw: Manager running");
            string path = ((backupIndex != 0) ? SaveSlots.GetBackupPath(SaveSlots.currentSlot, backupIndex) : SaveSlots.GetCurrentSavePath());
            SaveContainer saveContainer;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {   //unpack the save to access the saveContainer
                saveContainer = (SaveContainer)binaryFormatter.Deserialize(fileStream);
            }
            LoadSavedIndexes(saveContainer);
            ValidateIndexes();
            if (updateSave)
            {
                if (updateSave)
                {
                    saveContainer = UpdateSave(saveContainer);
                }
            }
            using (FileStream fileStream = File.Open(path, FileMode.Create))
            {   //pack the savecontainer back into the save
                binaryFormatter.Serialize(fileStream, saveContainer);
            }
        }
        public static void SaveIndex()
        {   //adds the boatName:sceneIndex data to the modData dictionary
            //this runs toward the end of the LoadGame() method (it patches LoadModData())

            if (!GameState.currentlyLoading)
            {
                return;
            }
            GameState.modData[ParawMain.pluginGuid] = "";
            SaveModData();
        }
        public static void StartNewGamePatch()
        {   //we need to save the modData if this is a newgame!
            SaveModData();
        }

        // SAVE MANIPULATION
        public static void AssignAvailableIndex(GameObject boat)
        {   //assigns the first available index to the boat
            SaveableObject[] objs = SaveLoadManager.instance.GetCurrentObjects();
            int holeSize = GetHoleSize(boat);
            int index = FindHole(objs, holeSize);
            boat.GetComponent<SaveableObject>().sceneIndex = index;
            indexMap[boat.name] = index;
        }
        public static void LoadSavedIndexes(SaveContainer saveContainer)
        {   //Check if we have previously assigned indexes to modded items

            if (saveContainer.modData.ContainsKey(ParawMain.pluginGuid))
            {
                string data = saveContainer.modData[ParawMain.pluginGuid];
                string[] entries = data.Split(';');                         //entries is now an array like: ["boat:1","boat:2",...]
                for (int i = 0; i < entries.Length - 1; i++)
                {
                    string boatName = entries[i].Split(':')[0];             //itemName is a string like: "item1"
                    int sceneIndex = int.Parse(entries[i].Split(':')[1]);    //itemIndex is an int like: 1
                    loadedIndexMap[boatName] = sceneIndex;
                }
            }
            else
            {
                //Debug.LogWarning("FFLParaw: BoatIndexManager: No mod data saved...");
            }
            if (saveContainer.modData.ContainsKey(ParawMain.shortName + ".version"))
            {
                loadedVersion = saveContainer.modData[ParawMain.shortName + ".version"];
                //check for version here if needed
            }
            else
            {   //no version saved means the save is a legacy version and that we need to update it
                updateLegacySave = true;
            }
        }
        private static SaveContainer UpdateSave(SaveContainer saveContainer)
        {   //updates the indexes in the save if necessary

            Debug.LogWarning("FFLParaw: updating save " + SaveSlots.currentSlot);
            foreach (string boat in loadedIndexMap.Keys)
            {   // change the parentIndex of saved items
                foreach (SavePrefabData prefab in saveContainer.savedPrefabs.Where(x => x != null && x.itemParentObject == loadedIndexMap[boat]))
                {
                    prefab.itemParentObject = indexMap[boat];
                }

                //change the boat indexes
                foreach (SaveObjectData savedBoat in saveContainer.savedObjects.Where(x => x != null && x.sceneIndex == loadedIndexMap[boat]))
                {
                    savedBoat.sceneIndex = indexMap[boat];
                }

                //updates the Shipyard Expansion saved data (important otherwise sail are loaded with their vanilla size!)
                if (saveContainer.modData.ContainsKey($"SEboatSails.{loadedIndexMap[boat]}"))
                {   
                    string seConfig = saveContainer.modData[$"SEboatSails.{loadedIndexMap[boat]}"];
                    saveContainer.modData.Remove($"SEboatSails.{loadedIndexMap[boat]}");
                    saveContainer.modData[$"SEboatSails.{indexMap[boat]}"] = seConfig;
                }
            }
            Debug.LogWarning("FFLParaw: save updated...");
            return saveContainer;
        }
        
        //UTILITIES
        private static void SaveModData()
        {   //used by SaveIndex and StartNewGamePatch to save data to modData dictionary
            GameState.modData[ParawMain.pluginGuid] = "";
            foreach (string name in indexMap.Keys)
            {
                string entry = name.ToString() + ":" + indexMap[name].ToString() + ";"; //name:1;
                if (GameState.modData.ContainsKey(ParawMain.pluginGuid))
                {
                    GameState.modData[ParawMain.pluginGuid] += entry;
                }
                else
                {
                    GameState.modData[ParawMain.pluginGuid] = entry;
                }
            }
            //Add mod version informations
            GameState.modData[ParawMain.shortName + ".version"] = ParawMain.pluginVersion;
        }
        private static int GetHoleSize(GameObject boat)
        {   //calculates how many free spaces the mod needs in the SaveableObjects array
            return boat.GetComponent<BoatMooringRopes>().ropes.Length + 1;
        }
        public static int FindHole(SaveableObject[] array, int holeSize)
        {   //finds the first "hole" in the array of the correct size (for the boat and all ropes)
            for (int i = 1; i < array.Length - holeSize; i++)
            {   //iterate over the whole array
                if (array[i] == null)
                {   //found a null
                    for (int j = 0; j < holeSize; j++)
                    {   //iterate over the next holeSize things to see if they are all null
                        if (array[i + j] != null)
                        {   //if one isn't null, leave this loop
                            break;
                        }
                        else
                        {   //keep going
                            if (j == holeSize - 1)
                            {   //if this is the right size, return i
                                return i;
                            }
                        }
                    }
                }
            }
            return -1;
        }
        private static void ValidateIndexes()
        {   //goes through all the items in the indexMap and checks if the old indexes are still valid
            //if at least one index is no longer valid we set updateSave to true
            foreach (string boat in indexMap.Keys)
            {
                if (loadedIndexMap.ContainsKey(boat))
                {
                    if (loadedIndexMap[boat] == indexMap[boat])
                    {
                        //Debug.LogWarning($"IndexManager: {boat} index is still valid: {indexMap[boat]}");
                    }
                    else
                    {
                        //Debug.LogWarning($"IndexManager: {boat} index is no longer valid, update required!");
                        updateSave = true;
                    }
                }
                else
                {
                    //Debug.LogWarning($"IndexManager: {boat} was not on the list before...");
                }
            }
        }
    }
}
