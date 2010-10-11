using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Astral.Projector.Initiative
{
    public class InitiativeManager : IList<InitiativeEvent>, INotifyCollectionChanged
    {
        int _maxFuture;
        List<InitiativeEvent> _events = new List<InitiativeEvent>();

        public InitiativeManager(int turns)
        {
            _maxFuture = turns;
        }

        public InitiativeManager(int turns, params InitiativeEvent[] initialState)
            : this(turns)
        {
            _events.AddRange(initialState);
        }

        public int CurrentTurn { get; set; }

        public void MoveNext()
        {
            // move to the next point in time
        }

        public IEnumerator<InitiativeEvent> GetEnumerator()
        {
            for (int x = 0; x < _maxFuture; x++)
            {
                foreach (var item in _events)
                    yield return (InitiativeEvent)item.Clone();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {

            return this.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int IndexOf(InitiativeEvent item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, InitiativeEvent item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public InitiativeEvent this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(InitiativeEvent item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(InitiativeEvent item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(InitiativeEvent[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(InitiativeEvent item)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class InitiativeEvent : ICloneable
    {
        public string Name { get; set; }
        public int Initiative { get; set; }
        public string TeamImage { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class UnitInitiative : InitiativeEvent
    {
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
    }

    public class SpellFade : InitiativeEvent
    {
    }
}
