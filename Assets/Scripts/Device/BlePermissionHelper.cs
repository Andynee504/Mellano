using UnityEngine;
using UnityEngine.Android;

public static class BlePermissionHelper
{
    private const string BluetoothScan = "android.permission.BLUETOOTH_SCAN";
    private const string BluetoothConnect = "android.permission.BLUETOOTH_CONNECT";
    private const string FineLocation = "android.permission.ACCESS_FINE_LOCATION";

    public static void RequestAll()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        RequestIfNeeded(BluetoothScan);
        RequestIfNeeded(BluetoothConnect);
        RequestIfNeeded(FineLocation);
#endif
    }

    private static void RequestIfNeeded(string permission)
    {
        if (!Permission.HasUserAuthorizedPermission(permission))
            Permission.RequestUserPermission(permission);
    }
}