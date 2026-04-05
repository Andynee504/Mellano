using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationScreen : UIScreen
{
    [Header("Services")]
    [SerializeField] private DeviceConfigService deviceConfigService;

    [Header("Status")]
    [SerializeField] private TMP_Text connectionStatusText;

    [Header("Sliders")]
    [SerializeField] private Slider sensXSlider;
    [SerializeField] private TMP_Text sensXValueText;

    [SerializeField] private Slider sensYSlider;
    [SerializeField] private TMP_Text sensYValueText;

    [SerializeField] private Slider deadzoneSlider;
    [SerializeField] private TMP_Text deadzoneValueText;

    [Header("Toggles")]
    [SerializeField] private Toggle invertXToggle;
    [SerializeField] private Toggle invertYToggle;

    [Header("Buttons")]
    [SerializeField] private Button calibrateCenterButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button saveAndRebootButton;

    private bool isRefreshingUI;

    private void Awake()
    {
        if (sensXSlider != null)
            sensXSlider.onValueChanged.AddListener(OnSensXChanged);

        if (sensYSlider != null)
            sensYSlider.onValueChanged.AddListener(OnSensYChanged);

        if (deadzoneSlider != null)
            deadzoneSlider.onValueChanged.AddListener(OnDeadzoneChanged);

        if (invertXToggle != null)
            invertXToggle.onValueChanged.AddListener(OnInvertXChanged);

        if (invertYToggle != null)
            invertYToggle.onValueChanged.AddListener(OnInvertYChanged);

        if (calibrateCenterButton != null)
            calibrateCenterButton.onClick.AddListener(OnPressCalibrateCenter);

        if (saveButton != null)
            saveButton.onClick.AddListener(OnPressSave);

        if (saveAndRebootButton != null)
            saveAndRebootButton.onClick.AddListener(OnPressSaveAndReboot);
    }

    private void OnEnable()
    {
        if (deviceConfigService != null)
        {
            deviceConfigService.ConnectionChanged += HandleConnectionChanged;
            deviceConfigService.ConfigChanged += HandleConfigChanged;
        }

        RefreshFromService();
    }

    private void OnDisable()
    {
        if (deviceConfigService != null)
        {
            deviceConfigService.ConnectionChanged -= HandleConnectionChanged;
            deviceConfigService.ConfigChanged -= HandleConfigChanged;
        }
    }

    private void OnDestroy()
    {
        if (sensXSlider != null)
            sensXSlider.onValueChanged.RemoveListener(OnSensXChanged);

        if (sensYSlider != null)
            sensYSlider.onValueChanged.RemoveListener(OnSensYChanged);

        if (deadzoneSlider != null)
            deadzoneSlider.onValueChanged.RemoveListener(OnDeadzoneChanged);

        if (invertXToggle != null)
            invertXToggle.onValueChanged.RemoveListener(OnInvertXChanged);

        if (invertYToggle != null)
            invertYToggle.onValueChanged.RemoveListener(OnInvertYChanged);

        if (calibrateCenterButton != null)
            calibrateCenterButton.onClick.RemoveListener(OnPressCalibrateCenter);

        if (saveButton != null)
            saveButton.onClick.RemoveListener(OnPressSave);

        if (saveAndRebootButton != null)
            saveAndRebootButton.onClick.RemoveListener(OnPressSaveAndReboot);
    }

    protected override void OnEnter()
    {
        RefreshFromService();
    }

    private void HandleConnectionChanged(bool connected)
    {
        RefreshConnectionStatus(connected);
    }

    private void HandleConfigChanged()
    {
        RefreshFromService();
    }

    private void RefreshFromService()
    {
        if (deviceConfigService == null)
            return;

        isRefreshingUI = true;

        RefreshConnectionStatus(deviceConfigService.IsConnected);

        if (sensXSlider != null)
            sensXSlider.SetValueWithoutNotify(deviceConfigService.SensX);

        if (sensYSlider != null)
            sensYSlider.SetValueWithoutNotify(deviceConfigService.SensY);

        if (deadzoneSlider != null)
            deadzoneSlider.SetValueWithoutNotify(deviceConfigService.Deadzone);

        if (invertXToggle != null)
            invertXToggle.SetIsOnWithoutNotify(deviceConfigService.InvertX);

        if (invertYToggle != null)
            invertYToggle.SetIsOnWithoutNotify(deviceConfigService.InvertY);

        UpdateSensXLabel();
        UpdateSensYLabel();
        UpdateDeadzoneLabel();

        isRefreshingUI = false;
    }

    private void RefreshConnectionStatus(bool connected)
    {
        if (connectionStatusText != null)
            connectionStatusText.text = connected ? "Dispositivo conectado" : "Dispositivo não conectado";
    }

    private void OnSensXChanged(float value)
    {
        UpdateSensXLabel();

        if (isRefreshingUI || deviceConfigService == null)
            return;

        deviceConfigService.SetSensX(value);
    }

    private void OnSensYChanged(float value)
    {
        UpdateSensYLabel();

        if (isRefreshingUI || deviceConfigService == null)
            return;

        deviceConfigService.SetSensY(value);
    }

    private void OnDeadzoneChanged(float value)
    {
        UpdateDeadzoneLabel();

        if (isRefreshingUI || deviceConfigService == null)
            return;

        deviceConfigService.SetDeadzone(value);
    }

    private void OnInvertXChanged(bool value)
    {
        if (isRefreshingUI || deviceConfigService == null)
            return;

        deviceConfigService.SetInvertX(value);
    }

    private void OnInvertYChanged(bool value)
    {
        if (isRefreshingUI || deviceConfigService == null)
            return;

        deviceConfigService.SetInvertY(value);
    }

    private void OnPressCalibrateCenter()
    {
        if (deviceConfigService == null)
            return;

        deviceConfigService.CalibrateCenter();
    }

    private void OnPressSave()
    {
        if (deviceConfigService == null)
            return;

        deviceConfigService.Save();
    }

    private void OnPressSaveAndReboot()
    {
        if (deviceConfigService == null)
            return;

        deviceConfigService.SaveAndReboot();
    }

    private void UpdateSensXLabel()
    {
        if (sensXValueText != null && sensXSlider != null)
            sensXValueText.text = sensXSlider.value.ToString("F2");
    }

    private void UpdateSensYLabel()
    {
        if (sensYValueText != null && sensYSlider != null)
            sensYValueText.text = sensYSlider.value.ToString("F2");
    }

    private void UpdateDeadzoneLabel()
    {
        if (deadzoneValueText != null && deadzoneSlider != null)
            deadzoneValueText.text = deadzoneSlider.value.ToString("F2");
    }
}