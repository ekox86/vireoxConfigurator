using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VireoxConfigurator
{
    public class NodoView
    {
        ObservableCollection<GroupView> fList;
        Nodo n;
        public NodoView(Nodo n)
        {
            this.n = n;
            fList = new ObservableCollection<GroupView>();
            List<Foglia> leaves=new List<Foglia>();
            n.getLeaves(leaves);
            IDictionary<string, Protocol> mapdef;
            if (leaves.Count == 0) return;
            if (leaves.First().GetType() == typeof(Flusso))
                mapdef = (VarDefinitions.Map);
            else
                mapdef = ComDefinitions.Map;
            foreach (var vp in mapdef)
            {
                List<Foglia> lst = leaves.FindAll(x => x.ProtocolName == vp.Key);
                if (lst.Count == 0) continue;
                fList.Add(new GroupView(vp, lst));
            }
        }
        public string Name { get { return n.Name; } }

        public GroupView First
        {
            get
            {
                if (fList.Count == 0) return null;
                else return fList[0];
            }
            set { }
        }

        public ObservableCollection<GroupView> GroupViewList { get { return fList; } }
    }

    public class GroupView
    {
        string protocolname;
        Protocol defs;
        List<Foglia> leaves;
        ObservableCollection<GroupPropertyItem> propertyValues;
        public GroupView(KeyValuePair<string,Protocol> vp,List<Foglia> lst)
        {
            propertyValues = new ObservableCollection<GroupPropertyItem>();
            defs = vp.Value;
            protocolname = vp.Key;
            leaves = lst;
            var v = leaves.First();
            foreach (var ptype in defs)
            {
                //controllo che il valore della proprietà sia uguale per tutti. Se è così, aggiungo al gruppo
                if (!ptype.Value.Visibile) continue;
                propertyValues.Add(new GroupPropertyItem(ptype.Key, v[ptype.Key], leaves.All(x => x[ptype.Key] == v[ptype.Key]), ptype.Value,leaves));
            }
        }
        public string ProtocolName
        {
            get { return protocolname; }
        }
        public ObservableCollection<GroupPropertyItem> propertylist
        {
            get
            {
                return propertyValues;
            }
            set
            {
                propertyValues = value;
            }
        }
        public string this[string index]
        {
            get
            {
                GroupPropertyItem gpi = propertyValues.FirstOrDefault(x => x.Name == index);
                if (gpi == null)
                    return null;
                else
                    return gpi.Name;
            }
            set
            {
                GroupPropertyItem gpi = propertyValues.FirstOrDefault(x => x.Name == index);
                if (gpi == null)
                    propertyValues.Add(new GroupPropertyItem(index, value, true, defs[index],leaves));
                else
                {
                    gpi.Value = value;
                    gpi.Common = true;
                }
                foreach(Flusso f in leaves)
                {
                    f[index] = value;                    
                }
            }
        }
        public override string ToString()
        {
            return protocolname;
        }
    }

    public class GroupPropertyItem:PropertyItem
    {
        bool common;
        List<Foglia> leaves;
        public bool Common { get { return common; } set { common = value; OnPropertyChanged("Common"); } }
        public GroupPropertyItem(string name, string value, bool common, proprietaType pt,List<Foglia> leaves) :base(name,value,pt)
        {
            this.common = common;
            this.leaves = leaves;
        }
        public override string Value { 
                get { return value; }
                set {                    
                    this.value = value;
                if (leaves != null)
                {
                    foreach (Foglia f in leaves)
                    {
                        f[name] = value;
                    }
                }
                    Common = true;
                    OnPropertyChanged("Value");
                }
            }
        }

    public class StringCollection : ObservableCollection<string>
    {
    }
}




