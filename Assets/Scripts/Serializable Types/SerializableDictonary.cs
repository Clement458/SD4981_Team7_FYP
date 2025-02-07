using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDicitonary<TKey, TTValue> : Dictionary<TKey, TTValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TTValue> values = new List<TTValue>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
         foreach (KeyValuePair<TKey, TTValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
        {            
            Debug.LogError("Tried to deseralize dictionary, but no. of keys (" + keys.Count + ") does not match the no. of values (" + values.Count);
        }

        for (int i = 0; i < keys.Count; i++)
        {
            this.Add(keys[i], values[i]);
        }
    }
}
