using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PriorityQueue<TElement, TKey>
{
    private SortedDictionary<TKey, Queue<TElement>> dictionary = new SortedDictionary<TKey, Queue<TElement>>();

    public void Enqueue(TElement item, TKey key)
    {
        if (!dictionary.ContainsKey(key)) 
        {
            dictionary.Add(key, new Queue<TElement>());
        }

        dictionary[key].Enqueue(item);
    }

    public TElement Dequeue()
    {
        if (dictionary.Count == 0)
            throw new Exception("No items to Dequeue:");
        var key = dictionary.Keys.First();

        var queue = dictionary[key];
        var output = queue.Dequeue();
        if (queue.Count == 0)
            dictionary.Remove(key);

        return output;
    }

    public bool isEmpty() 
    {
        return dictionary.Count == 0;
    }
}