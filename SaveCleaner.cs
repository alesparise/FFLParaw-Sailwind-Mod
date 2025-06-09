using HarmonyLib;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace FFLParaw
{   /// <summary>
    /// Cleans the save so you can remove the mod safely
    /// </summary>
    internal class SaveCleaner
    {
        private const int boatIndex = 275;  //the paraw is index 275

        [HarmonyPriority(300)]
        public static void CleanSaveLegacy(int backupIndex)
        {   //removes all references to the modded boat before the game loads
            //this is to be used with the dynamic index system
            Debug.LogWarning("FFLParaw: CleanSave running");
            Debug.LogWarning("FFLParaw: cleaning save " + SaveSlots.currentSlot);
            string path = ((backupIndex != 0) ? SaveSlots.GetBackupPath(SaveSlots.currentSlot, backupIndex) : SaveSlots.GetCurrentSavePath());
            SaveContainer saveContainer;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {   // Deserialize the save container from the file
                saveContainer = (SaveContainer)binaryFormatter.Deserialize(fileStream);
            }

            foreach (string boat in IndexManager.loadedIndexMap.Keys)
            {
                int i = IndexManager.loadedIndexMap[boat];
                saveContainer.savedPrefabs.RemoveAll(x => x.itemParentObject == i || x.itemParentObject == i + 1  || x.itemParentObject == i + 2 || x.itemParentObject == i + 3 || x.itemParentObject == i + 4);
                saveContainer.savedObjects.RemoveAll(x => x.sceneIndex == i || x.sceneIndex == i + 1 || x.sceneIndex == i + 2 || x.sceneIndex == i + 3 || x.sceneIndex == i + 4);
            }

            using (FileStream fileStream = File.Open(path, FileMode.Create))
            {
                binaryFormatter.Serialize(fileStream, saveContainer);
            }
            Debug.LogWarning("FFLParaw: save cleaned...");
        }
        public static void CleanSave(int backupIndex)
        {
            string path = ((backupIndex != 0) ? SaveSlots.GetBackupPath(SaveSlots.currentSlot, backupIndex) : SaveSlots.GetCurrentSavePath());
            SaveContainer saveContainer;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {   //Deserialize the save file
                saveContainer = (SaveContainer)binaryFormatter.Deserialize(fileStream);
            }
            //Do the actual cleaning
            saveContainer.savedPrefabs.RemoveAll(x => x.itemParentObject == boatIndex || x.itemParentObject == boatIndex + 1 || x.itemParentObject == boatIndex + 2 || x.itemParentObject == boatIndex + 3 || x.itemParentObject == boatIndex + 4);
            saveContainer.savedObjects.RemoveAll(x => x.sceneIndex == boatIndex || x.sceneIndex == boatIndex + 1 || x.sceneIndex == boatIndex + 2 || x.sceneIndex == boatIndex + 3 || x.sceneIndex == boatIndex + 4);

            using (FileStream fileStream = File.Open(path, FileMode.Create))
            {   //Serialize back to the save file
                binaryFormatter.Serialize(fileStream, saveContainer);
            }
            Debug.LogWarning("FFLParaw: save cleaned...");
        }
    }
}
