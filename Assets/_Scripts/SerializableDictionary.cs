using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();
    [SerializeField]
    private List<TValue> values = new List<TValue>();

    public void Add(TKey key, TValue value)
    {
        keys.Add(key);
        values.Add(value);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            value = values[index];
            return true;
        }
        value = default(TValue);
        return false;
    }

    public bool Remove(TKey key)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
            return true;
        }
        return false;
    }

    public int Count
    {
        get { return keys.Count; }
    }

    public TKey GetKey(int index)
    {
        return keys[index];
    }

    public TValue GetValue(int index)
    {
        return values[index];
    }
    
    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }
    
    public bool ContainsKey(TKey key)
    {
        return keys.Contains(key);
    }
    
    public bool ContainsValue(TValue value)
    {
        return values.Contains(value);
    }
    
    public List<TKey> GetKeys()
    {
        return keys;
    }
    
    public List<TValue> GetValues()
    {
        return values;
    }
    
    public void SetKeys(List<TKey> keys)
    {
        this.keys = keys;
    }
    
    public void SetValues(List<TValue> values)
    {
        this.values = values;
    }
    
    public TValue this[TKey key]
    {
        get
        {
            int index = keys.IndexOf(key);
            if (index >= 0)
            {
                return values[index];
            }
            throw new KeyNotFoundException(key.ToString());
        }
        set
        {
            int index = keys.IndexOf(key);
            if (index >= 0)
            {
                values[index] = value;
            }
            else
            {
                keys.Add(key);
                values.Add(value);
            }
        }
    }
    
    public void SetValue(TKey key, TValue value)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            values[index] = value;
        }
        else
        {
            keys.Add(key);
            values.Add(value);
        }
    }
    
    public void RemoveAt(int index)
    {
        keys.RemoveAt(index);
        values.RemoveAt(index);
    }
    
    public void RemoveValue(TValue value)
    {
        int index = values.IndexOf(value);
        if (index >= 0)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
    }
    
    public void RemoveKey(TKey key)
    {
        int index = keys.IndexOf(key);
        if (index >= 0)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
    }
    
    public void RemoveAll(Predicate<TKey> match)
    {
        for (int i = keys.Count - 1; i >= 0; i--)
        {
            if (match(keys[i]))
            {
                keys.RemoveAt(i);
                values.RemoveAt(i);
            }
        }
    }
    
    public void RemoveAllValues(Predicate<TValue> match)
    {
        for (int i = values.Count - 1; i >= 0; i--)
        {
            if (match(values[i]))
            {
                keys.RemoveAt(i);
                values.RemoveAt(i);
            }
        }
    }
    
    public void RemoveAllKeys(Predicate<TKey> match)
    {
        for (int i = keys.Count - 1; i >= 0; i--)
        {
            if (match(keys[i]))
            {
                keys.RemoveAt(i);
                values.RemoveAt(i);
            }
        }
    }
    
    public void RemoveRange(int index, int count)
    {
        keys.RemoveRange(index, count);
        values.RemoveRange(index, count);
    }
    
    public void RemoveRangeValues(int index, int count)
    {
        keys.RemoveRange(index, count);
        values.RemoveRange(index, count);
    }
    
    public void RemoveRangeKeys(int index, int count)
    {
        keys.RemoveRange(index, count);
        values.RemoveRange(index, count);
    }
    
    public void Insert(int index, TKey key, TValue value)
    {
        keys.Insert(index, key);
        values.Insert(index, value);
    }
    
    public void InsertValue(int index, TValue value)
    {
        keys.Insert(index, default(TKey));
        values.Insert(index, value);
    }
    
    public void InsertKey(int index, TKey key)
    {
        keys.Insert(index, key);
        values.Insert(index, default(TValue));
    }
    
    public void InsertRange(int index, IEnumerable<TKey> keys, IEnumerable<TValue> values)
    {
        this.keys.InsertRange(index, keys);
        this.values.InsertRange(index, values);
    }
    
    public void InsertRangeValues(int index, IEnumerable<TValue> values)
    {
        keys.InsertRange(index, new TKey[values.Count()]);
        this.values.InsertRange(index, values);
    }
    
    public void InsertRangeKeys(int index, IEnumerable<TKey> keys)
    {
        this.keys.InsertRange(index, keys);
        values.InsertRange(index, new TValue[keys.Count()]);
    }
    
    public void SetAt(int index, TKey key, TValue value)
    {
        keys[index] = key;
        values[index] = value;
    }
    
    public void SetKey(int index, TKey key)
    {
        keys[index] = key;
    }
    
    public TKey GetKey(TValue value)
    {
        int index = values.IndexOf(value);
        if (index >= 0)
        {
            return keys[index];
        }
        throw new KeyNotFoundException(value.ToString());
    }
}

[Serializable]
public class EditorDictionary<TKey,TValue> : SerializableDictionary<TKey, TValue>, IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        return GetValues().GetEnumerator();
    }
}