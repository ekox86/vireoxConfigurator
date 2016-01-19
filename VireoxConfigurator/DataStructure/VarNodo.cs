using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace VireoxConfigurator
{
    public class VarNodo : Nodo
    {
        public VarNodo() { }
        public VarNodo(string name, Nodo father, bool abilitazione) : base(name, father, abilitazione)
        {

        }

        public override void exportCSV(string file)
        {
            List<string> protocols = new List<string>();
            foreach (var p in ViewNode.GroupViewList)
            {
                protocols.Add(p.ProtocolName);
            }
            int depth = getMaxDepth() - 1;
            List<Variable> vars = new List<Variable>();
            getVariables(vars);
            FileManager.exportCSVVar(name,file,vars, protocols, depth);
        }

        public override Nodo Clone()
        {
            VarNodo n = new VarNodo(name, null, abilitazione);
            foreach (Nodo child in children)
            {
                n.Append((child.Clone()));
            }
            n.rebuild();
            return n;
        }    
    }
}
