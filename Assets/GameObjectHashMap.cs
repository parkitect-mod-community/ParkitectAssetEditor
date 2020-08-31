using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Utilities;
using UnityEngine;

/// <summary>
/// A hashmap helper for referencing asset game objects.
/// </summary>
public class GameObjectHashMap : MonoBehaviour
{
    private static GameObjectHashMap _instance;

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>
    /// The instance.
    /// </value>
    public static GameObjectHashMap Instance
    {
        get
        {
            if (_instance == null)
            {
                // got to do this because the asset editor window's awake can be called before the awake of this class, causing double hash map instances :/
                var result = GameObject.Find("GameObjectHashMap");
                if (result != null)
                {
                    _instance = result.GetComponent<GameObjectHashMap>();
                }
                else
                {
                    var go = new GameObject("GameObjectHashMap");
                    _instance = go.AddComponent<GameObjectHashMap>();
                }
            }

            return _instance;
        }
    }

    public List<string> Keys = new List<string>(10);

    public List<GameObject> GameObjects = new List<GameObject>(10);

    public List<String> TextureKeys = new List<string>(10);

    public List<Texture> Textures = new List<Texture>(10);

    void Awake()
    {
        _instance = this;
    }

    public void SetTexture(string key, Texture2D value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }
        if (value == null)
        {
            RemoveTexture2d(key);
            return;
        }

        if (!TextureKeys.Contains(key))
        {
            TextureKeys.Add(key);
            Textures.Add(value);
        }
        else
        {
            Textures[TextureKeys.IndexOf(key)] = value;
        }
    }



    /// <summary>
    /// Sets the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void SetGameObject(string key, GameObject value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (value == null)
        {
            RemoveGameObject(key);
            return;
        }

        if (!Keys.Contains(key))
        {
            Keys.Add(key);
            GameObjects.Add(value);
        }
        else
        {
            GameObjects[Keys.IndexOf(key)] = value;
        }
    }

    /// <summary>
    /// Gets the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The game object with the key.</returns>
    public GameObject GetGameObject(string key)
    {
        return ContainsGameObject(key) ? GameObjects[Keys.IndexOf(key)] : null;
    }

    public Texture GetTexture(String key)
    {
        return ContainsTexture2d(key) ? Textures[TextureKeys.IndexOf(key)] : null;
    }

    /// <summary>
    /// Removes the game object with specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    public void RemoveGameObject(string key)
    {
        if (ContainsGameObject(key))
        {
            GameObjects.RemoveAt(Keys.IndexOf(key));
            Keys.Remove(key);
        }
    }

    public void RemoveTexture2d(String key)
    {
        if (ContainsTexture2d(key))
        {
            Textures.RemoveAt(TextureKeys.IndexOf(key));
            Keys.Remove(key);
        }
    }

    /// <summary>
    /// Determines whether the specified key exists.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    ///   <c>true</c> if the specified key exists; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsGameObject(string key)
    {
        return Keys.Contains(key);
    }

    public bool ContainsTexture2d(string key)
    {
        return TextureKeys.Contains(key);
    }

    /// <summary>
    /// Determines whether the specified game object exists.
    /// </summary>
    /// <param name="value">The game object.</param>
    /// <returns>
    ///   <c>true</c> if the specified game object exists; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsGameObject(GameObject value)
    {
        return GameObjects.Contains(value);
    }
    /// <summary>
    /// Determines whether the specified texture exists.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool ContainsTexture2d(Texture2D value)
    {
        return Textures.Contains(value);
    }
}
