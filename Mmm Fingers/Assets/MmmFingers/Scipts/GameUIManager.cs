using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioManager audioManager;

    [Header("Panels")]
    [SerializeField] private GameObject tapToPlayPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button musicButton;

    [Header("Sound Toggle Visual")]
    [SerializeField] private RectTransform soundSwitch;
    [SerializeField] private Image soundOnImage;
    [SerializeField] private Image soundOffImage;

    [Header("Music Toggle Visual")]
    [SerializeField] private RectTransform musicSwitch;
    [SerializeField] private Image musicOnImage;
    [SerializeField] private Image musicOffImage;

    [Header("Toggle Animation")]
    [SerializeField] private float switchOffX = 128.6f;
    [SerializeField] private float switchOnX = 237f;
    [SerializeField] private float switchDuration = 0.3f;

    private void Awake()
    {
        SetupButtonListeners();
    }

    private void Start()
    {
        RefreshAudioUI(false);
    }

    // --------------------------------------------------
    // BUTTON LISTENERS
    // --------------------------------------------------

    private void SetupButtonListeners()
    {
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(
                OnSettingsPressed
            );
        }

        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.AddListener(
                OnCloseSettingsPressed
            );
        }

        if (soundButton != null)
        {
            soundButton.onClick.AddListener(
                OnSoundPressed
            );
        }

        if (musicButton != null)
        {
            musicButton.onClick.AddListener(
                OnMusicPressed
            );
        }
    }

    private void RemoveButtonListeners()
    {
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(
                OnSettingsPressed
            );
        }

        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveListener(
                OnCloseSettingsPressed
            );
        }

        if (soundButton != null)
        {
            soundButton.onClick.RemoveListener(
                OnSoundPressed
            );
        }

        if (musicButton != null)
        {
            musicButton.onClick.RemoveListener(
                OnMusicPressed
            );
        }
    }

    // --------------------------------------------------
    // BUTTON ACTIONS
    // --------------------------------------------------

    private void OnSettingsPressed()
    {
        if (gameManager == null)
        {
            return;
        }
        AudioManager.Instance?.PlayButtonSound();

        gameManager.OpenSettings();
    }

    private void OnCloseSettingsPressed()
    {
        if (gameManager == null)
        {
            return;
        }
        AudioManager.Instance?.PlayButtonSound();

        gameManager.CloseSettings();
    }

    private void OnSoundPressed()
    {
        if (audioManager == null)
        {
            return;
        }

        bool isOn =
            audioManager.ToggleSound();

        if (isOn)
        {
            audioManager.PlayButtonSound();
        }

        SetSoundVisual(
            isOn,
            true
        );
    }

    private void OnMusicPressed()
    {
        if (audioManager == null)
        {
            return;
        }

        audioManager.PlayButtonSound();

        bool isOn =
            audioManager.ToggleMusic();

        SetMusicVisual(
            isOn,
            true
        );
    }

    // --------------------------------------------------
    // PANELS
    // --------------------------------------------------

    public void ShowTapToPlay()
    {
        SetPanel(tapToPlayPanel, true);
        SetPanel(gameplayPanel, false);
        SetPanel(settingsPanel, false);
    }

    public void ShowGameplay()
    {
        SetPanel(tapToPlayPanel, false);
        SetPanel(gameplayPanel, true);
        SetPanel(settingsPanel, false);
    }

    public void ShowSettings()
    {
        //SetPanel(tapToPlayPanel, false);
        SetPanel(gameplayPanel, false);
        SetPanel(settingsPanel, true);

        RefreshAudioUI(false);
    }

    private void SetPanel(
        GameObject panel,
        bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    // --------------------------------------------------
    // AUDIO VISUALS
    // --------------------------------------------------

    private void RefreshAudioUI(bool animate)
    {
        if (audioManager == null)
        {
            return;
        }

        SetSoundVisual(
            audioManager.IsSoundOn,
            animate
        );

        SetMusicVisual(
            audioManager.IsMusicOn,
            animate
        );
    }

    private void SetSoundVisual(
        bool isOn,
        bool animate)
    {
        SetToggleVisual(
            soundSwitch,
            soundOnImage,
            soundOffImage,
            isOn,
            animate
        );
    }

    private void SetMusicVisual(
        bool isOn,
        bool animate)
    {
        SetToggleVisual(
            musicSwitch,
            musicOnImage,
            musicOffImage,
            isOn,
            animate
        );
    }

    private void SetToggleVisual(
        RectTransform switchTransform,
        Image onImage,
        Image offImage,
        bool isOn,
        bool animate)
    {
        float targetX =
            isOn ? switchOnX : switchOffX;

        if (switchTransform != null)
        {
            switchTransform.DOKill();

            if (animate)
            {
                switchTransform.DOAnchorPosX(
                    targetX,
                    switchDuration
                );
            }
            else
            {
                Vector2 position =
                    switchTransform.anchoredPosition;

                position.x = targetX;

                switchTransform.anchoredPosition =
                    position;
            }
        }

        if (onImage != null)
        {
            onImage.DOKill();

            if (animate)
            {
                onImage.DOFade(
                    isOn ? 1f : 0f,
                    switchDuration
                );
            }
            else
            {
                Color color = onImage.color;

                color.a = isOn ? 1f : 0f;

                onImage.color = color;
            }
        }

        if (offImage != null)
        {
            offImage.DOKill();

            if (animate)
            {
                offImage.DOFade(
                    isOn ? 0f : 1f,
                    switchDuration
                );
            }
            else
            {
                Color color = offImage.color;

                color.a = isOn ? 0f : 1f;

                offImage.color = color;
            }
        }
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
    }
}