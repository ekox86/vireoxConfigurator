using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VireoxConfigurator
{
    class ConfigChecker
    {
        Dictionary<string, Canale> channelMap;
        Dictionary<string, Variable> variables;
        List<Foglia> fluxes;
        string masterFolder, backupFolder;
        int mistakes=0;
        public int errors
        {
            get { return mistakes; }
            set
            {
                mistakes = value;
                if (mistakes>200)
                {
                    throw new ConfigCheckerException("Too many errors");
                }
            }
        }
        const int limErrors = 200;
        public ConfigChecker(Project p)
        {
            Logger.Log("Inizializzazione dello strumento di verifica...");
            channelMap = new Dictionary<string, Canale>();
            fluxes = new List<Foglia>();
            variables = new Dictionary<string, Variable>();
            List<Variable> ls = new List<Variable>();
            p.VarTree.getVariables(ls, true);
            foreach (Variable v in ls)
                variables.Add(v.Path, v);
            p.VarTree.getLeaves(fluxes, true);
            List<Foglia> lst = new List<Foglia>();
            p.ComTree.getLeaves(lst, true);
            foreach (Foglia f in lst)
            {
                try {
                    if (f.Enabled)
                        channelMap.Add(f.Name, (Canale)f);
                }
                catch (Exception e)
                {
                    Logger.Log("Errore: sono presenti due canali con lo stesso nome: " + f.Name,"Red");
                }
            }         
            //if (p.Properties["Ridondanza Enabled"].ToLower()=="true")
            //{
            //    mergeDirectory(p.Properties["MasterSharedFolder"], p.Properties["BackupSharedFolder"], "");
            //}

        }
        public void checkAll()
        {
            try {
                int errs = 0;
                Logger.Log("Inizio verifica configurazione...");
                Logger.Log("Inizio verifica GMAddress...");
                errs= checkGMAddress();
                Logger.Log("Verifica GMAddress: " + errs + " errori trovati.", errColor(errs));
                Logger.Log("Inizio verifica link tra variabili e comunicazioni...");
                errs=  checkVarComlink();
                Logger.Log("Verifica link tra variabili e comunicazioni: " + errs + " errori trovati.", errColor(errs));
                Logger.Log("Inizio verifica validità indirizzi 104");
                errs=  checkIOA104Presence();
                Logger.Log("Verifica validità indirizzi 104: " + errs + " errori trovati.", errColor(errs));
                Logger.Log("Inizio verifica univocità indirizzi 104 Master");
                errs=  checkIOA104MasterRepeated();
                Logger.Log("Verifica univocità indirizzi 104 Master: " + errs + " errori trovati.", errColor(errs));
                Logger.Log("Inizio verifica univocità indirizzi 104 Slave");
                errs=  checkIOA104SlaveRepeated();
                Logger.Log("Verifica univocità indirizzi 104 Slave: " + errs + " errori trovati.", errColor(errs));
                Logger.Log("Inizio verifica comandi 104");
                errs= checkCommand104();
                Logger.Log("Verifica comandi 104: " + errs + " errori trovati.", errColor(errs));
                Logger.Log("Inizio verifica univocità indirizzi Modbus Slave");
                errs= checkMBSlaveRepeated();
                Logger.Log("Verifica univocità indizzi Modbus Slave: " + errs + " errori trovati.", errColor(errs));
                Logger.Log("Inizio verifica univocità indirizzi Modbus Slave RTU");
                errs= checkMBRTUSlaveRepeated();
                Logger.Log("Verifica univocità indizzi Modbus Slave RTU: " + errs + " errori trovati.", errColor(errs));
                Logger.Log("Inizio verifica variabili copiate");
                errs= checkCopyVar();
                Logger.Log("Verifica variabili copiate: " + errs + " errori trovati.", errColor(errs));
                Logger.Log("Inizio verifica Modbus TCP Frame");
                errs= checkModbusTCPFrame();
                Logger.Log("Verifica modbus TCP Frame: " + errs + " errori trovati.", errColor(errs));
                System.Threading.Thread.Sleep(1000);
                Logger.Log("Totale errori trovati: " + errors, errColor(errors));
            }
            catch(ConfigCheckerException cce)
            {
                System.Threading.Thread.Sleep(1000);
                Logger.Log("Verifica interrotta. Superato il limite massimo di 200 errori.", "Red");               
            }
        }
        public int checkGMAddress()
        {
            int errs = 0;
            Dictionary<int, Variable> addressDict = new Dictionary<int, Variable>();
            foreach (Variable v in variables.Values)
            {
                if (addressDict.ContainsKey(v.Address))
                {
                    Logger.Log("Errore : GMAddress identico per  le variabili ");
                    Logger.Log(addressDict[v.Address].Path.Substring(16), "Purple", addressDict[v.Address]);
                    Logger.Log(v.Path.Substring(16), "Purple",v);
                    errs++;
                    errors++;
                }
                else
                    addressDict.Add(v.Address, v);
            }
            return errs;
        }
        public int checkCOMNames(Nodo cTree)
        {
            int errs = 0;
            List<Foglia> collisions, leaves = new List<Foglia>();
            cTree.getLeaves(leaves);
            foreach (Foglia f in leaves)
            {
                collisions = leaves.FindAll(x => x.Name == f.Name);
                if (collisions.Count <= 1) continue;
                Logger.Log("Channel names error: i seguenti canali hanno lo stesso nome","Red");
                foreach (Foglia vc in collisions)
                    Logger.Log(vc.Path,"Red");
                 errs++; errors++;
            }
            return errs;
        }
        public int checkVarComlink()
        {
            int errs = 0;
            foreach (Foglia f in fluxes)
            {
                if (f.ProtocolName == "Common" || f.ProtocolName == "Variables") continue;
                if (!channelMap.ContainsKey(f.Name))
                {
                    Logger.Log("Errore: canale inesistente per il flusso: ", "Red");
                    Logger.Log(f.Path, "purple",f);
                    errs++; errors++; 
                }
            }
            return errs;
        }
        public int checkIOA104Presence()
        {
            int errs = 0;
            var flussi104 = fluxes.FindAll(x => x.ProtocolName == "IEC 60870-5-104 Slave");
            string IOA = "", IOA1 = "", IOA2 = "", IOA3 = "";
            int IntIOA = 0, IntIOA1 = 0, IntIOA2 = 0, IntIOA3 = 0;
            foreach (Foglia f in flussi104)
            {
                IOA = f["IOA (totale)"];
                if (IOA == null || IOA == "")
                {
                    IOA1 = f["IOA 1"];
                    IOA2 = f["IOA 2"];
                    IOA3 = f["IOA 3"];
                    //--------------------------------
                    //-----------IOA 1----------------
                    if (IOA1 == null || IOA1 == "")
                    {
                        Logger.Log("Indirizzo IOA1 non presente:", "Red");
                        Logger.Log(f.Path.Substring(16), "purple",f);
                         errs++; errors++; 
                    }
                    else if (!Int32.TryParse(IOA1, out IntIOA1))
                    {
                        Logger.Log("Indirizzo IOA1 non definito correttamente:", "red");
                        Logger.Log(f.Path.Substring(16) + "\"", "purple",f);
                    }
                    else if (IntIOA1 < 0 || IntIOA1 > 255)
                    {
                        Logger.Log("Indirizzo IOA1 oltre i limiti; dev'essere compreso tra 0 e 255:", "red");
                        Logger.Log(f.Path.Substring(16), "purple",f);
                         errs++; errors++; 
                    }

                    //--------------------------------
                    //-----------IOA 2----------------
                    if (IOA2 == null || IOA2 == "")
                    {
                        Logger.Log("Indirizzo IOA2 non presente:", "red");
                        Logger.Log("\t\"" + f.Path.Substring(16) + "\"", "purple",f);
                         errs++; errors++; 
                    }
                    else if (!Int32.TryParse(IOA2, out IntIOA2))
                    {
                        Logger.Log("Indirizzo IOA2 non definito correttamente:", "red");
                        Logger.Log("\t\"" + f.Path.Substring(16) + "\"", "purple",f);
                         errs++; errors++;

                    }
                    else if (IntIOA2 < 0 || IntIOA2 > 255)
                    {
                        Logger.Log("Indirizzo IOA2 oltre i limiti; dev'essere compreso tra 0 e 255:", "red");
                        Logger.Log("\t\"" + f.Path.Substring(16) + "\"", "purple",f);
                         errs++; errors++; 
                    }

                    //--------------------------------
                    //-----------IOA 3----------------
                    if (IOA3 == null || IOA3 == "")
                    {
                        Logger.Log("Indirizzo IOA3 non presente:", "red");
                        Logger.Log("\t\"" + f.Path.Substring(16) + "\"", "purple",f);
                         errs++; errors++; 
                    }
                    else if (!Int32.TryParse(IOA3, out IntIOA3))
                    {
                        Logger.Log("Indirizzo IOA3 non definito correttamente:", "red");
                        Logger.Log("\t\"" + f.Path.Substring(16) + "\"", "purple",f);
                         errs++; errors++; 
                    }
                    else if (IntIOA3 < 0 || IntIOA3 > 255)
                    {
                        Logger.Log("Indirizzo IOA3 oltre i limiti; dev'essere compreso tra 0 e 255:", "red");
                        Logger.Log("\t\"" + f.Path.Substring(16) + "\"", "purple",f);
                         errs++; errors++; 
                    }
                }
                else
                {
                    if (!Int32.TryParse(IOA, out IntIOA))
                    {
                        Logger.Log("Indirizzo IOA non definito correttamente:", "red");
                        Logger.Log("\t\"" + f.Path.Substring(16) + "\"", "purple",f);
                         errs++; errors++; 
                    }
                    else if (IntIOA < 0 || IntIOA > 16777215)
                    {
                        Logger.Log("Indirizzo IOA oltre i limiti; dev'essere compreso tra 0 e 16777215:", "red");
                        Logger.Log("\t\"" + f.Path.Substring(16) + "\"", "purple",f);
                         errs++; errors++; 
                    }
                }
            }
            return errs;
        }
        public int checkIOA104MasterRepeated()
        {
            int errs = 0, ioa, casdu;
            Dictionary<string, Foglia> hashMap = new Dictionary<string, Foglia>();
            string key;
            var canali104 = channelMap.Values.Where(x => x.ProtocolName == "IEC 60870-5-104 Master");
            foreach (Foglia canale104 in canali104)
            {
                string styped = canale104["IOA Strong Typed"];
                bool strongtyped = false;
                if (styped == null) continue;
                strongtyped = Boolean.Parse(styped);
                List<Foglia> flussi104 = fluxes.FindAll(x => x.Name == canale104.Name);
                foreach (Foglia f in flussi104)
                {
                    key = "";
                    if (strongtyped) key = f["Type Information"];
                    ioa = getIOA(f);
                    if (ioa == -1) continue;
                    casdu = getCASDU(f);
                    if (casdu == -1) continue; 
                    key += casdu.ToString()+ioa.ToString();
                    if (hashMap.ContainsKey(key))
                    {
                        Logger.Log("IEC 60870-5-104 Master - Doppio Indirizzo CASDU IOA:", "red");
                        Logger.Log(f.Path.Substring(16), "purple",f);
                        Logger.Log(hashMap[key].Path.Substring(16), "purple", hashMap[key]);
                        errs++; errors++; 
                        continue;
                    }
                    hashMap.Add(key, f);
                }
                hashMap.Clear();
            }
            return errs;
        }
        public int checkIOA104SlaveRepeated()
        {
            int errs = 0, ioa, casdu;
            var canali104 = channelMap.Values.Where(x => x.ProtocolName == "IEC 60870-5-104 Slave");
            Dictionary<string, Foglia> hashMap = new Dictionary<string, Foglia>();
            string a104;
            foreach (Foglia canale104 in canali104)
            {
                List<Foglia> flussi104 = fluxes.FindAll(x => x.Name == canale104.Name);
                foreach (Foglia f in flussi104)
                {
                    ioa = getIOA(f);
                    if (ioa == -1) continue;
                    casdu = getCASDU(f);
                    if (casdu == -1) continue;
                    a104 = casdu.ToString();
                    a104 += ioa.ToString();
                    if (hashMap.ContainsKey(a104))
                    {
                        Logger.Log("IEC 60870-5-104 Slave - Doppio Indirizzo CASDU IOA:" , "red");
                        Logger.Log(f.Path.Substring(16), "purple",f);
                        Logger.Log(hashMap[a104].Path, "purple");
                         errs++; errors++; 
                        continue;
                    }
                    hashMap.Add(a104, f);
                }
                hashMap.Clear();
            }
            return errs;
        }
        public int checkCommand104()
        {
            int errs = 0, tinfo, on, off;
            List<Foglia> fluxMaster104 = fluxes.FindAll(x => x.ProtocolName == "IEC 60870-5-104 Master");
            foreach (Foglia f in fluxMaster104)
            {
                if (!Int32.TryParse(f["Type Information"], out tinfo)) continue;
                if (tinfo == 45 || tinfo == 56)
                {
                    if (!Int32.TryParse(f["GM to ON"], out on))
                    {
                        Logger.Log("IEC 104 - Errore Comandi il campo GM to ON non è settato: ", "Red");
                        Logger.Log(f.Path.Substring(16), "purple",f);
                         errs++; errors++; if (errors > limErrors) throw new ConfigCheckerException();
                    }
                    else if (!Int32.TryParse(f["GM to OFF"], out off))
                    {
                        Logger.Log("IEC 104 - Errore Comandi il campo GM to OFF non è settato: ", "Red");
                        Logger.Log(f.Path.Substring(16), "purple",f);
                        errs++; errors++; 
                    }
                }
            }
            return errs;
        }
        private int checkMBSlaveRepeated()
        {
            int errs = 0;
            Dictionary<string, Foglia> presenti = new Dictionary<string, Foglia>();
            var canaliMBSlave = channelMap.Values.Where(x => x.ProtocolName == "ModBus Slave");
            foreach (var canaleMBSlave in canaliMBSlave)
            {
                var fs = fluxes.FindAll(x => x.Name == canaleMBSlave.Name);
                foreach (Foglia f in fs)
                {
                    string key = f.Name + f["Indirizzo"] + f["Funzione"] + f["Stazione"];
                    if (presenti.ContainsKey(key))
                    {
                        Logger.Log("Modbus Slave - Doppio indirizzo:", "Red");
                        Logger.Log(f.Path.Substring(16), "purple",f);
                        Logger.Log(presenti[key].Path.Substring(16), "purple",presenti[key]);
                         errs++; errors++;
                    }
                    else
                        presenti.Add(key, f);
                }
                presenti.Clear();
            }
            return errs;
        }
        private int checkMBRTUSlaveRepeated()
        {
            int errs = 0;
            var canalimbrtuslave = channelMap.Values.Where(x => x.ProtocolName == "ModBus RTU Slave");
            Dictionary<string, Foglia> presenti = new Dictionary<string, Foglia>();
            foreach (var canalembrtu in canalimbrtuslave)
            {
                var fs = fluxes.FindAll(x => x.Name == canalembrtu.Name);
                foreach (Foglia f in fs)
                {
                    string key = f.Name + f["Indirizzo"] + f["Funzione"] + f["Bit Memoria Globale"] + f["Stazione"];
                    if (presenti.ContainsKey(key))
                    {
                        Logger.Log("Modbus Slave RTU - Doppio indirizzo:", "Red");
                        Logger.Log(f.Path, "purple",f);
                        Logger.Log(presenti[key].Path.Substring(16), "purple", presenti[key]);
                         errs++; errors++; 
                    }
                    else
                        presenti.Add(key, f);
                }
                presenti.Clear();
            }
            return errs;
        }
        private int checkCopyVar()
        {
            int errs = 0;
            List<Foglia> vs = fluxes.FindAll(x => x.ProtocolName == "Variables");
            foreach (var f in vs)
            {
                if (!String.IsNullOrEmpty(f["Copia da"]))
                {
                    if (!variables.ContainsKey("Memoria Globale~" + f["Copia da"]))
                    {
                        Logger.Log("Variabile per copia senza riferimento:");
                        Logger.Log(f.Path.Substring(16), "purple",f);
                        Logger.Log("-> "+f["Copia da"], "purple");
                         errs++; errors++;
                    }
                }
            }
            return errs;
        }
        public int checkModbusTCPFrame()
        {
            int errs = 0;
            List<Foglia> fs = fluxes.FindAll(x => x.ProtocolName == "ModBus TCP Frame");
            Canale ch;
            int startCh, lenCh, startVar;
            foreach (Foglia f in fs)
            {
                string keyfunction = f["Funzione di conversione"] == null ? "" : f["Funzione di conversione"].ToLower().Trim();
                if (!channelMap.ContainsKey(f.Name)) continue;
                ch = channelMap[f.Name];
                if (!Int32.TryParse(ch["Start"], out startCh) || !Int32.TryParse(ch["Len"], out lenCh)) {
                    Logger.Log("Canale Modbus TCP Frame non configurato correttamente: " + ch.Path, "Red");
                     errs++; errors++; if (errors > limErrors) throw new ConfigCheckerException();
                    continue;
                }
                if (!Int32.TryParse(f["Indirizzo"], out startVar))
                {
                    Logger.Log("Flusso Modbus TCP Frame non configurato correttamente: ", "Red");
                    Logger.Log(f.Path.Substring(16), "purple",f);
                     errs++; errors++; if (errors > limErrors) throw new ConfigCheckerException();
                    continue;
                }
                if (startVar >= startCh && startVar < startCh + lenCh)
                    continue;
                else
                {
                    Logger.Log("Flusso Modbus non contenuto nel Frame di riferimento: ", "Red");
                    Logger.Log(f.Path.Substring(16), "purple",f);
                    errs++; errors++;
                }
            }
            return errs;
        }
        //TODO: Controllare nelle variabili per copia che la variabile sorgente esista davvero.
        //TODO: Da controllare nel Modbus che la variabile sia dentro il Frame 
        private int getIOA(Foglia f)
        {
            int retvalue, tmp;
            string IOATOT = f["IOA (totale)"];
            if (IOATOT != null && IOATOT != "")
                return Int32.TryParse(IOATOT, out retvalue) ? retvalue : -1;
            string IOA1 = f["IOA 1"], IOA2 = f["IOA 2"], IOA3 = f["IOA 3"];
            if (IOA1 == null || IOA2 == null || IOA3 == null) return -1;
            if (!Int32.TryParse(IOA1, out tmp)) return -1;
            retvalue = tmp;
            if (!Int32.TryParse(IOA2, out tmp)) return -1;
            retvalue += tmp * 256;
            if (!Int32.TryParse(IOA3, out tmp)) return -1;
            retvalue += tmp * 256 * 256;
            return retvalue;
        }
        private int getCASDU(Foglia f)
        {
            int retvalue, tmp;
            string CASDUTOT = f["CASDU (totale)"];
            if (CASDUTOT != null && CASDUTOT != "")
                return Int32.TryParse(CASDUTOT, out retvalue) ? retvalue : -1;
            string CASDU1 = f["CASDU 1"], CASDU2 = f["CASDU 2"];
            if (CASDU1 == null || CASDU2 == null) return -1;
            if (!Int32.TryParse(CASDU1, out tmp)) return -1;
            retvalue = tmp;
            if (!Int32.TryParse(CASDU2, out tmp)) return -1;
            retvalue += tmp << 24;
            return retvalue;
        }


        string errColor(int errs)
        {
            return errs > 0 ? "Red" : "Green";
        }

    }


    [Serializable]
    class ConfigCheckerException : Exception
    {
        public ConfigCheckerException() { }

        public ConfigCheckerException(string message) : base(message) { }

        public ConfigCheckerException(string message, Exception inner) : base(message, inner) { }
    }

}
