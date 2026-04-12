using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public class DeviceProfileSnapshot
{
    public float sensX;
    public float sensY;
    public float deadzone;
    public bool invertX;
    public bool invertY;
}

public class DeviceConfigService : MonoBehaviour
{
    public bool IsConnected { get; private set; }
    public bool IsScanning { get; private set; }
    public string LastDeviceAddress { get; private set; }
    public string LastStatus { get; private set; } = "Idle";

    public float SensX { get; private set; } = 18f;
    public float SensY { get; private set; } = 18f;
    public float Deadzone { get; private set; } = 0.80f;
    public bool InvertX { get; private set; } = true;
    public bool InvertY { get; private set; } = false;

    public string ConnectedDeviceName { get; private set; } = "N/A";
    public string FirmwareVersion { get; private set; } = "Desconhecida";

    public float TelemetryAccelX { get; private set; }
    public float TelemetryAccelY { get; private set; }
    public bool TelemetryButton1 { get; private set; }
    public bool TelemetryButton2 { get; private set; }
    public bool TelemetryButton3 { get; private set; }

    public event Action<bool> ConnectionChanged;
    public event Action ConfigChanged;
    public event Action<string> StatusChanged;
    public event Action<string, string> DeviceFound;
    public event Action TelemetryChanged;
    public event Action DeviceInfoChanged;

    private BleBridge bleBridge;
    private bool isConnecting;

    private void Awake()
    {
        bleBridge = new BleBridge();
    }

    public string ConnectionStatusLabel =>
        IsConnected ? "Dispositivo conectado" : "Dispositivo não conectado";

    public void RequestPermissions(System.Action onGranted, System.Action<string, bool> onDenied = null)
    {
        Debug.Log("[DeviceConfigService] RequestPermissions");
        BlePermissionHelper.RequestBlePermissions(onGranted: onGranted, onDenied: onDenied);
    }

    public void StartScan()
    {
        Debug.Log("[DeviceConfigService] StartScan");
        isConnecting = false;
        IsScanning = true;
        LastStatus = "Escaneando...";
        StatusChanged?.Invoke(LastStatus);
        bleBridge.StartScan();
    }

    public void StopScan()
    {
        Debug.Log("[DeviceConfigService] StopScan");
        IsScanning = false;
        bleBridge.StopScan();
        LastStatus = "Scan parado";
        StatusChanged?.Invoke(LastStatus);
    }

    public void ConnectToAddress(string address)
    {
        Debug.Log("[DeviceConfigService] ConnectToAddress -> " + address);
        LastDeviceAddress = address;
        bleBridge.ConnectToAddress(address);
    }

    public void Disconnect()
    {
        bleBridge.Disconnect();
    }

    public void SetSensX(float value)
    {
        SensX = value;
        SendCommand($"SX={FormatFloat(value)}");
        ConfigChanged?.Invoke();
    }

    public void SetSensY(float value)
    {
        SensY = value;
        SendCommand($"SY={FormatFloat(value)}");
        ConfigChanged?.Invoke();
    }

    public void SetDeadzone(float value)
    {
        Deadzone = value;
        SendCommand($"DZ={FormatFloat(value)}");
        ConfigChanged?.Invoke();
    }

    public void SetInvertX(bool value)
    {
        InvertX = value;
        SendCommand($"IX={(value ? 1 : 0)}");
        ConfigChanged?.Invoke();
    }

    public void SetInvertY(bool value)
    {
        InvertY = value;
        SendCommand($"IY={(value ? 1 : 0)}");
        ConfigChanged?.Invoke();
    }

    public void CalibrateCenter() => SendCommand("CAL");
    public void Save() => SendCommand("SAVE");
    public void RequestDeviceInfo() => SendCommand("INFO");
    public void StartInputStream() => SendCommand("STREAM=1");
    public void StopInputStream() => SendCommand("STREAM=0");
    public void FactoryReset() => SendCommand("FACTORY_RESET");
    public void RebootToGameMode() => SendCommand("REBOOT");

    public void SaveAndReboot()
    {
        StartCoroutine(SaveAndRebootRoutine());
    }

    private System.Collections.IEnumerator SaveAndRebootRoutine()
    {
        SendCommand("SAVE");
        yield return new WaitForSeconds(1f);
        SendCommand("REBOOT");
    }

    public void SaveProfile(int slot)
    {
        if (slot < 1 || slot > 3)
            return;

        DeviceProfileSnapshot snapshot = CreateProfileSnapshot();
        string json = JsonUtility.ToJson(snapshot);

        PlayerPrefs.SetString(GetProfileKey(slot), json);
        PlayerPrefs.Save();

        LastStatus = $"Perfil {slot} salvo localmente";
        StatusChanged?.Invoke(LastStatus);
    }

    public bool LoadProfile(int slot)
    {
        if (!TryGetProfile(slot, out DeviceProfileSnapshot snapshot))
        {
            LastStatus = $"Perfil {slot} vazio";
            StatusChanged?.Invoke(LastStatus);
            return false;
        }

        ApplyProfileSnapshot(snapshot);
        LastStatus = $"Perfil {slot} carregado";
        StatusChanged?.Invoke(LastStatus);
        return true;
    }

    public bool HasSavedProfile(int slot)
    {
        return PlayerPrefs.HasKey(GetProfileKey(slot));
    }

    public void SendCommand(string command)
    {
        bleBridge.WriteCommand(command);
        LastStatus = "CMD: " + command;
        StatusChanged?.Invoke(LastStatus);
    }

    public void OnBleDeviceFound(string payload)
    {
        Debug.Log("[DeviceConfigService] OnBleDeviceFound -> " + payload);

        if (isConnecting)
            return;

        string[] parts = payload.Split('|');
        if (parts.Length < 2)
            return;

        isConnecting = true;
        IsScanning = false;

        ConnectedDeviceName = string.IsNullOrWhiteSpace(parts[0]) ? "N/A" : parts[0];
        LastDeviceAddress = parts[1];

        DeviceFound?.Invoke(parts[0], parts[1]);
        DeviceInfoChanged?.Invoke();

        LastStatus = "Encontrado: " + parts[0];
        StatusChanged?.Invoke(LastStatus);
    }

    public void OnBleStatus(string status)
    {
        Debug.Log("[DeviceConfigService] OnBleStatus -> " + status);
        LastStatus = status;
        StatusChanged?.Invoke(LastStatus);
    }

    public void OnBleConnectionChanged(string state)
    {
        Debug.Log("[DeviceConfigService] OnBleConnectionChanged -> " + state);

        IsConnected = state == "CONNECTED";

        if (state == "CONNECTED" || state == "DISCONNECTED")
        {
            isConnecting = false;
            IsScanning = false;
        }

        if (!IsConnected)
        {
            TelemetryAccelX = 0f;
            TelemetryAccelY = 0f;
            TelemetryButton1 = false;
            TelemetryButton2 = false;
            TelemetryButton3 = false;
            FirmwareVersion = "Desconhecida";
            TelemetryChanged?.Invoke();
            DeviceInfoChanged?.Invoke();
        }

        ConnectionChanged?.Invoke(IsConnected);
        LastStatus = state;
        StatusChanged?.Invoke(LastStatus);
    }

    public void OnBleNotify(string payload)
    {
        Debug.Log("[DeviceConfigService] OnBleNotify -> " + payload);

        if (string.IsNullOrWhiteSpace(payload))
            return;

        if (payload.StartsWith("INFO:", StringComparison.OrdinalIgnoreCase))
        {
            ParseInfoPayload(payload.Substring(5));
            return;
        }

        if (payload.StartsWith("INPUT:", StringComparison.OrdinalIgnoreCase))
        {
            ParseInputPayload(payload.Substring(6));
            return;
        }

        LastStatus = payload;
        StatusChanged?.Invoke(LastStatus);
    }

    private DeviceProfileSnapshot CreateProfileSnapshot()
    {
        return new DeviceProfileSnapshot
        {
            sensX = SensX,
            sensY = SensY,
            deadzone = Deadzone,
            invertX = InvertX,
            invertY = InvertY
        };
    }

    private void ApplyProfileSnapshot(DeviceProfileSnapshot snapshot)
    {
        if (snapshot == null)
            return;

        SetSensX(snapshot.sensX);
        SetSensY(snapshot.sensY);
        SetDeadzone(snapshot.deadzone);
        SetInvertX(snapshot.invertX);
        SetInvertY(snapshot.invertY);
        ConfigChanged?.Invoke();
    }

    private bool TryGetProfile(int slot, out DeviceProfileSnapshot snapshot)
    {
        snapshot = null;

        if (!HasSavedProfile(slot))
            return false;

        string json = PlayerPrefs.GetString(GetProfileKey(slot), string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return false;

        snapshot = JsonUtility.FromJson<DeviceProfileSnapshot>(json);
        return snapshot != null;
    }

    private void ParseInfoPayload(string rawPayload)
    {
        string[] parts = rawPayload.Split('|');

        if (parts.Length >= 1 && !string.IsNullOrWhiteSpace(parts[0]))
            ConnectedDeviceName = parts[0];

        if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
            FirmwareVersion = parts[1];

        DeviceInfoChanged?.Invoke();
    }

    private void ParseInputPayload(string rawPayload)
    {
        string[] parts = rawPayload.Split('|');
        if (parts.Length < 5)
            return;

        if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float accelX))
            TelemetryAccelX = accelX;

        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float accelY))
            TelemetryAccelY = accelY;

        TelemetryButton1 = parts[2] == "1";
        TelemetryButton2 = parts[3] == "1";
        TelemetryButton3 = parts[4] == "1";

        TelemetryChanged?.Invoke();
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }

    private static string GetProfileKey(int slot)
    {
        return $"mellano_profile_{slot}";
    }
}
