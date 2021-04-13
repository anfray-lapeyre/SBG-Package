using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaltButter.Saving
{
    public class SavingSystem : MonoBehaviour
    {

        public IEnumerator LoadLastScene(string saveFile)
        {
            Dictionary<string, object> state = LoadFile(saveFile);
            if (state.ContainsKey("lastSceneBuildIndex"))
            {
                int buildIndex = (int)state["lastSceneBuildIndex"];
                if (buildIndex != SceneManager.GetActiveScene().buildIndex)
                {
                    yield return SceneManager.LoadSceneAsync(buildIndex);

                }
            }
            
            RestoreState(state);
        }


        public void Save(string saveFile)
        {
            Dictionary<string, object>  state = LoadFile(saveFile);

            CaptureState(state);

            SaveFile(saveFile, state);
        }

        

        public void Load(string saveFile)
        {
            
            RestoreState(LoadFile(saveFile));

        }

        private Dictionary<string, object> LoadFile(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);

            if (!File.Exists(path))
                return new Dictionary<string, object>();
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Dictionary<string, object>)formatter.Deserialize(stream);
            }
        }

        private void SaveFile(string saveFile, object state)
        {
            string path = GetPathFromSaveFile(saveFile);
            Debug.Log($"Saving to : {path}");
            using (FileStream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, state);
            }
        }



        private byte[] SerializeVector(Vector3 vector)
        {
            byte[] vectorBytes = new byte[3 * 4];
            BitConverter.GetBytes(vector.x).CopyTo(vectorBytes,0);
            BitConverter.GetBytes(vector.x).CopyTo(vectorBytes,4);
            BitConverter.GetBytes(vector.x).CopyTo(vectorBytes,8);
            return vectorBytes;
        }

        private Vector3 DeserializeVector(byte[] buffer)
        {
            return new Vector3(BitConverter.ToSingle(buffer, 0), BitConverter.ToSingle(buffer, 4), BitConverter.ToSingle(buffer, 8));
        }

        private void CaptureState(Dictionary<string, object> state)
        {
            foreach(SaveableEntity entity in FindObjectsOfType<SaveableEntity>())
            {
                state[entity.GetUniqueIdentifier()] = entity.CaptureState();
            }
            state["lastSceneBuildIndex"] = SceneManager.GetActiveScene().buildIndex;
        }

        private void RestoreState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity entity in FindObjectsOfType<SaveableEntity>())
            {
                string ID = entity.GetUniqueIdentifier();
                if (state.ContainsKey(ID))
                {
                    entity.RestoreState(state[ID]);
                }
            }
        }

        private string GetPathFromSaveFile(string saveFile)
        {
            return Path.Combine(Application.persistentDataPath, saveFile + ".salt");
        }
    }
}