﻿using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ColossalFramework.Globalization;
using ForestBrush.GUI;

namespace ForestBrush.TranslationFramework
{
    public delegate void LanguageChangedEventHandler(string languageIdentifier);

    /// <summary>
    /// Handles localisation for a mod.
    /// </summary>
    public class Translation
    {

        protected List<Language> _languages = new List<Language>();
        protected Language _currentLanguage = null;
        protected bool _languagesLoaded = false;
        protected bool _loadLanguageAutomatically = true;
        private const string fallbackLanguage = "en";

        private static Translation instance;

        public static Translation Instance
        {
            get
            {
                if (instance == null) instance = new Translation();
                return instance;
            }
        }

        private Translation(bool loadLanguageAutomatically = true)
        {
            _loadLanguageAutomatically = loadLanguageAutomatically;
            LocaleManager.eventLocaleChanged += SetCurrentLanguage;
        }

        private void SetCurrentLanguage()
        {
            if (_languages == null || _languages.Count == 0 || !LocaleManager.exists)
                return;
            _currentLanguage = _languages.Find(lang => lang._uniqueName == LocaleManager.instance.language) ??
                               _languages.Find(lang => lang._uniqueName == fallbackLanguage);
        }


        /// <summary>
        /// Loads all languages up if not already loaded.
        /// </summary>
        public void LoadLanguages()
        {
            if (!_languagesLoaded && _loadLanguageAutomatically)
            {
                RefreshLanguages();
                SetCurrentLanguage();
                _languagesLoaded = true;
            }
        }

        /// <summary>
        /// Forces a reload of the languages, even if they're already loaded
        /// </summary>
        public void RefreshLanguages()
        {
            _languages.Clear();
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
                                _languages.Add(loadedLanguage);

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

            foreach (Language availableLanguage in _languages)
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

            foreach (Language availableLanguage in _languages)
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

            foreach (Language availableLanguage in _languages)
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
            return _currentLanguage != null && _currentLanguage._conversionDictionary.ContainsKey(translationId);
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

            if (_currentLanguage != null)
            {
                if (HasTranslation(translationId))
                    translatedText = _currentLanguage._conversionDictionary[translationId];
                else UnityEngine.Debug.LogWarning("Returned translation for language \"" + _currentLanguage._uniqueName + "\" doesn't contain a suitable translation for \"" + translationId + "\"");
            }
            else UnityEngine.Debug.LogWarning("Can't get a translation for \"" + translationId + "\" as there is not a language defined");

            return translatedText;
        }
    }
}
