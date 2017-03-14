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

            public void Setup(float volume, bool mute)
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
        public struct MouseSensitivityControls
        {
            [SerializeField]
            GameObject controlParent;
            [SerializeField]
            Slider slider;
            [SerializeField]
            Text percentLabel;

            public void Setup(float sensitivity)
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

        [Header("Features to Enable")]
        [SerializeField]
        bool enableMusicControls = true;
        [SerializeField]
        bool enableSoundEffectControls = true;
        [SerializeField]
        bool enableMouseSensitivityControls = true;
        [SerializeField]
        bool enableScrollWheelSensitivityControls = true;
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
        Toggle mouseSensitivityAdvancedToggle;
        [SerializeField]
        MouseSensitivityControls bothMouseAxisSensitivity;
        [SerializeField]
        MouseSensitivityControls mouseXAxisSensitivity;
        [SerializeField]
        MouseSensitivityControls mouseYAxisSensitivity;
        [SerializeField]
        MouseSensitivityControls scrollWheelSensitivity;
        [SerializeField]
        GameObject[] mouseSensitivityLabelsAndDividers;

        SoundEffect audioCache;
        bool inSetupMode = false;

        System.Action<OptionsMenu> hideAction = null;

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

        void Start()
        {
            // Setup controls
            inSetupMode = true;
            musicControls.Setup(BackgroundMusic.GlobalVolume, BackgroundMusic.GlobalMute);
            soundEffectsControls.Setup(SoundEffect.GlobalVolume, SoundEffect.GlobalMute);
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
    }
}
