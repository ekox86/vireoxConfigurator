using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VireoxConfigurator
{
    public class Variable : Nodo
    {
        public Variable(string name, Nodo father, bool abilitazione,int GMAddress) : base(name, father, abilitazione) {
            this.GMAddress = GMAddress;
        }
        int GMAddress;
        private int v;

        public Variable() { }
        
        public Variable(string name, Nodo father, bool abilitazione) : base(name, father, abilitazione)
        {
        }

        public Variable(int v)
        {
            name = "Nuova Variabile";
            GMAddress = v;
        }

        [XmlAttribute]        
        public int Address {
            get {
                return GMAddress;
            }
            set {
                GMAddress = value;
                OnPropertyChanged("Address");
            }
        }
        
        //public override void getLeaves(List<Flusso> lst,string filter)
        //{
        //    Flusso f=null;
        //    foreach (var child in children)
        //    {
        //        if (child.GetType() == typeof(Flusso))
        //        {
        //            f = child as Flusso;
        //            if (f.ProtocolName == filter)
        //                lst.Add(child as Flusso);
        //        }
        //    }
        //}
        internal void addFluxes(List<string> groups, List<List<string>> groupMap)
        {
            for (int i=0;i<groups.Count;i++)
            {
                string[] groupFields = groups[i].Split(';');
                int counter = 0;
                foreach (string field in groupFields)
                {
                    if (field != "") break;
                    counter++;
                }
                if (counter == groupFields.Length) continue; //salto le righe che non contengono nulla o che hanno nome vuoto                
                Children.Add(new Flusso(this,groupFields, groupMap[i]));
            }
        }
        public override Nodo Clone()
        {
            Variable v = new Variable(name, null, abilitazione, GMAddress);
            foreach (var child in children)
            {
                v.Append(child.Clone());
            }
            return v;

        }
        public override void getVariables(List<Variable> lst,bool checkEnabled=false)
        {
            if (checkEnabled && !Enabled) return;
            lst.Add(this);
        }

        public override int countVariables()
        {
            return 1;
        }
    }

}
