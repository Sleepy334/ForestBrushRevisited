using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ColossalFramework.Globalization;
using ForestBrushRevisited.GUI;

namespace ForestBrushRevisited.TranslationFramework
{
    public delegate void LanguageChangedEventHandler(string languageIdentifier);

    /// <summary>
    /// Handles localisation for a mod.
    /// </summary>
    public class Translation
    {
        protected List<Language> m_languages = new List<Language>();
        protected Language? m_currentLanguage = null;
        protected bool m_languagesLoaded = false;
        protected bool m_loadLanguageAutomatically = true;
        private const string fallbackLanguage = "en";

        private static Translation? instance = null;

        public static Translation Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Translation();
                }
                return instance;
            }
        }

        private Translation(bool loadLanguageAutomatically = true)
        {
            m_loadLanguageAutomatically = loadLanguageAutomatically;
            LocaleManager.eventLocaleChanged += SetCurrentLanguage;
        }

        private void SetCurrentLanguage()
        {
            if (m_languages == null || m_languages.Count == 0 || !LocaleManager.exists)
            {
                return;
            }
                
            m_currentLanguage = m_languages.Find(lang => lang._uniqueName == LocaleManager.instance.language) ??
                                m_languages.Find(lang => lang._uniqueName == fallbackLanguage);
        }


        /// <summary>
        /// Loads all languages up if not already loaded.
        /// </summary>
        public void LoadLanguages()
        {
            if (!m_languagesLoaded && m_loadLanguageAutomatically)
            {
                RefreshLanguages();
                SetCurrentLanguage();
                m_languagesLoaded = true;
            }
        }

        /// <summary>
        /// Forces a reload of the languages, even if they're already loaded
        /// </summary>
        public void RefreshLanguages()
        {
            m_languages.Clear();
            string basePath = TranslationUtil.AssemblyPath;

            if (basePath != "")
            {
                string languagePath = Path.Combine(basePath, "Locale");

                if (Directory.Exists(languagePath))
                {
                    string[] languageFiles = Directory.GetFiles(languagePath);

                    foreach (string languageFile in languageFiles)
                    {
                        using (StreamReader reader = new StreamReader(languageFile))
                        {
                            XmlSerializer xmlSerialiser = new XmlSerializer(typeof(Language));
                            if (xmlSerialiser.Deserialize(reader) is Language loadedLanguage)
                                m_languages.Add(loadedLanguage);

                            else UnityEngine.Debug.LogWarning($"Failed to Deserialize {languageFile}!");
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Locale Directory not found!");
                }
            }
            else UnityEngine.Debug.LogWarning("Mod Path was empty!");
        }

        /// <summary>
        /// Returns a list of languages which are available to the mod. This will return readable languages for use on the UI
        /// </summary>
        /// <returns>A list of languages available.</returns>
        public List<string> AvailableLanguagesReadable()
        {
            LoadLanguages();
            List<string> languageNames = new List<string>();

            foreach (Language availableLanguage in m_languages)
                languageNames.Add(availableLanguage._readableName);

            return languageNames;
        }

        /// <summary>
        /// Returns a list of languages which are available to the mod. This will return language IDs for searching.
        /// </summary>
        /// <returns>A list of languages available.</returns>
        public List<string> AvailableLanguages()
        {
            LoadLanguages();
            List<string> languageNames = new List<string>();

            foreach (Language availableLanguage in m_languages)
                languageNames.Add(availableLanguage._uniqueName);

            return languageNames;
        }

        /// <summary>
        /// Returns a list of Language unique IDs that have the name
        /// </summary>
        /// <param name="name">The name of the language to get IDs for</param>
        /// <returns>A list of IDs that match</returns>
        public List<string> GetLanguageIDsFromName(string name)
        {
            List<string> returnLanguages = new List<string>();

            foreach (Language availableLanguage in m_languages)
                if (availableLanguage._readableName == name)
                    returnLanguages.Add(availableLanguage._uniqueName);

            return returnLanguages;
        }

        /// <summary>
        /// Returns whether you can translate into a specific translation ID
        /// </summary>
        /// <param name="translationId">The ID of the translation to check</param>
        /// <returns>Whether a translation into this ID is possible</returns>
        public bool HasTranslation(string translationId)
        {
            LoadLanguages();
            return m_currentLanguage != null && m_currentLanguage._conversionDictionary.ContainsKey(translationId);
        }

        /// <summary>
        /// Gets a translation for a specific translation ID
        /// </summary>
        /// <param name="translationId">The ID to return the translation for</param>
        /// <returns>A translation of the translationId</returns>
        public string GetTranslation(string translationId)
        {
            LoadLanguages();
            string translatedText = translationId;

            if (m_currentLanguage != null)
            {
                if (HasTranslation(translationId))
                {
                    translatedText = m_currentLanguage._conversionDictionary[translationId];
                }
                else
                {
                    Debug.LogWarning("Returned translation for language \"" + m_currentLanguage._uniqueName + "\" doesn't contain a suitable translation for \"" + translationId + "\"");
                }
            }
            else
            {
                Debug.LogWarning("Can't get a translation for \"" + translationId + "\" as there is not a language defined");
            }

            return translatedText;
        }
    }
}
