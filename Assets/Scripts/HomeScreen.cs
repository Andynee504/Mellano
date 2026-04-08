using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : UIScreen
{
    [Header("Services")]
    [SerializeField] private DeviceConfigService deviceConfigService;

    [Header("Status UI")]
    [SerializeField] private TMP_Text connectionStatusText;
    [SerializeField] private TMP_Text linkDeviceButtonText;

    [Header("Buttons gated by connection")]
    [SerializeField] private Button calibrateButton;
    [SerializeField] private Button inputTestButton;

    [Header("Optional")]
    [SerializeField] private Button linkDeviceButton;

    private void OnEnable()
    {
        if (deviceConfigService != null)
        {
            deviceConfigService.ConnectionChanged += HandleConnectionChanged;
            deviceConfigService.ConfigChanged += HandleConfigChanged;
            deviceConfigService.DeviceFound += HandleDeviceFound;
            deviceConfigService.StatusChanged += HandleStatusChanged;
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (deviceConfigService != null)
        {
            deviceConfigService.ConnectionChanged -= HandleConnectionChanged;
            deviceConfigService.ConfigChanged -= HandleConfigChanged;
            deviceConfigService.DeviceFound -= HandleDeviceFound;
            deviceConfigService.StatusChanged -= HandleStatusChanged;
        }
    }

    protected override void OnEnter()
    {
        RefreshUI();
    }

    public void OnPressLinkDevice()
    {
        Debug.Log("[HomeScreen] OnPressLinkDevice chamado");

        if (deviceConfigService == null)
        {
            Debug.Log("[HomeScreen] deviceConfigService == null");
            return;
        }

        if (deviceConfigService.IsConnected)
        {
            Debug.Log("[HomeScreen] Já conectado, desconectando");
            deviceConfigService.Disconnect();
            return;
        }
        
        if (deviceConfigService.IsScanning)
        {
            Debug.Log("[HomeScreen] Scan já em andamento");
            return;
        }

        Debug.Log("[HomeScreen] Pedindo permissões");

        deviceConfigService.RequestPermissions(
            onGranted: () =>
            {
                Debug.Log("[HomeScreen] Permissões concedidas, iniciando scan");
                deviceConfigService.StartScan();
            },
            onDenied: (permission, canAskAgain) =>
            {
                Debug.LogWarning(canAskAgain ? $"[HomeScreen] Permissão negada: {permission}" : $"[HomeScreen] Permissão negada permanentemente: {permission}");
            }
        );
    }

    private void HandleDeviceFound(string deviceName, string address)
    {
        Debug.Log($"[HomeScreen] Dispositivo encontrado: {deviceName} | {address}");
        if (deviceConfigService == null)
            return;

        deviceConfigService.StopScan();
        deviceConfigService.ConnectToAddress(address);
    }

    private void HandleStatusChanged(string status)
    {
        if (connectionStatusText != null)
            connectionStatusText.text = status;
    }

    private void HandleConnectionChanged(bool connected)
    {
        RefreshUI();
    }

    private void HandleConfigChanged()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (deviceConfigService == null)
            return;

        bool connected = deviceConfigService.IsConnected;

        if (connectionStatusText != null)
            connectionStatusText.text = deviceConfigService.ConnectionStatusLabel;

        if (linkDeviceButtonText != null)
            linkDeviceButtonText.text = connected ? "Desconectar dispositivo" : "Conectar dispositivo";

        if (calibrateButton != null)
            calibrateButton.interactable = connected;

        if (inputTestButton != null)
            inputTestButton.interactable = connected;
    }
}