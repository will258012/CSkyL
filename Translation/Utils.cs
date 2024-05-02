using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using static ColossalFramework.Plugins.PluginManager;

namespace CSkyL.Translation
{
    internal class Utils
    {
        internal static string AssemblyPath {
            get {

                // No path cached - get list of currently active plugins.
                Assembly thisAssembly = Assembly.GetExecutingAssembly();
                IEnumerable<PluginInfo> plugins = PluginManager.instance.GetPluginsInfo();

                // Iterate through list.
                foreach (PluginInfo plugin in plugins) {
                    try {
                        // Iterate through each assembly in plugins
                        foreach (Assembly assembly in plugin.GetAssemblies()) {
                            if (assembly == thisAssembly) {
                                return plugin.modPath;
                            }
                        }
                    }
                    catch (Exception e) {
                        // Don't care; just don't let a single failure stop us iterating through the plugins.
                        Log.Err("Translator: " + e + "exception iterating through plugins");
                    }
                }

                // If we got here, then we didn't find the assembly.
                Log.Err("Translator: assembly path not found");
                return null;
            }
        }
        /// <summary>
        /// Read the "Language" key value pairs in the mod configuration file.
        /// In this way, when using `Translates.Translate` method for the first time, the language can be loaded correctly.
        /// </summary>
        /// <returns>
        /// Returns the language index from the configuration file. 
        /// Returns 0 if the "Language" node or the configuration file is not found.
        /// </returns>
        internal static int ConfigLanguageIndex {
            get {
                try {
                    string filePath = "FPSCameraConfig.xml";

                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePath);

                    XmlNode languageNode = doc.SelectSingleNode("/Config/Language");

                    if (languageNode != null) {
                        return int.Parse(languageNode.InnerText);
                    }
                    else {
                        return default;
                    }
                }
                catch {
                    return default;
                }
            }
        }
    }
}
