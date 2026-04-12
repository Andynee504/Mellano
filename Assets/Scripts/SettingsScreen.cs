using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : UIScreen
{
    [Header("Services")]
    [SerializeField] private DeviceConfigService deviceConfigService;

    [Header("Info UI")]
    [SerializeField] private TMP_Text deviceNameText;
    [SerializeField] private TMP_Text firmwareText;
    [SerializeField] private TMP_Text statusText;

    [Header("Buttons")]
    [SerializeField] private Button factoryResetButton;
    [SerializeField] private Button rebootButton;
    [SerializeField] private Button refreshInfoButton;

    private void Awake()
    {
        if (factoryResetButton != null)
            factoryResetButton.onClick.AddListener(OnPressFactoryReset);

        if (rebootButton != null)
            rebootButton.onClick.AddListener(OnPressReboot);

        if (refreshInfoButton != null)
            refreshInfoButton.onClick.AddListener(OnPressRefreshInfo);
    }

    private void OnEnable()
    {
        if (deviceConfigService != null)
        {
            deviceConfigService.ConnectionChanged += HandleConnectionChanged;
            deviceConfigService.StatusChanged += HandleStatusChanged;
            deviceConfigService.DeviceInfoChanged += HandleDeviceInfoChanged;
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (deviceConfigService != null)
        {
            deviceConfigService.ConnectionChanged -= HandleConnectionChanged;
            deviceConfigService.StatusChanged -= HandleStatusChanged;
            deviceConfigService.DeviceInfoChanged -= HandleDeviceInfoChanged;
        }
    }

    protected override void OnEnter()
    {
        RefreshUI();

        if (deviceConfigService != null && deviceConfigService.IsConnected)
            deviceConfigService.RequestDeviceInfo();
    }

    private void HandleConnectionChanged(bool connected)
    {
        RefreshUI();
    }

    private void HandleStatusChanged(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }

    private void HandleDeviceInfoChanged()
    {
        RefreshUI();
    }

    private void OnPressFactoryReset()
    {
        if (deviceConfigService == null || !deviceConfigService.IsConnected)
            return;

        deviceConfigService.FactoryReset();
    }

    private void OnPressReboot()
    {
        if (deviceConfigService == null || !deviceConfigService.IsConnected)
            return;

        deviceConfigService.RebootToGameMode();
    }

    private void OnPressRefreshInfo()
    {
        if (deviceConfigService == null || !deviceConfigService.IsConnected)
            return;

        deviceConfigService.RequestDeviceInfo();
    }

    private void RefreshUI()
    {
        bool connected = deviceConfigService != null && deviceConfigService.IsConnected;

        if (deviceNameText != null)
            deviceNameText.text = connected
                ? $"Dispositivo: {deviceConfigService.ConnectedDeviceName}"
                : "Dispositivo: --";

        if (firmwareText != null)
            firmwareText.text = connected
                ? $"Firmware: {deviceConfigService.FirmwareVersion}"
                : "Firmware: --";

        if (factoryResetButton != null)
            factoryResetButton.interactable = connected;

        if (rebootButton != null)
            rebootButton.interactable = connected;

        if (refreshInfoButton != null)
            refreshInfoButton.interactable = connected;
    }
}
