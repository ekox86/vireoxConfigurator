using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ionic.Zip;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace VireoxConfigurator
{
    class FileManager
    {
        /// <summary>
        /// Carica il progetto da un file di salvataggio (vecchio (.gprj) o nuovo (.xml)) nell'albero. 
        /// </summary>
        /// <param name="file">Path assoluto del file di salvataggio da caricare.</param>
        /// <returns></returns>
        public static Project openProject(string file)
        {
            Logger.Log("Inizio caricamento file  " + Path.GetFileName(file));
            Project p = null;
            if (!File.Exists(file))
                throw new FileManagerException("Il file " + file + "non esiste.");
            string ext = Path.GetExtension(file).ToLower();
            string filename = Path.GetFileNameWithoutExtension(file);
            if (ext == ".gprj")
                p = loadOldProjectFile(file);
            else if (ext == ".xml" || ext==".vrx")
                p = loadNew(file);
            else
                throw new FileManagerException("L'estensione del file non è stata riconosciuta");
            if (filename.Length > 3)
            {
                string rlsstring = filename.Substring(filename.Length - 3, 3);
                if (Regex.IsMatch(rlsstring, "r[0-9][0-9]", RegexOptions.IgnoreCase)) 
                     p.release = Int32.Parse(rlsstring.Substring(1, 2));
            }
            Logger.Log("Progetto:" +p.Properties["Nuovo progetto"], "Green");
            Logger.Log(p.VarTree.countVariables() + " variabili caricate","Green");
            Logger.Log(p.ComTree.countLeaves() + " canali caricati", "Green");
            return p;
        }
        internal static void exportCSVCom(string root,string file, List<Foglia> coms, List<string> protocols, int depth)
        {
            StreamWriter sw = null;
            StringBuilder line = new StringBuilder();
            try
            {
                sw = new StreamWriter(file);
                for (int i = 0; i < depth; i++)
                {
                    line.Append("Variabile;");
                }
                line.Append("#;");
                for (int i = 0; i < protocols.Count; i++)
                {
                    int nTimes = ComDefinitions.Map[protocols[i]].Values.Count(x => x.Visibile)+2;
                    for (int v = 0; v < nTimes; v++)
                    {
                        line.Append(protocols[i] + ";");
                    }
                    if (i != protocols.Count - 1)
                        line.Append("#;");
                }
                sw.WriteLine(line.ToString());
                line.Clear();
                for (int i = 0; i < depth; i++)
                    line.AppendFormat("Nome Variabile.{0};", i);
                line.Append("#;");
                for (int i = 0; i < protocols.Count; i++)
                {
                    line.Append("Abilitato;Nome;");
                    var properties = ComDefinitions.Map[protocols[i]].Values.Where(x => x.Visibile);
                    foreach (proprietaType p in properties)
                    {
                        line.AppendFormat("{0};", p.NomeVisualizzato);
                    }
                    if (i != protocols.Count - 1)
                        line.Append("#;");
                }
                sw.WriteLine(line.ToString());
                line.Clear();
                foreach (Canale com in coms)
                {
                    int index = com.Path.IndexOf(root + "~");
                    string[] path = com.Path.Substring(index).Split('~');                   
                    for (int i = 0; i < depth; i++)
                    {
                        if (i >= path.Length)
                            line.Append(";");
                        else
                            line.AppendFormat("{0};", path[i]);
                    }
                    line.Append("#;");
                    for (int i = 0; i < protocols.Count; i++)
                    {                        
                        if (protocols[i]!=com.ProtocolName)
                        {
                            line.Append(';', ComDefinitions.Map[protocols[i]].Values.Count(x => x.Visibile)+2);
                        }
                        else
                        {
                            var properties = ComDefinitions.Map[protocols[i]].Values.Where(x => x.Visibile);
                            line.AppendFormat("{0};{1};", com.Enabled,com.Name);
                            foreach (proprietaType p in properties)
                            {
                                if (com[p.NomeVisualizzato] != null)
                                    line.AppendFormat("{0};", com[p.NomeVisualizzato].Replace(";", "%sc%"));
                                else
                                    line.Append(";");
                            }
                        }
                        if (i != protocols.Count - 1)
                            line.Append("#;");
                    }
                    sw.WriteLine(line.ToString());
                    line.Clear();
                }
            }
            catch (Exception e)
            {
                throw new FileManagerException(e.Message);
                
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }    
        internal static void exportCSVVar(string root,string file,List<Variable> vars, List<string> protocols, int depth)
        {
            StreamWriter sw=null;
            StringBuilder line = new StringBuilder();
            try
            {
                sw = new StreamWriter(file);
                for (int i= 0;i < depth + 2; i++)
                {
                    line.Append("Variabile;");
                }
                line.Append("#;");
                for (int i=0;i<protocols.Count;i++)
                {
                    int nTimes = VarDefinitions.Map[protocols[i]].Values.Count(x => x.Visibile)+2;
                    for(int v=0;v<nTimes; v++)
                    {
                        line.Append(protocols[i] + ";");
                    }
                    if (i!=protocols.Count-1)
                        line.Append("#;");  
                }
                sw.WriteLine(line.ToString());
                line.Clear();
                for (int i = 0; i < depth; i++)
                    line.AppendFormat("Nome Variabile.{0};", i);
                line.Append("Indirizzo di memoria;Abilitazione Variabile;#;");
                for(int i=0;i<protocols.Count;i++)
                {
                    line.Append("Abilitato;Nome;");
                    var properties = VarDefinitions.Map[protocols[i]].Values.Where(x => x.Visibile);
                    foreach (proprietaType p in properties)
                    {
                        line.AppendFormat("{0};", p.NomeVisualizzato);
                    }
                    if (i != protocols.Count - 1)
                        line.Append("#;");
                }
                sw.WriteLine(line.ToString());
                line.Clear();
                foreach(Variable var in vars)
                {
                    int index = var.Path.IndexOf(root + "~");
                    string[] path = var.Path.Substring(index).Split('~');
                    for (int i = 0; i < depth; i++)
                    {
                        if (i >= path.Length)
                            line.Append(";");
                        else
                            line.AppendFormat("{0};", path[i]);
                    }
                    line.AppendFormat("{0};{1};#;", var.Address, var.Enabled);
                    for (int i=0;i<protocols.Count;i++)
                    {
                        Flusso f = var.Children.FirstOrDefault(x => ((Flusso)x).ProtocolName == protocols[i]) as Flusso;
                        if (f==null)
                        {
                            line.Append(';', VarDefinitions.Map[protocols[i]].Values.Count(x => x.Visibile)+2);
                        }
                        else
                        {
                            var properties = VarDefinitions.Map[protocols[i]].Values.Where(x => x.Visibile);
                            line.AppendFormat("{0};{1};", f.Enabled,f.Name);                                               
                            foreach (proprietaType p in properties)
                            {                                
                                if (f[p.NomeVisualizzato] != null)
                                    line.AppendFormat("{0};", f[p.NomeVisualizzato].Replace(";", "%sc%"));
                                else
                                    line.Append(";");
                            }
                        }
                        if (i != protocols.Count - 1)
                            line.Append("#;");
                    }
                    sw.WriteLine(line.ToString());
                    line.Clear();
                }
            }
            catch (Exception e)
            {
                throw new FileManagerException(e.Message);
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }        
        private static Project loadOldProjectFile(string file)
        {
            Project p = new Project(false);
            Logger.Log("Unzipping file " + file + "...");
            unzipFile(file);
            string projectFile;// = Path.GetDirectoryName(file) + "\\Temp\\" + Path.GetFileName(file);
            string directory = Path.GetDirectoryName(file);
            string[] files = Directory.GetFiles(Path.Combine (directory,"Temp"),"*.gprj");
            string scriptFile = Path.Combine(directory, "Temp\\user_logics.php");            
            projectFile = files[0];     
            StreamReader sr = null;
            int lineCounter = 0;
            try
            {
                sr = new StreamReader(projectFile);
                string line, section1, propValue, propName, path, prevPath = "";
                bool first = true;
                bool firstcom = true;
                bool firstOth = true;
                string[] lineFields, pathFields;
                Dictionary<string, string> nodeProps = new Dictionary<string, string>();
                int pathLength;
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (line.Length < 1) continue;
                    lineFields = line.Split(new string[] { "###" }, StringSplitOptions.None);
                    section1 = lineFields[0];
                    path = getPathString(section1);
                    propValue = lineFields[1];
                    pathFields = section1.Split(';');
                    pathLength = pathFields.Length;
                    propName = pathFields[pathLength - 1];
                    switch (pathFields[0])
                    {
                        case "Progetto":
                            if (propName.ToLower() == "false") continue;
                            if (!nodeProps.ContainsKey(propName))
                                nodeProps.Add(propName, propValue);
                            else
                                Logger.Log("Attenzione! Chiave " + propName + "ripetuta per il nodo " + path);
                            break;
                        case "Memoria Globale":
                            if (first)
                            {
                                p.addPropertiesGPRJ(nodeProps);
                                nodeProps.Clear();
                                prevPath = path;
                                first = false;
                            }
                            if (path == prevPath)
                            {
                                if (!nodeProps.ContainsKey(propName) && !Regex.IsMatch(propName, "false|true", RegexOptions.IgnoreCase) && !String.IsNullOrWhiteSpace(propValue))
                                    nodeProps.Add(propName, propValue);
                            }
                            else
                            {
                                p.VarTree.addVarNodeGPRJ(prevPath, nodeProps);
                                nodeProps.Clear();
                                nodeProps.Add(propName, propValue);
                            }
                            prevPath = path;
                            break;
                        case "Comunicazione":

                            if (firstcom)
                            {
                                p.VarTree.addVarNodeGPRJ(prevPath, nodeProps);
                                prevPath = path;
                                nodeProps.Clear();
                                firstcom = false;
                            }
                            if (path == prevPath)
                            {
                                if (!nodeProps.ContainsKey(propName) && !Regex.IsMatch(propName, "false|true", RegexOptions.IgnoreCase) && !String.IsNullOrWhiteSpace(propValue))
                                    nodeProps.Add(propName, propValue);
                            }
                            else
                            {
                                p.ComTree.addComNodeGPRJ(prevPath, nodeProps);
                                nodeProps.Clear();
                                nodeProps.Add(propName, propValue);
                            }
                            prevPath = path;
                            break;
                        case "Altro":
                            if (firstOth)
                            {
                                if (pathFields[1] == "User") continue;
                                p.ComTree.addComNodeGPRJ(prevPath, nodeProps);                               
                                prevPath = path;
                                nodeProps.Clear();
                                firstOth = false;
                            }
                            
                            if (path == prevPath)
                            {
                                if (!nodeProps.ContainsKey(propName) && !Regex.IsMatch(propName, "false|true", RegexOptions.IgnoreCase) && !String.IsNullOrWhiteSpace(propValue))
                                    nodeProps.Add(propName, propValue);
                            }
                            else
                            {
                                p.OtherTree.addReport61850NodeGPRJ(prevPath, nodeProps);
                                nodeProps.Clear();
                                nodeProps.Add(propName, propValue);
                            }
                            prevPath = path;
                            break;                            
                    }
                    lineCounter++;
                }
                sr.Close();
                loadOldScriptFile(p, scriptFile);

                FileManager.deleteDir(Path.GetDirectoryName(file) + "\\Temp");
            }
            catch (Exception e)
            {
                Logger.Log("Errore durante il caricamento del file " + file + " sulla linea: " + lineCounter + "-" + e.Message + " -" + e.StackTrace);
            }
            finally
            {
                if (sr != null) sr.Close();
            }
            return p;
        }

        private static void loadOldScriptFile(Project p, string scriptFile)
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(scriptFile);
                p.Properties["Script"] = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error reading old script file: " + e.Message + " "+e.StackTrace);
            }
            finally
            {
                sr.Close();
            }
        }

        static private string getPathString(string section1)
        {
            int startIndex = section1.IndexOf(';') + 1;
            int length = section1.LastIndexOf(';') - startIndex;
            return section1.Substring(startIndex, length);
        }
        public static Project loadNew(string file)
        {
            Project p = null;
            XmlSerializer xser = new XmlSerializer(typeof(Project));
            StreamReader sr = null;
            string xmlfile="";
            string ext = Path.GetExtension(file);
            bool vrx = true;
            try
            {
                if (ext.ToLower() == ".xml")
                {
                    xmlfile = file;
                    vrx = false;
                }
                else
                {
                    xmlfile = Path.Combine(Path.GetDirectoryName(file),unzipXML(file));
                    //unzipFile(file);
                }
                sr = new StreamReader(xmlfile);
                p = (Project)xser.Deserialize(sr);
                p.rebuild();
                p.updateEnabledProtocolList();
                p.setFreeGMAddress();
            }
            catch (Exception e)
            {
                throw new FileManagerException("Errore nel caricamento del file " + file + ": " + e.Message, e);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
                if (vrx)
                {
                    deleteFile(xmlfile);
                }
            }
            return p;
        }
        public static VarNodo importCSVVar(string fileName)
        {
            VarNodo retValue = new VarNodo();
            StreamReader sr = null; 
            List<List<string>> groupMap = new List<List<string>>();
            int varCount = 0;
            int lineCounter = 2;
            //Father.Children = new ObservableCollection<Nodo>();
            try
            {
                sr= new StreamReader(fileName);
                string[] intestazioni = sr.ReadLine().Split('#');
                List<string> gruppi = new List<string>();
                foreach (string i in intestazioni)
                {
                    //gruppi.Add(i.Substring(0, i.IndexOf(';')));
                    List<string> l = new List<string>();
                    l.Add(i.Split(';').ElementAt(1));
                    groupMap.Add(l);
                }
                intestazioni = sr.ReadLine().Split(new string[] { ";#;" }, StringSplitOptions.None);
                for (int j = 0; j < intestazioni.Length; j++)
                {
                    groupMap[j].AddRange(intestazioni[j].Split(';').ToList());
                }
                int nIndent = 0;
                foreach (string s in groupMap[0])       //conto il numero di indentazioni
                {
                    if (Regex.IsMatch(s, "Nome Variabile"))
                        nIndent++;
                }
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    List<string> groups = line.Split(new string[] { ";#;" }, StringSplitOptions.None).ToList();
                    Variable var = retValue.addVarCSV(groups[0], nIndent);
                    if (var != null) 
                        var.addFluxes(groups.GetRange(1, groups.Count - 1), groupMap.GetRange(1, groupMap.Count - 1));
                    varCount++;
                    lineCounter++;
                }
                Logger.Log(varCount + " variabili importate.");
            }
            catch (Exception e)
            {
                Logger.Log("Errore di importazione CSV - riga "+lineCounter+": " + e.Message, "Red");
            }
            finally
            {
                sr.Close();
            }
            return retValue;
        }
        public static ComNodo importCSVCOM(string fileName)
        {
            ComNodo retValue = new ComNodo();
            StreamReader sr = null;
            List<List<string>> groupMap = new List<List<string>>();
            int varCount = 0;
            try
            {
                sr = new StreamReader(fileName);
                string[] intestazioni = sr.ReadLine().Split('#');
                List<string> gruppi = new List<string>();
                foreach (string i in intestazioni)
                {
                    //gruppi.Add(i.Substring(0, i.IndexOf(';')));
                    List<string> l = new List<string>();
                    l.Add(i.Split(';').ElementAt(1));
                    groupMap.Add(l);
                }
                intestazioni = sr.ReadLine().Split(new string[] { ";#;" }, StringSplitOptions.None);
                for (int j = 0; j < intestazioni.Length; j++)
                {
                    groupMap[j].AddRange(intestazioni[j].Split(';').ToList());
                }
                int nIndent = 0;
                foreach (string s in groupMap[0])       //conto il numero di indentazioni
                {
                    if (Regex.IsMatch(s, "Nome Variabile"))
                        nIndent++;
                }
                while (!sr.EndOfStream)
                {

                    string line = sr.ReadLine();
                    List<string> groups = line.Split(new string[] { ";#;" }, StringSplitOptions.None).ToList();
                    retValue.addComCSV(groups, groupMap);
                    varCount++;
                }
                Logger.Log(varCount + " canali importati.");
            }
            catch (Exception e)
            {
                Logger.Log("Errore di importazione CSV: " + e.Message, "Red");
            }
            finally
            {
                sr.Close();
            }
            return retValue;
        }
        public static void generateConf(Project p,string dir=".\\Config")     //si può farne la versione multithreaded 
        {
            Logger.Log("Inizio esportazione...");
            string[] filestodelete = Directory.GetFiles(dir);
            foreach (string file in filestodelete) {
                if (Path.GetFileName(file).ToLower() == "user_logics.php") continue;
                deleteFile(file);                    
            }
            Logger.Log("Scrittura file user_logics.php...");
            writeScript(p, dir);
            writeProjectConfFile(p,dir);
            VarDefinitions vardefs = VarDefinitions.Map;
            List<Foglia> flussi = new List<Foglia>();
            p.VarTree.getLeaves(flussi,true);
            foreach (var vp in vardefs)
            {
                List<Foglia> nodidascrivere = flussi.FindAll(x => x.ProtocolName == vp.Key);
                if (nodidascrivere.Count != 0)
                {
                    writeVarConfFile(dir, nodidascrivere, vp);
                    if (vp.Key== "IEC 61850")
                    {
                        writeReports61850File(p, dir);
                    }
                }
            }
            flussi.Clear();
            List<Foglia> canali = new List<Foglia>();
            p.ComTree.getLeaves(canali,true);
            ComDefinitions comdefs = ComDefinitions.getDefinitions();
            foreach (var cd in comdefs)
            {
                List<Foglia> candascrivere = canali.FindAll(x => x.ProtocolName == cd.Key);
                if (candascrivere.Count != 0)
                {
                    writeComConfFile(dir, candascrivere, cd);
                }
            }
            writeStationFile(dir, p, 1);
            if (p.Properties.ContainsKey("Ridondanza Enabled") && bool.Parse(p.Properties["Ridondanza Enabled"])){
                string destfolder = p.Properties["BackupSharedFolder"]+"\\Config";
                copyDir(dir,destfolder,true);
                writeStationFile(destfolder, p, 2);
            }
        }

        private static void writeReports61850File(Project p, string dir)
        {
            if (p.Rep61850Tree.Children.Count == 0) return;
            Protocol definitions = VarDefinitions.Map["Reports61850"];
            string fileName = Path.Combine(dir, definitions["a"].DefaultValue);
            StreamWriter sw = null;
            List<Foglia> nodidascrivere = new List<Foglia>();
            string valueString;
            p.Rep61850Tree.getLeaves(nodidascrivere, true);
            Logger.Log("Scrittura file " + definitions["a"].DefaultValue + "...");
            try
            {
                sw = new StreamWriter(fileName);
                foreach (Report61850 f in nodidascrivere)
                {
                    sw.WriteLine("[" + f.Path + "]");
                    sw.WriteLine("Description=" + f.Name);
                    sw.WriteLine("GM.Address=NULL");
                    foreach (proprietaType defProp in definitions.Values)
                    {
                        if (defProp.NomeEsportazioneGPM == "false") continue;
                        if (defProp.name.ToLower() == "namenodo")
                            valueString = f.Name;
                        else if (defProp.name.ToLower() == "mastername")
                            valueString = f.Father.Name;
                        else
                            valueString = f[defProp.NomeVisualizzato];
                        if (String.IsNullOrWhiteSpace(valueString)) continue;
                        sw.WriteLine(defProp.NomeEsportazioneGPM + "=" + valueString);
                    }
                    sw.WriteLine("");
                }
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error writing file " + fileName, e);
            }
            finally
            {
                if (sw != null) sw.Close();
            }

        }

        public static void writeScript(Project p, string dir=".\\Config")
        {
            string fileName = Path.Combine(dir, "user_logics.php");
            StreamWriter sw = null;            
            try
            {
                sw = new StreamWriter(fileName);
                string script = p.Properties["Script"];
                byte[] ansiscriptbytes = Encoding.Convert(Encoding.UTF8, Encoding.Default, Encoding.UTF8.GetBytes(script));
                string ansiscript = Encoding.Default.GetString(ansiscriptbytes);
                sw.Write(ansiscript);
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error writing file " + fileName, e);
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }

        private static void writeProjectConfFile(Project p,string dir=".\\Config")
        {
            string fileName = Path.Combine(dir, ProjectDefinitions.Map["a"].DefaultValue);
            StreamWriter sw = null;
            Logger.Log("Scrittura file "+ ProjectDefinitions.Map["a"].DefaultValue+"...");
            try
            {
                sw = new StreamWriter(fileName);
                foreach (var group in ProjectDefinitions.Groups)
                {
                    sw.WriteLine("[" + group.Key + "]");
                    foreach (var pr in p.propertylist)
                    {
                        if (String.IsNullOrWhiteSpace(pr.Value)) continue;
                        proprietaType pt = group.Value.FirstOrDefault(x => x.NomeVisualizzato == pr.Name);
                        if (pt==null)
                            continue;
                        //proprietaType pdef = ProjectDefinitions.Map[pr.Name];
                        if (pt.NomeEsportazioneGPM.ToLower() == "false") continue;
                        sw.WriteLine(pt.NomeEsportazioneGPM + "=" + pr.Value);
                    }
                }
                sw.WriteLine("[Scheduler]");
                sw.WriteLine("LogLevel=1");
                sw.WriteLine("Enabled=true");
                sw.WriteLine("RequiredVesion=1.0.0.0");

            }
            catch (Exception e)
            {
                throw new FileManagerException("Error writing file " + fileName, e);
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }
        private static void writeStationFile(string dir, Project p, int stNumber)
        {
            string fileName = Path.Combine(dir, ProjectDefinitions.Map["a"].DefaultValue);
            string content = "[Main]\nStationNumber=";
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(Path.Combine(dir, "Station.cfg"));
                sw.Write(content + stNumber);
                sw.Close();
            }
            catch (Exception e) {
                throw new FileManagerException("Error writing file " + fileName, e);
            }
            finally { if (sw != null) sw.Close(); }
        }
        private static void writeComConfFile(string dir, List<Foglia> candascrivere, KeyValuePair<string, Protocol> cd)
        {
            Protocol definitions = cd.Value;
            string fileName = Path.Combine(dir, definitions["a"].DefaultValue);
            StreamWriter sw = null;
            Logger.Log("Scrittura file " + definitions["a"].DefaultValue + "...");
            string valueString;
            try
            {
                sw = new StreamWriter(fileName);
                foreach (Canale f in candascrivere)
                {
                    sw.WriteLine("[" + f.Path + "]");
                    sw.WriteLine("Description=" + f.Name);
                    //foreach (var property in f.propertylist)
                    //{
                    //    if (String.IsNullOrWhiteSpace(property.Value)) continue;
                    //    if (!definitions.ContainsKey(property.Name))
                    //        continue;
                    //    proprietaType defProp = definitions[property.Name];
                    //    if (defProp.NomeEsportazioneGPM == "false") continue;
                    //    sw.WriteLine(defProp.NomeEsportazioneGPM + "=" + property.Value);
                    //}
                    foreach (proprietaType defProp in definitions.Values)
                    {
                        if (defProp.NomeEsportazioneGPM == "false") continue;
                        if (defProp.name.ToLower() == "namenodo")
                            valueString = f.Name;
                        else
                            valueString = f[defProp.NomeVisualizzato];
                        if (String.IsNullOrWhiteSpace(valueString)) continue;
                        sw.WriteLine(defProp.NomeEsportazioneGPM + "=" + valueString);

                    }
                    sw.WriteLine("");
                }
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error writing file " + fileName, e);
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }
        public static void writeVarConfFile(string dir, List<Foglia> nodidascrivere, KeyValuePair<string, Protocol> vp)
        {
            Protocol definitions = vp.Value;
            string fileName = Path.Combine(dir, definitions["a"].DefaultValue);
            StreamWriter sw = null;
            Logger.Log("Scrittura file " + definitions["a"].DefaultValue + "...");
            string valueString;
            try
            {
                sw = new StreamWriter(fileName);
                foreach (Flusso f in nodidascrivere)
                {
                    sw.WriteLine("[" + f.Path + "]");
                    sw.WriteLine("Description=" + f.Father.Path.Substring(16));
                    sw.WriteLine("GM.Address=" + ((Variable)f.Father).Address);
                    foreach (proprietaType defProp in definitions.Values)
                    {
                        if (defProp.NomeEsportazioneGPM == "false") continue;
                        if (defProp.name.ToLower() == "namenodo")
                            valueString = f.Name;
                        else 
                            valueString = f[defProp.NomeVisualizzato];
                        if (String.IsNullOrWhiteSpace(valueString)) continue;
                        sw.WriteLine(defProp.NomeEsportazioneGPM + "=" + valueString);

                    }
                    sw.WriteLine("");
                }
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error writing file " + fileName, e);
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }
        /// <summary>
        /// Estrae tutto il contenuto di un file zip
        /// </summary>
        /// <param name="file">Path assoluto del file compresso</param>
        /// <param name="dir">Path assoluto della directory nella quale verrà estratto il contenuto del file. Se assente, sarà estratto nel path del file compresso.</param>
        public static void unzipFile(string file, string dir = null)
        {
            if (dir == null)
                dir = Path.GetDirectoryName(file);
            ZipFile zf = null;
            try
            {
                zf = new ZipFile(file);
                zf.ExtractAll(dir, ExtractExistingFileAction.OverwriteSilently);                
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error unzipping file " + file + ": " + e.Message, e);
            }
            finally
            {
                if (zf != null)
                    zf.Dispose();
            }
        }
        public static string unzipXML(string file, string dir=null)
        {
            if (dir == null)
                dir = Path.GetDirectoryName(file);
            ZipFile zf = null;
            try
            {
                zf = new ZipFile(file);
                var entry=zf.Entries.FirstOrDefault(x => x.FileName.EndsWith(".xml"));
                if (entry == null)
                    throw new FileNotFoundException("Nessun file xml trovato compresso nel file vrx");
                entry.Extract(dir,ExtractExistingFileAction.OverwriteSilently);
                return entry.FileName;               
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error unzipping file " + file + ": " + e.Message, e);                
            }
            finally
            {
                if (zf != null)
                    zf.Dispose();
            }
        }
        /// <summary>
        /// Comprime uno o più file o una directory
        /// </summary>
        /// <param name="files">Elenco dei path assoluti dei file da comprimere.</param>
        /// <param name="zipfileName"></param>
        public static void zipFile(string[] files, string zipfileName = null)
        {
            if (zipfileName == null)
                zipfileName = Path.GetFileNameWithoutExtension(files[0]) + ".zip";
            ZipFile zf = null;
            try
            {
                zf = new ZipFile(zipfileName);
                foreach (string file in files)
                {
                    if (Directory.Exists(file))
                        zf.AddDirectory(file);
                    else
                        zf.AddFile(file);
                }
                zf.Save();
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error creating zip file " + zipfileName + ": " + e.Message, e);
            }
            finally
            {
                if (zf != null)
                    zf.Dispose();
            }
        }
        public static void deleteDir(string dir)
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error deleting file " + dir, e);
            }
        }
        public static void save(string file, Project progetto)
        {
            XmlSerializer xSer;
            StreamWriter sw = null;
            string zipfilename = Path.GetFileNameWithoutExtension(file) + ".zip";
            string xmlfilename= Path.GetFileNameWithoutExtension(file) + DateTime.Now.ToString("yyyyMMddHHmmssfff")+".xml";
            try
            {
                if (File.Exists(xmlfilename))
                    deleteFile(xmlfilename);
                if (File.Exists(file))
                    deleteFile(file);
                List<Type> tlist = new List<Type>();
                tlist.Add(typeof(Nodo));
                tlist.Add(typeof(VarNodo));
                tlist.Add(typeof(ComNodo));
                tlist.Add(typeof(Report61850Nodo));
                tlist.Add(typeof(Report61850));
                tlist.Add(typeof(Foglia));
                tlist.Add(typeof(Canale));
                tlist.Add(typeof(Variable));
                tlist.Add(typeof(Flusso));
                tlist.Add(typeof(PropertyItem));
                xSer = new XmlSerializer(typeof(Project));
                sw = new StreamWriter(xmlfilename);
                xSer.Serialize(sw, progetto);
                sw.Close();
                sw = null;
                zipFile(new string[] { xmlfilename },zipfilename);
                File.Move(zipfilename, file);
                deleteFile(xmlfilename);                
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error saving file " + file + ":" + e.Message, e);
            }
            finally
            {
                if (sw != null) sw.Close();
            }

        }
        private static void deleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception e)
            {
                throw new FileManagerException("Error deleting xml project file.", e);
            }
        }
        private static void copyDir(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try { 
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }
                DirectoryInfo[] dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }
                //string[] filestodelete = Directory.GetFiles(destDirName);
                //foreach (string file in filestodelete)
                //{
                //    deleteFile(file);
                //}
                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, true);
                }

                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        copyDir(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }
            catch(Exception xe)
            {
                Logger.Log("Error copying into backup directory: "+xe.Message, "Red");
            }
            finally { }
    }
        public static ObservableCollection<string> getLastProjects()
        {
            ObservableCollection<string> retValue = new ObservableCollection<string>();
            if (!File.Exists("lru")) return retValue;
            StreamReader sr = null;
            string line;
            try
            {
                sr= new StreamReader("lru");
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    retValue.Add(line);
                }
            }
            catch (Exception e)
            {
                throw new FileManagerException("Errore nel caricamento della lista degli ultimi file aperti: "+ e.Message);               
            }
            finally
            {
                if (sr!=null) sr.Dispose();
            }
            return retValue;
        }
        public static void setLastProjects(ICollection<string> files)
        {
            StreamWriter sr = null;
            try
            {
                if (File.Exists("lru"))
                {
                    FileInfo fi = new FileInfo("lru");
                    fi.Attributes &= ~FileAttributes.Hidden;
                }
                sr = new StreamWriter("lru",false);
                foreach (string file in files)
                {
                    sr.WriteLine(file);
                }
                File.SetAttributes("lru", File.GetAttributes("lru") | FileAttributes.Hidden);
            }
            catch(Exception e)
            {
                throw new FileManagerException("Errore durante il salvataggio degli ultimi progetti aperti: " + e.Message);
            }
            finally
            {
                if (sr != null) sr.Close();
            }            
        }
    }
    [Serializable]
    class FileManagerException : Exception
    {
        public FileManagerException() { }

        public FileManagerException(string message) :base(message) { }

        public FileManagerException(string message, Exception inner) : base(message,inner) { }
    }
}
