using UnityEngine;

public class BleBridge
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject plugin;

    public BleBridge()
    {
        using var pluginClass = new AndroidJavaClass("com.mellano.ble.MellanoBlePlugin");
        plugin = pluginClass.CallStatic<AndroidJavaObject>("getInstance");
    }

    public bool IsBluetoothReady() => plugin.Call<bool>("isBluetoothReady");
    public void StartScan() => plugin.Call("startScan");
    public void StopScan() => plugin.Call("stopScan");
    public void ConnectToAddress(string address) => plugin.Call("connectToAddress", address);
    public void WriteCommand(string command) => plugin.Call("writeCommand", command);
    public void Disconnect() => plugin.Call("disconnect");
#else
    public bool IsBluetoothReady() => true;
    public void StartScan() => Debug.Log("[BleBridge] StartScan");
    public void StopScan() => Debug.Log("[BleBridge] StopScan");
    public void ConnectToAddress(string address) => Debug.Log("[BleBridge] ConnectToAddress " + address);
    public void WriteCommand(string command) => Debug.Log("[BleBridge] WriteCommand " + command);
    public void Disconnect() => Debug.Log("[BleBridge] Disconnect");
#endif
}