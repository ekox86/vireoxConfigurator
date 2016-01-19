using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VireoxConfigurator
{
    public class Protocol : IDictionary<string, proprietaType>
    {
        Dictionary<string, proprietaType> propDefinitions;
        Dictionary<string, string> nsnvMap;
        proprietaType[] pList;
        string name;
        public Protocol(string n, proprietaType[] propList)
        {
            pList = propList;
            name = n;
            propDefinitions = new Dictionary<string, proprietaType>();
            nsnvMap = new Dictionary<string, string>();
            foreach (proprietaType p in propList)
            {
                if (propDefinitions.ContainsKey(p.NomeVisualizzato)) continue;
                propDefinitions.Add(p.NomeVisualizzato, p);
                if (nsnvMap.ContainsKey(p.NomeSalvato)) continue;
                nsnvMap.Add(p.NomeSalvato, p.NomeVisualizzato);
            }
        }
        public proprietaType[] PropertiesDef { get { return pList; } }
        public string Name { get { return name; } }
        public proprietaType this[string key]
        {
            get
            {
                return ((IDictionary<string, proprietaType>)propDefinitions)[key];
            }

            set
            {
                ((IDictionary<string, proprietaType>)propDefinitions)[key] = value;
            }
        }
        /// <summary>
        /// Converte il nome salvato nel vecchio file gprj nel nome visualizzato della proprietà (intestazione dei file csv)
        /// </summary>
        /// <param name="Nome della proprietà salvato nel file gprj"></param>
        /// <returns></returns>
        public string NStoNV(string nomesalvato)
        {
            if (!nsnvMap.ContainsKey(nomesalvato)) return null;
            return nsnvMap[nomesalvato];
        }
        public int Count
        {
            get
            {
                return ((IDictionary<string, proprietaType>)propDefinitions).Count;
            }
        } 

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary<string, proprietaType>)propDefinitions).IsReadOnly;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return ((IDictionary<string, proprietaType>)propDefinitions).Keys;
            }
        }

        public ICollection<proprietaType> Values
        {
            get
            {
                return ((IDictionary<string, proprietaType>)propDefinitions).Values;
            }
        }

        public void Add(KeyValuePair<string, proprietaType> item)
        {
            ((IDictionary<string, proprietaType>)propDefinitions).Add(item);
        }

        public void Add(string key, proprietaType value)
        {
            ((IDictionary<string, proprietaType>)propDefinitions).Add(key, value);
        }

        public void Clear()
        {
            ((IDictionary<string, proprietaType>)propDefinitions).Clear();
        }

        public bool Contains(KeyValuePair<string, proprietaType> item)
        {
            return ((IDictionary<string, proprietaType>)propDefinitions).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, proprietaType>)propDefinitions).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, proprietaType>[] array, int arrayIndex)
        {
            ((IDictionary<string, proprietaType>)propDefinitions).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, proprietaType>> GetEnumerator()
        {
            return ((IDictionary<string, proprietaType>)propDefinitions).GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, proprietaType> item)
        {
            return ((IDictionary<string, proprietaType>)propDefinitions).Remove(item);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, proprietaType>)propDefinitions).Remove(key);
        }

        public bool TryGetValue(string key, out proprietaType value)
        {
            return ((IDictionary<string, proprietaType>)propDefinitions).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, proprietaType>)propDefinitions).GetEnumerator();
        }

        public PropertyList getDefaultPropertyList()
        {
            PropertyList retlist = new PropertyList();
            PropertyItem pi;
            foreach (proprietaType pdef in propDefinitions.Values)
            {
                if (!pdef.Visibile) continue;
                //if (Regex.IsMatch(pdef.name, "SottoTipo|Tipo|NameNodo|Enabled|PathNode|Addres|GPMExportFile|Percorso", RegexOptions.IgnoreCase)) continue;
                pi = new PropertyItem(pdef.NomeVisualizzato, pdef.DefaultValue, pdef);
                retlist.Add(pi);
            }
            return retlist;
        }
    }
}
