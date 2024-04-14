using System.Xml;

namespace CSkyL.Translation
{
    internal class Utils
    {
        /// <summary>
        /// Read the "Language" key value pairs in the mod configuration file.
        /// In this way, when using `Translates.Translate` method for the first time, the language can be loaded correctly.
        /// </summary>
        /// <returns>
        /// Returns the language index from the configuration file. 
        /// Returns 0 if the "Language" node is not found.
        /// </returns>
        internal static int GetConfigLanguageIndex()
        {
            string filePath = "FPSCameraConfig.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNode languageNode = doc.SelectSingleNode("/Config/Language");

            if (languageNode != null) {
                return int.Parse(languageNode.InnerText);
            }
            else {
                return 0;
            }

        }

    }
}
