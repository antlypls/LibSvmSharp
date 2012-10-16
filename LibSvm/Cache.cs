using System;

namespace LibSvm
{
  //
  // Kernel Cache
  //
  // length is the number of total data items
  // size is the cache size limit in bytes
  //
  internal class Cache
  {
    private readonly int _length;
    private long _size;

    private sealed class head_t
    {
      public head_t prev, next;     // a cicular list
      public double[] data;
      public int length;            // data[0,length) is cached in this entry
    }

    private readonly head_t[] head;
    private readonly head_t lru_head;

    public Cache(int length, long size)
    {
      _length = length;
      _size = size;
      head = new head_t[_length];
      for (int i = 0; i < _length; i++) head[i] = new head_t();
      _size /= 4;
      _size -= _length * (16 / 4);  // sizeof(head_t) == 16
      _size = Math.Max(_size, 2 * (long)_length);  // cache must be large enough for two columns
      lru_head = new head_t();
      lru_head.next = lru_head.prev = lru_head;
    }

    private static void lru_delete(head_t h)
    {
      // delete from current location
      h.prev.next = h.next;
      h.next.prev = h.prev;
    }

    private void lru_insert(head_t h)
    {
      // insert to last position
      h.next = lru_head;
      h.prev = lru_head.prev;
      h.prev.next = h;
      h.next.prev = h;
    }

    // request data [0,length)
    // return some position p where [p,length) need to be filled
    // (p >= length if nothing needs to be filled)
    public int get_data(int index, out double[] data, int length)
    {
      head_t h = head[index];
      if (h.length > 0) lru_delete(h);
      int more = length - h.length;

      if (more > 0)
      {
        // free old space
        while (_size < more)
        {
          head_t old = lru_head.next;
          lru_delete(old);
          _size += old.length;
          old.data = null;
          old.length = 0;
        }

        // allocate new space
        double[] new_data = new double[length];

        if (h.data != null) Array.Copy(h.data, 0, new_data, 0, h.length);
        h.data = new_data;
        _size -= more;

        Common.Swap(ref h.length, ref length);
      }

      lru_insert(h);
      data = h.data;
      return length;
    }

    public void swap_index(int i, int j)
    {
      if (i == j) return;

      if (head[i].length > 0) lru_delete(head[i]);
      if (head[j].length > 0) lru_delete(head[j]);

      Common.Swap(ref head[i].data, ref head[j].data);
      Common.Swap(ref head[i].length, ref head[j].length);

      if (head[i].length > 0) lru_insert(head[i]);
      if (head[j].length > 0) lru_insert(head[j]);

      if (i > j)
      {
        Common.Swap(ref i, ref j);
      }

      for (head_t h = lru_head.next; h != lru_head; h = h.next)
      {
        if (h.length > i)
        {
          if (h.length > j)
          {
            Common.Swap(ref h.data[i], ref h.data[j]);
          }
          else
          {
            // give up
            lru_delete(h);
            _size += h.length;
            h.data = null;
            h.length = 0;
          }
        }
      }
    }
  }
}
