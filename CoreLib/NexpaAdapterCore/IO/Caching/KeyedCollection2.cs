using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NexpaAdapterStandardLib.IO.Caching
{
    public class KeyedCollection2<TKey, TItem> : KeyedCollection<TKey, TItem>
    {
        private const string DelegateNullExceptionMessage = "Delegate passed cannot be null";
        private Func<TItem, TKey> _getKeyForItemDelegate;

        public KeyedCollection2(Func<TItem, TKey> getKeyForItemDelegate)
            : base()
        {
            if (getKeyForItemDelegate == null) throw new ArgumentNullException(DelegateNullExceptionMessage);
            _getKeyForItemDelegate = getKeyForItemDelegate;
        }

        public KeyedCollection2(Func<TItem, TKey> getKeyForItemDelegate, IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            if (getKeyForItemDelegate == null) throw new ArgumentNullException(DelegateNullExceptionMessage);
            _getKeyForItemDelegate = getKeyForItemDelegate;
        }

        protected override TKey GetKeyForItem(TItem item)
        {
            return _getKeyForItemDelegate(item);
        }

        public void SortByKeys()
        {
            var comparer = Comparer<TKey>.Default;
            SortByKeys(comparer);
        }

        public void SortByKeys(IComparer<TKey> keyComparer)
        {
            var comparer = new Comparer2<TItem>((x, y) => keyComparer.Compare(GetKeyForItem(x), GetKeyForItem(y)));
            Sort(comparer);
        }

        public void SortByKeys(Comparison<TKey> keyComparison)
        {
            var comparer = new Comparer2<TItem>((x, y) => keyComparison(GetKeyForItem(x), GetKeyForItem(y)));
            Sort(comparer);
        }

        public void Sort()
        {
            var comparer = Comparer<TItem>.Default;
            Sort(comparer);
        }

        public void Sort(Comparison<TItem> comparison)
        {
            var newComparer = new Comparer2<TItem>((x, y) => comparison(x, y));
            Sort(newComparer);
        }

        public void Sort(IComparer<TItem> comparer)
        {
            List<TItem> list = base.Items as List<TItem>;
            if (list != null)
            {
                list.Sort(comparer);
            }
        }
    }

    public class Comparer2<T> : Comparer<T>
    {
        //private readonly Func<T, T, int> _compareFunction;
        private readonly Comparison<T> _compareFunction;

        #region Constructors

        public Comparer2(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException("comparison");
            _compareFunction = comparison;
        }

        #endregion

        public override int Compare(T arg1, T arg2)
        {
            return _compareFunction(arg1, arg2);
        }
    }

    public class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator, IDisposable
    {
        readonly IEnumerator<KeyValuePair<TKey, TValue>> impl;
        public void Dispose() { impl.Dispose(); }
        public DictionaryEnumerator(IDictionary<TKey, TValue> value)
        {
            this.impl = value.GetEnumerator();
        }
        public void Reset() { impl.Reset(); }
        public bool MoveNext() { return impl.MoveNext(); }
        public DictionaryEntry Entry
        {
            get
            {
                var pair = impl.Current;
                return new DictionaryEntry(pair.Key, pair.Value);
            }
        }
        public object Key { get { return impl.Current.Key; } }
        public object Value { get { return impl.Current.Value; } }
        public object Current { get { return Entry; } }
    }
}
