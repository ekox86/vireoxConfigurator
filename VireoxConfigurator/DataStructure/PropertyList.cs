using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VireoxConfigurator
{
    public class PropertyList:IDictionary<string,string>,ICloneable
    {
        ObservableCollection<PropertyItem> pList;

        public PropertyList()
        {
            pList = new ObservableCollection<PropertyItem>();
        }
        public PropertyList(PropertyList p)
        {
            pList = new ObservableCollection<PropertyItem>();
            foreach (PropertyItem pi in p.pList)
            {
                pList.Add(new PropertyItem(pi));
            }
        }

        public string this[string key]
        {
            get
            {
                return pList.FirstOrDefault(x => x.Name == key).Value;
            }

            set
            {
                PropertyItem p= pList.FirstOrDefault(x => x.Name == key);
                if (p != null)
                    p.Value = value;
                else
                    Add(key, value);
            }
        }

        public int Count
        {
            get
            {
                return pList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return pList.Select(x => x.Name).ToList();
            }
        }

        public ObservableCollection<PropertyItem> Properties
        {
            get {
                return pList;
            }
            set { pList = value;
            }
        }

        public ICollection<string> Values
        {
            get
            {
                return pList.Select(x => x.Value).ToList();
            }
        }

        public void Add(KeyValuePair<string, string> item)
        {
            pList.Add(new PropertyItem(item.Key, item.Value));
        }
        public void Add(PropertyItem item)
        {
            pList.Add(item);
        }

        public void Add(string key, string value)
        {
            if (ContainsKey(key))
                throw new ArgumentException("Chiave già presente in elenco");
            pList.Add(new PropertyItem(key, value));
        }
        public void Add(string key, string value,proprietaType pt)
        {
            if (ContainsKey(key))
                throw new ArgumentException("Chiave già presente in elenco");
            pList.Add(new PropertyItem(key, value,pt));
        }
        public void Clear()
        {
            pList.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return pList.FirstOrDefault(x => x.Name == item.Key && x.Value == item.Value)!=null;
        }

        public bool ContainsKey(string key)
        {
            return pList.FirstOrDefault(x => x.Name == key) != null;
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            List<KeyValuePair<string, string>> lst = array.ToList();
            int i = arrayIndex;
            foreach (var item in pList)
            {
                lst.Insert(i, new KeyValuePair<string, string>(item.Name, item.Value));
                i++;
            }
            array = lst.ToArray();            
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            PropertyItem p = pList.FirstOrDefault(x => x.Name ==item.Key && x.Value==item.Value);
            if (p != null)
            {
                pList.Remove(p);
                return true;
            }
            else
            {
                throw new KeyNotFoundException("Proprietà " + item.Key + ":"+item.Value+ " non trovata.");
                return false;
            }
        }

        public bool Remove(string key)
        {
            PropertyItem p = pList.FirstOrDefault(x => x.Name == key);
            if (p != null)
            {
                pList.Remove(p);
                return true;
            }
            else
            {
                throw new KeyNotFoundException("Proprietà " + key + "non trovata.");
                return false;
            }
        }

        public bool TryGetValue(string key, out string value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return pList.GetEnumerator();
        }
        public PropertyItem getpItem(String name )
        {
            return pList.FirstOrDefault(x => x.Name == name);
        }

        public object Clone()
        {
            return new PropertyList(this);
        }
    }

  
}
