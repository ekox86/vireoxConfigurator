using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VireoxConfigurator 
{
    /// <summary>
    /// Classe singleton che contiene le definizioni di tutte le comunicazioni.
    /// </summary>
    class ComDefinitions : IDictionary<string, Protocol>
{
        internal static Dictionary<string, Protocol> defs;
        private static ComDefinitions instance;
        public Protocol this[string index]
        {
            get
            {
                if (defs.ContainsKey(index))
                    return defs[index];
                else
                    return null;
            }
        }

        Protocol IDictionary<string, Protocol>.this[string key]
        {
            get
            {
                return ((IDictionary<string, Protocol>)defs)[key];
            }

            set
            {
                ((IDictionary<string, Protocol>)defs)[key] = value;
            }
        }

        public static ComDefinitions Map {
            get
            {
                return getDefinitions();
            }
        }

        public int Count
        {
            get
            {
                return ((IDictionary<string, Protocol>)defs).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary<string, Protocol>)defs).IsReadOnly;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return ((IDictionary<string, Protocol>)defs).Keys;
            }
        }

        public ICollection<Protocol> Values
        {
            get
            {
                return ((IDictionary<string, Protocol>)defs).Values;
            }
        }

        /// <summary>
        /// Ritorna l'unica istanza dell'oggetto contenente tutte le definizioni delle comunicazioni
        /// </summary>
        /// <returns>Dizionario di oggetti ComProtocol</returns>
        public static ComDefinitions getDefinitions()
        {
            if (instance == null) instance = new ComDefinitions();
            return instance;
        }
        /// <summary>
        /// Carica le definizioni delle comunicazioni a partire dalla struttura dati ricavata dal file XML delle definizioni
        /// </summary>
        /// <param name="defStructure">Nodo radice della struttura dati del file definitions.xml</param>
        public static void load(root defStructure) {
            defs = new Dictionary<string, Protocol>();
            foreach (groupType nodo in defStructure.Comunicazioni)
            {
                ComProtocol comProt = new ComProtocol(nodo.name,nodo.proprieta);
                defs.Add(nodo.name, comProt);
            }
        }

        public void Add(KeyValuePair<string, Protocol> item)
        {
            ((IDictionary<string, Protocol>)defs).Add(item);
        }

        public void Add(string key, Protocol value)
        {
            ((IDictionary<string, Protocol>)defs).Add(key, value);
        }

        public void Clear()
        {
            ((IDictionary<string, Protocol>)defs).Clear();
        }

        public bool Contains(KeyValuePair<string, Protocol> item)
        {
            return ((IDictionary<string, Protocol>)defs).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, Protocol>)defs).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, Protocol>[] array, int arrayIndex)
        {
            ((IDictionary<string, Protocol>)defs).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, Protocol>> GetEnumerator()
        {
            return ((IDictionary<string, Protocol>)defs).GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, Protocol> item)
        {
            return ((IDictionary<string, Protocol>)defs).Remove(item);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, Protocol>)defs).Remove(key);
        }

        public bool TryGetValue(string key, out Protocol value)
        {
            return ((IDictionary<string, Protocol>)defs).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, Protocol>)defs).GetEnumerator();
        }
    }
    /// <summary>
    /// Classe  singleton che contiene le definizioni di tutti i protocolli.
    /// </summary>
    class VarDefinitions : IDictionary<string,Protocol>
    {
        internal static Dictionary<string, Protocol> defs;
        private static VarDefinitions instance;
        public static VarDefinitions Map
        {
            get
            {
                return getDefinitions();
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return ((IDictionary<string, Protocol>)defs).Keys;
            }
        }

        public ICollection<Protocol> Values
        {
            get
            {
                return ((IDictionary<string, Protocol>)defs).Values;
            }
        }

        public int Count
        {
            get
            {
                return ((IDictionary<string, Protocol>)defs).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary<string, Protocol>)defs).IsReadOnly;
            }
        }

        Protocol IDictionary<string, Protocol>.this[string key]
        {
            get
            {
                return ((IDictionary<string, Protocol>)defs)[key];
            }

            set
            {
                ((IDictionary<string, Protocol>)defs)[key] = value;
            }
        }

        public Protocol this[string index]
        {
            get
            {
                if (defs.ContainsKey(index))
                    return defs[index];
                else
                    return null;
            }
        }


        public static  VarDefinitions getDefinitions()
        {
            if (instance == null) instance = new VarDefinitions();
            return instance;
        }
        /// <summary>
        /// Carica in memoria le definizioni dei protocolli a partire dalla struttura dati estratta dal file definitions.xml
        /// </summary>
        /// <param name="defStructure">Nodo radice della struttura dati ricavata dal file definitions.xml</param>
        public static void load(root defStructure) {
            defs = new Dictionary<string, Protocol>();
            foreach (groupType nodo in defStructure.Protocolli)
            {
                VarProtocol varProt = new VarProtocol(nodo.name,nodo.proprieta);
                defs.Add(nodo.name, varProt);
            }
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, Protocol>)defs).ContainsKey(key);
        }

        public void Add(string key, Protocol value)
        {
            ((IDictionary<string, Protocol>)defs).Add(key, value);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, Protocol>)defs).Remove(key);
        }

        public bool TryGetValue(string key, out Protocol value)
        {
            return ((IDictionary<string, Protocol>)defs).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, Protocol> item)
        {
            ((IDictionary<string, Protocol>)defs).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<string, Protocol>)defs).Clear();
        }

        public bool Contains(KeyValuePair<string, Protocol> item)
        {
            return ((IDictionary<string, Protocol>)defs).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, Protocol>[] array, int arrayIndex)
        {
            ((IDictionary<string, Protocol>)defs).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, Protocol> item)
        {
            return ((IDictionary<string, Protocol>)defs).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, Protocol>> GetEnumerator()
        {
            return ((IDictionary<string, Protocol>)defs).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, Protocol>)defs).GetEnumerator();
        }
    }
    /// <summary>
    /// Classe  singleton contiene le definizioni delle proprietà del progetto.
    /// </summary>
    class ProjectDefinitions : IDictionary<string, proprietaType>
    {
        static Dictionary<string, proprietaType> defs;
        static Dictionary<string, string> nsnvMap;
        static Dictionary<string, List<proprietaType>> groups;
        static proprietaType[] pList;

        public static string NStoNV(string nomesalvato)
        {
            if (!nsnvMap.ContainsKey(nomesalvato)) return null;
            return nsnvMap[nomesalvato];
        }
        public proprietaType[] PropertiesDef { get { return pList; } }
        public static Dictionary<string,proprietaType> Map { get { return defs; } }
        public static void load(root defStructure)
        {
            defs = new Dictionary<string, proprietaType>();
            nsnvMap = new Dictionary<string, string>();
            groups=new Dictionary<string, List<proprietaType>>();
            proprietaType[] props = defStructure.Progetto.proprieta;
            pList = props;
            for(int i=0;i<props.Length;i++)
            {
                proprietaType p = props[i];
                defs.Add(p.NomeVisualizzato, p);
                if (!nsnvMap.ContainsKey(p.NomeSalvato))
                    nsnvMap.Add(p.NomeSalvato, p.NomeVisualizzato);
                if(!groups.ContainsKey(p.GruppoVisualizzazione))
                {
                    groups.Add(p.GruppoVisualizzazione, new List<proprietaType>());
                }
                groups[p.GruppoVisualizzazione].Add(p);
            }
        }
        public proprietaType this[string key]
        {
            get
            {
                return ((IDictionary<string, proprietaType>)defs)[key];
            }

            set
            {
                ((IDictionary<string, proprietaType>)defs)[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IDictionary<string, proprietaType>)defs).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary<string, proprietaType>)defs).IsReadOnly;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return ((IDictionary<string, proprietaType>)defs).Keys;
            }
        }

        public ICollection<proprietaType> Values
        {
            get
            {
                return ((IDictionary<string, proprietaType>)defs).Values;
            }
        }

        public static Dictionary<string,List<proprietaType>> Groups { get { return groups;  } }

        public void Add(KeyValuePair<string, proprietaType> item)
        {
            ((IDictionary<string, proprietaType>)defs).Add(item);
        }

        public void Add(string key, proprietaType value)
        {
            ((IDictionary<string, proprietaType>)defs).Add(key, value);
        }

        public void Clear()
        {
            ((IDictionary<string, proprietaType>)defs).Clear();
        }

        public bool Contains(KeyValuePair<string, proprietaType> item)
        {
            return ((IDictionary<string, proprietaType>)defs).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, proprietaType>)defs).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, proprietaType>[] array, int arrayIndex)
        {
            ((IDictionary<string, proprietaType>)defs).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, proprietaType>> GetEnumerator()
        {
            return ((IDictionary<string, proprietaType>)defs).GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, proprietaType> item)
        {
            return ((IDictionary<string, proprietaType>)defs).Remove(item);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, proprietaType>)defs).Remove(key);
        }

        public bool TryGetValue(string key, out proprietaType value)
        {
            return ((IDictionary<string, proprietaType>)defs).TryGetValue(key, out value);
        }

        public static PropertyList getDefaultPropertyList()
        {
            PropertyList retlist = new PropertyList();
            PropertyItem pi;
            foreach (proprietaType pdef in defs.Values)
            {
                if (!pdef.Visibile) continue;
                //if (Regex.IsMatch(pdef.name, "SottoTipo|Tipo|NameNodo|Enabled|PathNode|Addres|GPMExportFile|Percorso", RegexOptions.IgnoreCase)) continue;
                pi = new PropertyItem(pdef.NomeVisualizzato, pdef.DefaultValue, pdef);
                retlist.Add(pi);
            }
            return retlist;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, proprietaType>)defs).GetEnumerator();
        }
    }

}

