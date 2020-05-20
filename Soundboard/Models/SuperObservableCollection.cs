using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundboard.Models
{
    public class SuperObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public PropertyChangedEventHandler ElementChanged { get; }

        public SuperObservableCollection(PropertyChangedEventHandler changedEvent)
        {
            ElementChanged = changedEvent;
            CollectionChanged += SuperObservableCollection_CollectionChanged;
        }

        public SuperObservableCollection(List<T> list, PropertyChangedEventHandler changedEvent)
            : base(list)
        {
            ElementChanged = changedEvent;
            CollectionChanged += SuperObservableCollection_CollectionChanged;

            foreach (T elem in list)
                elem.PropertyChanged += ElementChanged;
        }

        public SuperObservableCollection(IEnumerable<T> collection, PropertyChangedEventHandler changedEvent)
            : base(collection)
        {
            ElementChanged = changedEvent;
            CollectionChanged += SuperObservableCollection_CollectionChanged;

            foreach (T elem in collection)
                elem.PropertyChanged += ElementChanged;
        }

        private void SuperObservableCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                foreach (T elem in e.NewItems)
                    elem.PropertyChanged += ElementChanged;
            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                foreach (T elem in e.OldItems)
                    elem.PropertyChanged -= ElementChanged;
        }
    }
}
