using UnityEngine;
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

                overallSensitivity.Update(xSensitivity);
                xAxisSensitivity.Update(xSensitivity);
                yAxisSensitivity.Update(ySensitivity);
                
                UpdateAxisSensitivityControls();
            }

            public bool IsAxisSplit
            {
                get
                {
                    return splitAxisToggle.IsInverted;
                }
                set
                {
                    splitAxisToggle.IsInverted = value;
                    UpdateAxisSensitivityControls();
                }
            }

            public float XAxisSensitivity
            {
                get
                {
                    if(splitAxisToggle.IsInverted == true)
                    {
                        return overallSensitivity.SensitivitySlider.value;
                    }
                    else
                    {
                        return xAxisSensitivity.SensitivitySlider.value;
                    }
                }
                set
                {
                    overallSensitivity.Update(value);
                    xAxisSensitivity.Update(value);
                }
            }

            public float YAxisSensitivity
            {
                get
                {
                    if(splitAxisToggle.IsInverted == true)
                    {
                        return overallSensitivity.SensitivitySlider.value;
                    }
                    else
                    {
                        return yAxisSensitivity.SensitivitySlider.value;
                    }
                }
                set
                {
                    yAxisSensitivity.Update(value);
                }
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
            
            void UpdateAxisSensitivityControls()
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
        [Header("Features to Enable")]
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

        [Header("Audio Controls")]
        [SerializeField]
        AudioControls musicControls;
        [SerializeField]
        AudioControls soundEffectsControls;
        [SerializeField]
        GameObject audioDividers;

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

            // Update how music controls are enabled
            SetupAudioControls();

            //SetupMouseSensitivityControls();

            //SetupInvertControls();

            // Update whether the rest of the controls are enabled
            resetAllDataParent.SetActive(enableResetDataButton);
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

        public void OnMusicSliderChanged()
        {
            if (inSetupMode == false)
            {
                BackgroundMusic.GlobalVolume = musicControls.VolumeSlider.value;
                musicControls.VolumePercentLabel.text = Percent(BackgroundMusic.GlobalVolume);
            }
        }

        public void OnSoundEffectsSliderChanged()
        {
            if (inSetupMode == false)
            {
                SoundEffect.GlobalVolume = soundEffectsControls.VolumeSlider.value;
                soundEffectsControls.VolumePercentLabel.text = Percent(SoundEffect.GlobalVolume);
            }
        }
        
        public void OnSoundEffectsSliderPointerUp()
        {
            TestSoundEffect.Play();
        }

        public void OnMusicMuteClicked()
        {
            if (inSetupMode == false)
            {
                // Toggle mute
                BackgroundMusic.GlobalMute = !BackgroundMusic.GlobalMute;

                // Change the check box
                musicControls.CheckBoxMark.isOn = BackgroundMusic.GlobalMute;

                // disable the slider
                musicControls.VolumeSlider.interactable = !BackgroundMusic.GlobalMute;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }

        public void OnSoundEffectsMuteClicked()
        {
            if (inSetupMode == false)
            {
                // Toggle mute
                SoundEffect.GlobalMute = !SoundEffect.GlobalMute;

                // Change the check box
                soundEffectsControls.CheckBoxMark.isOn = SoundEffect.GlobalMute;

                // disable the slider
                soundEffectsControls.VolumeSlider.interactable = !SoundEffect.GlobalMute;

                // Indicate button is clicked
                Manager.ButtonClick.Play();
            }
        }

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

        void SetupAudioControls()
        {
            musicControls.Update(BackgroundMusic.GlobalVolume, BackgroundMusic.GlobalMute);
            musicControls.IsActive = enableMusicControls;
            
            soundEffectsControls.Update(SoundEffect.GlobalVolume, SoundEffect.GlobalMute);
            soundEffectsControls.IsActive = enableSoundEffectControls;
            
            if((enableMusicControls == true) || (enableSoundEffectControls == true))
            {
                audioDividers.SetActive(true);
            }
            else
            {
                audioDividers.SetActive(false);
            }
        }
    }
}
