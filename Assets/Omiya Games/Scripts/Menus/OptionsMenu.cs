﻿using UnityEngine;
using UnityEngine.UI;

namespace OmiyaGames
{
    ///-----------------------------------------------------------------------
    /// <copyright file="OptionsMenu.cs" company="Omiya Games">
    /// The MIT License (MIT)
    /// 
    /// Copyright (c) 2014-2016 Omiya Games
    /// 
    /// Permission is hereby granted, free of charge, to any person obtaining a copy
    /// of this software and associated documentation files (the "Software"), to deal
    /// in the Software without restriction, including without limitation the rights
    /// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    /// copies of the Software, and to permit persons to whom the Software is
    /// furnished to do so, subject to the following conditions:
    /// 
    /// The above copyright notice and this permission notice shall be included in
    /// all copies or substantial portions of the Software.
    /// 
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    /// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    /// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    /// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    /// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    /// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    /// THE SOFTWARE.
    /// </copyright>
    /// <author>Taro Omiya</author>
    /// <date>8/18/2015</date>
    ///-----------------------------------------------------------------------
    /// <summary>
    /// Menu that provides options.  Currently only supports changing sound
    /// and music volume. You can retrieve this menu from the singleton script,
    /// <code>MenuManager</code>.
    /// </summary>
    /// <seealso cref="MenuManager"/>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SoundEffect))]
    public class OptionsMenu : IMenu
    {
        public const float MinimumDisplayedVolume = 0.01f;
        public const float MaximumDisplayedVolume = 1f;

        [System.Serializable]
        public struct AudioControls
        {
            [SerializeField]
            GameObject[] controlParents;
            [SerializeField]
            Slider volumeSlider;
            [SerializeField]
            Text volumePercentLabel;
            [SerializeField]
            Toggle checkBoxMark;

            public void Update(float volume, bool mute)
            {
                VolumeSlider.value = volume;
                VolumePercentLabel.text = Percent(volume);
                VolumeSlider.interactable = !mute;
                CheckBoxMark.isOn = mute;
            }

            public Slider VolumeSlider
            {
                get
                {
                    return volumeSlider;
                }
            }

            public Text VolumePercentLabel
            {
                get
                {
                    return volumePercentLabel;
                }
            }

            public Toggle CheckBoxMark
            {
                get
                {
                    return checkBoxMark;
                }
            }

            public float MinValue
            {
                get
                {
                    return VolumeSlider.minValue;
                }
            }

            public float MaxValue
            {
                get
                {
                    return VolumeSlider.maxValue;
                }
            }

            public bool IsActive
            {
                get
                {
                    bool returnFlag = false;
                    foreach(GameObject control in controlParents)
                    {
                        if(control != null)
                        {
                            returnFlag = control.activeSelf;
                            break;
                        }
                    }
                    return returnFlag;
                }
                set
                {
                    foreach(GameObject control in controlParents)
                    {
                        if(control != null)
                        {
                            control.SetActive(value);
                        }
                    }
                }
            }
        }

        [System.Serializable]
        public struct SensitivityControls
        {
            [SerializeField]
            GameObject controlParent;
            [SerializeField]
            Slider slider;
            [SerializeField]
            Text percentLabel;

            public void Update(float sensitivity)
            {
                SensitivitySlider.value = sensitivity;
                SensitivityPercentLabel.text = Percent(sensitivity);
            }

            public Slider SensitivitySlider
            {
                get
                {
                    return slider;
                }
            }

            public Text SensitivityPercentLabel
            {
                get
                {
                    return percentLabel;
                }
            }

            public float MinValue
            {
                get
                {
                    return SensitivitySlider.minValue;
                }
            }

            public float MaxValue
            {
                get
                {
                    return SensitivitySlider.maxValue;
                }
            }

            public bool IsActive
            {
                get
                {
                    return controlParent.activeSelf;
                }
                set
                {
                    controlParent.SetActive(value);
                }
            }
        }

        [System.Serializable]
        public struct ToggleControls
        {
            [SerializeField]
            GameObject controlParent;
            [SerializeField]
            Toggle toggle;

            public bool IsInverted
            {
                get
                {
                    return toggle.isOn;
                }
                set
                {
                    toggle.isOn = value;
                }
            }

            public bool IsActive
            {
                get
                {
                    return controlParent.activeSelf;
                }
                set
                {
                    controlParent.SetActive(value);
                }
            }
        }

        [System.Serializable]
        public struct CompoundSensitivityControls
        {
            [SerializeField]
            ToggleControls splitAxisToggle;
            [SerializeField]
            SensitivityControls overallSensitivity;
            [SerializeField]
            SensitivityControls xAxisSensitivity;
            [SerializeField]
            SensitivityControls yAxisSensitivity;

            public void Update(bool splitSensitivity, float xSensitivity, float ySensitivity)
            {
                splitAxisToggle.IsInverted = splitSensitivity;

                OverallSensitivity.Update(xSensitivity);
                xAxisSensitivity.Update(xSensitivity);
                yAxisSensitivity.Update(ySensitivity);
                
                UpdateAxisSensitivityControls();
            }

            public SensitivityControls OverallSensitivity
            {
                get
                {
                    return overallSensitivity;
                }
            }

            public SensitivityControls XAxisSensitivity
            {
                get
                {
                    return xAxisSensitivity;
                }
            }

            public SensitivityControls YAxisSensitivity
            {
                get
                {
                    return yAxisSensitivity;
                }
            }

            public bool IsAxisSplit
            {
                get
                {
                    return splitAxisToggle.IsInverted;
                }
                // set
                // {
                //     splitAxisToggle.IsInverted = value;
                //     UpdateAxisSensitivityControls();
                // }
            }

            public bool IsActive
            {
                get
                {
                    return splitAxisToggle.IsActive;
                }
                set
                {
                    splitAxisToggle.IsActive = value;
                    UpdateAxisSensitivityControls();
                }
            }
            
            public void UpdateAxisSensitivityControls()
            {
                if(splitAxisToggle.IsActive == false)
                {
                    xAxisSensitivity.IsActive = false;
                    yAxisSensitivity.IsActive = false;
                    overallSensitivity.IsActive = false;
                }
                else if(splitAxisToggle.IsInverted == true)
                {
                    xAxisSensitivity.IsActive = true;
                    yAxisSensitivity.IsActive = true;
                    overallSensitivity.IsActive = false;
                }
                else
                {
                    overallSensitivity.IsActive = true;
                    xAxisSensitivity.IsActive = false;
                    yAxisSensitivity.IsActive = false;
                }
            }
        }

        #region Serialized Fields
        // FIXME: Add more booleans based on new fields!
        [Header("Features to Enable")]
        [SerializeField]
        bool enableLanguageControls = true;
        [SerializeField]
        bool enableMusicControls = true;
        [SerializeField]
        bool enableSoundEffectControls = true;
        [SerializeField]
        bool enableKeyboardSensitivityControls = true;
        [SerializeField]
        bool enableMouseSensitivityControls = true;
        [SerializeField]
        bool enableScrollWheelSensitivityControls = true;
        [SerializeField]
        bool enableKeyboardInvertedControls = true;
        [SerializeField]
        bool enableMouseInvertedControls = true;
        [SerializeField]
        bool enableScrollWheelInvertedControls = true;
        [SerializeField]
        bool enableResetDataButton = true;

        [Header("Language Controls")]
        [SerializeField]
        LanguageDropDown languageDropDown;
        [SerializeField]
        GameObject[] languageParents;

        [Header("Audio Controls")]
        [SerializeField]
        AudioControls musicControls;
        [SerializeField]
        AudioControls soundEffectsControls;
        [SerializeField]
        GameObject audioDividers;

        [Header("Special Effects Controls")]
        [SerializeField]
        ToggleControls flashesControls;
        [SerializeField]
        ToggleControls motionBlursControls;
        [SerializeField]
        GameObject[] specialEffectsParents;

        [Header("Mouse Sensitivity")]
        [SerializeField]
        CompoundSensitivityControls keyboardSensitivity;
        [SerializeField]
        CompoundSensitivityControls mouseSensitivity;
        [SerializeField]
        SensitivityControls scrollWheelSensitivity;
        [SerializeField]
        GameObject[] mouseSensitivityLabelsAndDividers;

        [Header("Mouse Sensitivity")]
        [SerializeField]
        ToggleControls keyboardXInvert;
        [SerializeField]
        ToggleControls keyboardYInvert;
        [SerializeField]
        ToggleControls mouseXInvert;
        [SerializeField]
        ToggleControls mouseYInvert;
        [SerializeField]
        ToggleControls scrollWheelInvert;

        [SerializeField]
        GameObject[] mouseInvertLabelsAndDividers;

        [Header("Other controls")]
        [SerializeField]
        GameObject resetAllDataParent;
        #endregion

        SoundEffect audioCache;
        bool inSetupMode = false;

        System.Action<OptionsMenu> hideAction = null;

        #region Properties
        public SoundEffect TestSoundEffect
        {
            get
            {
                if(audioCache == null)
                {
                    audioCache = GetComponent<SoundEffect>();
                }
                return audioCache;
            }
        }

        GameSettings settings
        {
            get
            {
                return Singleton.Get<GameSettings>();
            }
        }

        public override Type MenuType
        {
            get
            {
                return Type.ManagedMenu;
            }
        }

        public override GameObject DefaultUi
        {
            get
            {
                return musicControls.VolumeSlider.gameObject;
            }
        }
        #endregion

        void Start()
        {
            // Setup controls
            inSetupMode = true;

            // Update how the languages are enabled
            SetupLanguageControls();

            // Update how music controls are enabled
            SetupAudioControls();

            // Update how sensitivity controls are enabled
            SetupSensitivityControls();

            // Update how invert controls are enabled
            SetupInvertControls();

            // Update whether the rest of the controls are enabled
            SetupOtherControls();
            inSetupMode = false;
        }

        protected override void OnStateChanged(IMenu.State from, IMenu.State to)
        {
            // Call the base method
            base.OnStateChanged(from, to);

            if ((from == State.Visible) && (to == State.Hidden))
            {
                // Run the last action
                if (hideAction != null)
                {
                    hideAction(this);
                    hideAction = null;
                }
            }
        }

        #region UI events
        public override void Hide()
        {
            base.Hide();

            // Indicate button is clicked
            Manager.ButtonClick.Play();
        }

        public void OnLanguageSeleced(int selectedIndex)
        {
            if ((inSetupMode == false) && (selectedIndex >= 0))
            {
                // Grab the translator
                TranslationManager translator = Singleton.Get<TranslationManager>();
                if((translator != null) && (selectedIndex < translator.SupportedLanguages.Count))
                {
                    // Change the language
                    translator.CurrentLanguage = translator.SupportedLanguages[selectedIndex];
                }

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }

        #region Music Group
        public void OnMusicSliderChanged(float sliderValue)
        {
            if (inSetupMode == false)
            {
                BackgroundMusic.GlobalVolume = sliderValue;
                musicControls.VolumePercentLabel.text = Percent(sliderValue);
            }
        }

        public void OnMusicMuteToggled(bool mute)
        {
            if (inSetupMode == false)
            {
                // Toggle mute
                BackgroundMusic.GlobalMute = mute;

                // disable the slider
                musicControls.VolumeSlider.interactable = !mute;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }
        #endregion

        #region Sound Effects Group
        public void OnSoundEffectsSliderChanged(float sliderValue)
        {
            if (inSetupMode == false)
            {
                SoundEffect.GlobalVolume = sliderValue;
                soundEffectsControls.VolumePercentLabel.text = Percent(sliderValue);
            }
        }
        
        public void OnSoundEffectsSliderPointerUp()
        {
            TestSoundEffect.Play();
        }

        public void OnSoundEffectsMuteToggled(bool mute)
        {
            if (inSetupMode == false)
            {
                // Toggle mute
                SoundEffect.GlobalMute = mute;

                // disable the slider
                soundEffectsControls.VolumeSlider.interactable = !mute;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }
        #endregion

        #region Special Effects Group
        public void OnEnableFlashesToggled(bool enable)
        {
            if (inSetupMode == false)
            {
                // Toggle mute
                settings.IsFlashesEnabled = enable;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }

        public void OnEnableMotionBlursToggled(bool enable)
        {
            if (inSetupMode == false)
            {
                // Toggle mute
                settings.IsMotionBlursEnabled = enable;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }
        #endregion

        #region Keyboard Sensitivity
        public void OnSplitKeyboardAxisToggled(bool splitAxis)
        {
            if (inSetupMode == false)
            {
                // Store this settings
                settings.SplitKeyboardAxis = splitAxis;

                // Toggle which sliders will be showing up
                keyboardSensitivity.UpdateAxisSensitivityControls();
                if(splitAxis == true)
                {
                    keyboardSensitivity.XAxisSensitivity.SensitivitySlider.value = keyboardSensitivity.OverallSensitivity.SensitivitySlider.value;
                    keyboardSensitivity.YAxisSensitivity.SensitivitySlider.value = keyboardSensitivity.OverallSensitivity.SensitivitySlider.value;
                }
                else
                {
                    keyboardSensitivity.OverallSensitivity.SensitivitySlider.value = keyboardSensitivity.XAxisSensitivity.SensitivitySlider.value;
                }

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }

        public void OnKeyboardOverallSensitivityChanged(float sliderValue)
        {
            if (inSetupMode == false)
            {
                // Setup settings
                settings.KeyboardXAxisSensitivity = sliderValue;
                settings.KeyboardYAxisSensitivity = sliderValue;
                keyboardSensitivity.OverallSensitivity.SensitivityPercentLabel.text = Percent(sliderValue);
            }
        }

        public void OnKeyboardXAxisSensitivityChanged(float sliderValue)
        {
            if (inSetupMode == false)
            {
                // Setup settings
                settings.KeyboardXAxisSensitivity = sliderValue;

                keyboardSensitivity.XAxisSensitivity.SensitivityPercentLabel.text = Percent(sliderValue);
            }
        }

        public void OnKeyboardYAxisSensitivityChanged(float sliderValue)
        {
            if (inSetupMode == false)
            {
                // Setup settings
                settings.KeyboardYAxisSensitivity = sliderValue;

                keyboardSensitivity.YAxisSensitivity.SensitivityPercentLabel.text = Percent(sliderValue);
            }
        }
        #endregion

        #region Keyboard Inverted
        public void OnInvertKeyboardXAxisToggled(bool invert)
        {
            if (inSetupMode == false)
            {
                // Store this settings
                settings.IsKeyboardXAxisInverted = invert;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }

        public void OnInvertKeyboardYAxisToggled(bool invert)
        {
            if (inSetupMode == false)
            {
                // Store this settings
                settings.IsKeyboardYAxisInverted = invert;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }
        #endregion

        #region Mouse Sensitivity
        public void OnSplitMouseAxisToggled(bool splitAxis)
        {
            if (inSetupMode == false)
            {
                // Store this settings
                settings.SplitMouseAxis = splitAxis;

                // Toggle which sliders will be showing up
                mouseSensitivity.UpdateAxisSensitivityControls();
                if(splitAxis == true)
                {
                    mouseSensitivity.XAxisSensitivity.SensitivitySlider.value = mouseSensitivity.OverallSensitivity.SensitivitySlider.value;
                    mouseSensitivity.YAxisSensitivity.SensitivitySlider.value = mouseSensitivity.OverallSensitivity.SensitivitySlider.value;
                }
                else
                {
                    mouseSensitivity.OverallSensitivity.SensitivitySlider.value = mouseSensitivity.XAxisSensitivity.SensitivitySlider.value;
                }

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }

        public void OnMouseOverallSensitivityChanged(float sliderValue)
        {
            if (inSetupMode == false)
            {
                // Setup settings
                settings.MouseXAxisSensitivity = sliderValue;
                settings.MouseYAxisSensitivity = sliderValue;

                mouseSensitivity.OverallSensitivity.SensitivityPercentLabel.text = Percent(sliderValue);
            }
        }

        public void OnMouseXAxisSensitivityChanged(float sliderValue)
        {
            if (inSetupMode == false)
            {
                // Setup settings
                settings.MouseXAxisSensitivity = sliderValue;

                mouseSensitivity.XAxisSensitivity.SensitivityPercentLabel.text = Percent(sliderValue);
            }
        }

        public void OnMouseYAxisSensitivityChanged(float sliderValue)
        {
            if (inSetupMode == false)
            {
                // Setup settings
                settings.MouseYAxisSensitivity = sliderValue;

                mouseSensitivity.YAxisSensitivity.SensitivityPercentLabel.text = Percent(sliderValue);
            }
        }
        #endregion

        #region Mouse Inverted
        public void OnInvertMouseXAxisToggled(bool invert)
        {
            if (inSetupMode == false)
            {
                // Store this settings
                settings.IsMouseXAxisInverted = invert;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }

        public void OnInvertMouseYAxisToggled(bool invert)
        {
            if (inSetupMode == false)
            {
                // Store this settings
                settings.IsMouseYAxisInverted = invert;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }
        #endregion

        #region Scroll Wheel
        public void OnScrollWheelSensitivityChanged(float sliderValue)
        {
            if (inSetupMode == false)
            {
                // Setup settings
                settings.ScrollWheelSensitivity = sliderValue;

                // Update label
                scrollWheelSensitivity.SensitivityPercentLabel.text = Percent(sliderValue);
            }
        }

        public void OnInvertScrollWheelToggled(bool invert)
        {
            if (inSetupMode == false)
            {
                // Store this settings
                settings.IsScrollWheelInverted = invert;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }
        #endregion

        public void OnResetSavedData()
        {
            ConfirmationMenu menu = Manager.GetMenu<ConfirmationMenu>();
            if(menu != null)
            {
                // Display confirmation dialog
                menu.DefaultToYes = false;
                menu.Show(CheckResetSavedDataConfirmation);

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }
        #endregion

        #region Helper Methods
        static string Percent(float val)
        {
            return val.ToString("0%");
        }

        void CheckResetSavedDataConfirmation(IMenu menu)
        {
            if(((ConfirmationMenu)menu).IsYesSelected == true)
            {
                // Clear settings
                settings.ClearSettings();

                // Update the level select menu, if one is available
                LevelSelectMenu levelSelect = Manager.GetMenu<LevelSelectMenu>();
                if(levelSelect != null)
                {
                    levelSelect.SetButtonsEnabled(true);
                }
            }
        }

        void SetupLanguageControls()
        {
            if(languageDropDown.IsSetup == false)
            {
                // Setup the drop down
                languageDropDown.Setup();
            }

            // Update whether the controls are visible or not
            foreach(GameObject controls in languageParents)
            {
                controls.SetActive(enableLanguageControls);
            }
        }

        void SetupAudioControls()
        {
            // Update music controls
            musicControls.Update(BackgroundMusic.GlobalVolume, BackgroundMusic.GlobalMute);
            musicControls.IsActive = enableMusicControls;
            
            // Update sound effect controls
            soundEffectsControls.Update(SoundEffect.GlobalVolume, SoundEffect.GlobalMute);
            soundEffectsControls.IsActive = enableSoundEffectControls;
            
            // Update divider
            if((enableMusicControls == true) || (enableSoundEffectControls == true))
            {
                audioDividers.SetActive(true);
            }
            else
            {
                audioDividers.SetActive(false);
            }
        }

        void SetupSensitivityControls()
        {
            // FIXME: Update keyboard sensitivity
            keyboardSensitivity.Update(false, 1f, 0.5f);
            keyboardSensitivity.IsActive = enableKeyboardSensitivityControls;

            // FIXME: Update mouse sensitivity
            mouseSensitivity.Update(true, 0.1f, 0.01f);
            mouseSensitivity.IsActive = enableMouseSensitivityControls;

            // FIXME: Update scroll wheel sensitivity
            scrollWheelSensitivity.Update(0.75f);
            scrollWheelSensitivity.IsActive = enableScrollWheelSensitivityControls;

            // Update label and dividers
            if((enableKeyboardSensitivityControls == true) || (enableMouseSensitivityControls == true) || (enableScrollWheelSensitivityControls == true))
            {
                foreach(GameObject control in mouseSensitivityLabelsAndDividers)
                {
                    control.SetActive(true);
                }
            }
            else
            {
                foreach(GameObject control in mouseSensitivityLabelsAndDividers)
                {
                    control.SetActive(false);
                }
            }
        }

        void SetupInvertControls()
        {
            // FIXME: Update keyboard inverting controls
            keyboardXInvert.IsInverted = false;
            keyboardXInvert.IsActive = enableKeyboardInvertedControls;
            keyboardYInvert.IsInverted = true;
            keyboardYInvert.IsActive = enableKeyboardInvertedControls;
            
            // FIXME: Update mouse inverting controls
            mouseXInvert.IsInverted = true;
            mouseXInvert.IsActive = enableMouseInvertedControls;
            mouseYInvert.IsInverted = false;
            mouseYInvert.IsActive = enableMouseInvertedControls;

            // FIXME: Update scroll wheel inverting controls
            scrollWheelInvert.IsInverted = true;
            scrollWheelInvert.IsActive = enableScrollWheelInvertedControls;

            // Update label and dividers
            if((enableKeyboardInvertedControls == true) || (enableMouseInvertedControls == true) || (enableScrollWheelInvertedControls == true))
            {
                foreach(GameObject control in mouseInvertLabelsAndDividers)
                {
                    control.SetActive(true);
                }
            }
            else
            {
                foreach(GameObject control in mouseInvertLabelsAndDividers)
                {
                    control.SetActive(false);
                }
            }
        }

        void SetupOtherControls()
        {
            resetAllDataParent.SetActive(enableResetDataButton);
        }
        #endregion
    }
}
