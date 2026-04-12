using TMPro;
using UnityEngine;

public class InputTestScreen : UIScreen
{
    [Header("Services")]
    [SerializeField] private DeviceConfigService deviceConfigService;

    [Header("Telemetry UI")]
    [SerializeField] private TMP_Text axisValueXText;
    [SerializeField] private TMP_Text axisValueYText;
    [SerializeField] private TMP_Text pedalValue1Text;
    [SerializeField] private TMP_Text pedalValue2Text;
    [SerializeField] private TMP_Text pedalValue3Text;

    private void OnEnable()
    {
        if (deviceConfigService != null)
        {
            deviceConfigService.ConnectionChanged += HandleConnectionChanged;
            deviceConfigService.TelemetryChanged += HandleTelemetryChanged;
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (deviceConfigService != null)
        {
            deviceConfigService.ConnectionChanged -= HandleConnectionChanged;
            deviceConfigService.TelemetryChanged -= HandleTelemetryChanged;
            deviceConfigService.StopInputStream();
        }
    }

    protected override void OnEnter()
    {
        RefreshUI();

        if (deviceConfigService != null && deviceConfigService.IsConnected)
            deviceConfigService.StartInputStream();
    }

    protected override void OnExit()
    {
        if (deviceConfigService != null && deviceConfigService.IsConnected)
            deviceConfigService.StopInputStream();
    }

    private void HandleConnectionChanged(bool connected)
    {
        if (connected)
            deviceConfigService?.StartInputStream();
        else
            SetDisconnectedUI();
    }

    private void HandleTelemetryChanged()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (deviceConfigService == null || !deviceConfigService.IsConnected)
        {
            SetDisconnectedUI();
            return;
        }

        if (axisValueXText != null)
            axisValueXText.text = $"Accel X: {deviceConfigService.TelemetryAccelX:F2}";

        if (axisValueYText != null)
            axisValueYText.text = $"Accel Y: {deviceConfigService.TelemetryAccelY:F2}";

        if (pedalValue1Text != null)
            pedalValue1Text.text = $"Botão 1: {(deviceConfigService.TelemetryButton1 ? "Pressionado" : "Solto")}";

        if (pedalValue2Text != null)
            pedalValue2Text.text = $"Botão 2: {(deviceConfigService.TelemetryButton2 ? "Pressionado" : "Solto")}";

        if (pedalValue3Text != null)
            pedalValue3Text.text = $"Botão 3: {(deviceConfigService.TelemetryButton3 ? "Pressionado" : "Solto")}";
    }

    private void SetDisconnectedUI()
    {
        if (axisValueXText != null)
            axisValueXText.text = "Accel X: --";

        if (axisValueYText != null)
            axisValueYText.text = "Accel Y: --";

        if (pedalValue1Text != null)
            pedalValue1Text.text = "Botão 1: --";

        if (pedalValue2Text != null)
            pedalValue2Text.text = "Botão 2: --";

        if (pedalValue3Text != null)
            pedalValue3Text.text = "Botão 3: --";
    }
}
