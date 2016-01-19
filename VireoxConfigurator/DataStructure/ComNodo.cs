using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace VireoxConfigurator
{
    public class ComNodo : Nodo
    {
        public ComNodo() { }
        public ComNodo(string name, Nodo father, bool abilitazione) : base(name, father, abilitazione)
        {
        }
        public override void exportCSV(string file)
        {
            List<string> protocols = new List<string>();
            foreach (var p in ViewNode.GroupViewList)
            {
                protocols.Add(p.ProtocolName);
            }
            int depth = getMaxDepth();
            List<Foglia> coms = new List<Foglia>();
            getLeaves(coms);
            FileManager.exportCSVCom(name,file, coms, protocols, depth);
        }
        public override Nodo Clone()
        {
            ComNodo n = new ComNodo(name, null, abilitazione);
            foreach (Nodo child in children)
            {
                n.Append((child.Clone()));
            }
            n.rebuild();
            return n;
        }
    }
}
