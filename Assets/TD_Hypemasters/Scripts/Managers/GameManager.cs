using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager
{
    #region Singleton
    
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameManager();
            }

            return _instance;
        }
    }
    
    #endregion

    private Dictionary<string, MonoBehaviour> _managers;

    private GameManager()
    {
        _managers = new Dictionary<string, MonoBehaviour>();
    }

    public void RegisterManager<T>(T manager) where T : MonoBehaviour
    {
        string key = typeof(T).ToString();
        
        if (_managers.ContainsKey(key))
        {
            Debug.LogError("Manager already registered: " + key);
            return;
        }
        
        _managers.Add(key, manager);
    }

    public T GetManager<T>() where T: MonoBehaviour
    {
        string key = typeof(T).ToString();
        if (!_managers.ContainsKey(key))
        {
            Debug.LogError("Manager not found: " + key);
            return null;
        }

        return _managers[key] as T;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
        SceneManager.sceneUnloaded += OnSceneUnload;
    }

    public void OnSceneUnload(Scene scene)
    {
        _managers.Clear();
    }
}
