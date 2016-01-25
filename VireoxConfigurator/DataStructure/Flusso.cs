using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.ObjectModel;

namespace VireoxConfigurator
{
    public class Flusso : Foglia
    {   //il nome del flusso coincide con quello del canale associato
        public Flusso(string name, Nodo father, bool abilitazione) : base(name, father, abilitazione)
        {
        }
        public Flusso() { }
        public Flusso(Variable father, string[] fields, List<string> map)
        {
            this.father = father;
            protocolname = map[0];
            propertyDefinitions = VarDefinitions.Map[map[0]];
            propertyValues = new PropertyList();
            if (Regex.IsMatch(map[0], "Variables|Common", RegexOptions.IgnoreCase))
                name = map[0];
            else
                name = fields[1];

            name = name.Trim();
            foreach (var pt in propertyDefinitions)
            {
                if (!pt.Value.Visibile) continue;
                int i = map.FindIndex(x => x == pt.Key);
                if (pt.Key=="Abilitato")
                {
                    if (!Boolean.TryParse(fields[i-1], out abilitazione)) abilitazione = false;
                }
                if (i > 0)
                {
                    propertyValues.Add(pt.Key, fields[i - 1].Replace("%sc%",";").Trim(),pt.Value);
                }
                else
                    propertyValues.Add(pt.Key, "", pt.Value);
            }   
        }

        [XmlAttribute]
        public override string ProtocolName
        {
            get { return protocolname; }
            set
            {
                protocolname = value;
                if (VarDefinitions.Map.ContainsKey(protocolname))
                {
                    propertyDefinitions = VarDefinitions.Map[protocolname];
                }
                OnPropertyChanged("ProtocolName");
            }
        }
        public override void setDefaultPropertyList()
        {
            propertyValues = propertyDefinitions.getDefaultPropertyList();
        }
        public override Nodo Clone()
        {
            Flusso f = new Flusso(name, null, true);
            f.propertyDefinitions = propertyDefinitions;
            f.protocolname = protocolname;
            f.abilitazione = abilitazione;
            f.propertyValues = (PropertyList)propertyValues.Clone();
            return f;
        }
    }
}


