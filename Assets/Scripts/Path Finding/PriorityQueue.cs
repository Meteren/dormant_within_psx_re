using System;
using System.Collections.Generic;

public class PriorityQueue<T> where T : IComparable<T>
{ 
    private List<T> queue = new List<T>();  

    public int Count { get =>  queue.Count; }  
    public void Enqueue(T item)
    {
        queue.Add(item);
        HeapifyUp(queue.Count - 1);
    }

    public T Dequeue()
    {
        T smallest = queue[0];
        queue[0] = queue[queue.Count - 1];
        queue.RemoveAt(queue.Count - 1);
        HeapifyDown(0);

        return smallest;
    }

    public bool Contains(T item)
    {
        if(queue.Count > 0)
            return queue.Contains(item);

        return false;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            parentIndex = parentIndex < 0 ? 0 : parentIndex;

            if (queue[index].CompareTo(queue[parentIndex]) >= 0)
                break;

            (queue[index], queue[parentIndex]) = (queue[parentIndex], queue[index]);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        int lastIndex = queue.Count - 1;

        while (true)
        {
            int leftChild = index * 2 + 1;
            int rightChild = index * 2 + 2;
            int smallest = index;

            if (leftChild <= lastIndex && queue[leftChild].CompareTo(queue[smallest]) < 0)
                smallest = leftChild;

            if (rightChild <= lastIndex && queue[rightChild].CompareTo(queue[smallest]) < 0)
                smallest = rightChild;

            if (smallest == index)
                break;

            (queue[index], queue[smallest]) = (queue[smallest], queue[index]);
            index = smallest;
        }
    }

}