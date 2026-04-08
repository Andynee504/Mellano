using System;
using UnityEngine;
using UnityEngine.Android;

public static class BlePermissionHelper
{
    public static void RequestBlePermissions(Action onGranted, Action<string, bool> onDenied = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string[] permissions =
        {
            "android.permission.BLUETOOTH_SCAN",
            "android.permission.BLUETOOTH_CONNECT"
        };

        RequestNextPermission(permissions, 0, onGranted, onDenied);
#else
        onGranted?.Invoke();
#endif
    }

    private static void RequestNextPermission(string[] permissions, int index, Action onGranted, Action<string, bool> onDenied)
    {
        if (index >= permissions.Length)
        {
            onGranted?.Invoke();
            return;
        }

        string permission = permissions[index];

        if (Permission.HasUserAuthorizedPermission(permission))
        {
            RequestNextPermission(permissions, index + 1, onGranted, onDenied);
            return;
        }

        var callbacks = new PermissionCallbacks();

        callbacks.PermissionGranted += grantedPermission =>
        {
            if (grantedPermission == permission)
                RequestNextPermission(permissions, index + 1, onGranted, onDenied);
        };

        callbacks.PermissionDenied += deniedPermission =>
        {
            if (deniedPermission != permission) return;

            bool canAskAgain = Permission.ShouldShowRequestPermissionRationale(permission);
            onDenied?.Invoke(permission, canAskAgain);
        };

        callbacks.PermissionRequestDismissed += dismissedPermission =>
        {
            if (dismissedPermission != permission) return;

            bool canAskAgain = Permission.ShouldShowRequestPermissionRationale(permission);
            onDenied?.Invoke(permission, canAskAgain);
        };

        Permission.RequestUserPermission(permission, callbacks);
    }
}