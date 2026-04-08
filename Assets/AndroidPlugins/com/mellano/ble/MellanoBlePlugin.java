package com.mellano.ble;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.BluetoothManager;
import android.bluetooth.le.BluetoothLeScanner;
import android.bluetooth.le.ScanCallback;
import android.bluetooth.le.ScanResult;
import android.content.Context;

import com.unity3d.player.UnityPlayer;

import java.nio.charset.StandardCharsets;
import java.util.UUID;

import android.bluetooth.BluetoothProfile;
import android.os.ParcelUuid;
import java.util.List;

public class MellanoBlePlugin {
    private static MellanoBlePlugin instance;

    private final Activity activity;
    private final BluetoothManager bluetoothManager;
    private final BluetoothAdapter bluetoothAdapter;
    private BluetoothLeScanner scanner;
    private BluetoothGatt gatt;
    private BluetoothGattCharacteristic writeCharacteristic;

    private static final String UNITY_RECEIVER = "DeviceConfigService";

    private static final UUID SERVICE_UUID = UUID.fromString("6E400001-B5A3-F393-E0A9-E50E24DCCA9E");
    private static final UUID WRITE_UUID = UUID.fromString("6E400002-B5A3-F393-E0A9-E50E24DCCA9E");
    private static final String TARGET_NAME = "Mellano Config";

    public MellanoBlePlugin(Activity activity) {
        this.activity = activity;
        this.bluetoothManager = (BluetoothManager) activity.getSystemService(Context.BLUETOOTH_SERVICE);
        this.bluetoothAdapter = bluetoothManager.getAdapter();
    }

    public static MellanoBlePlugin getInstance() {
        if (instance == null) {
            instance = new MellanoBlePlugin(UnityPlayer.currentActivity);
        }
        return instance;
    }

    public boolean isBluetoothReady() {
        return bluetoothAdapter != null && bluetoothAdapter.isEnabled();
    }

    public void startScan() {
        if (bluetoothAdapter == null) {
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "SCAN_FAILED:NO_ADAPTER");
            return;
        }

        if (!bluetoothAdapter.isEnabled()) {
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "SCAN_FAILED:BT_DISABLED");
            return;
        }

        scanner = bluetoothAdapter.getBluetoothLeScanner();
        if (scanner == null) {
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "SCAN_FAILED:NO_SCANNER");
            return;
        }

        try {
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "SCAN_STARTED");
            scanner.startScan(scanCallback);
        } catch (SecurityException e) {
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "SCAN_FAILED:SECURITY");
        } catch (Exception e) {
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "SCAN_FAILED:" + e.getClass().getSimpleName());
        }
    }

    public void stopScan() {
        if (scanner != null) {
            scanner.stopScan(scanCallback);
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "SCAN_STOPPED");
        }
    }

    public void disconnect() {
        if (gatt != null) {
            gatt.disconnect();
            gatt.close();
            gatt = null;
            writeCharacteristic = null;
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleConnectionChanged", "DISCONNECTED");
        }
    }

    public void connectToAddress(final String address) {
        if (bluetoothAdapter == null)
            return;

        BluetoothDevice device = bluetoothAdapter.getRemoteDevice(address);
        if (device == null)
            return;

        UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "CONNECTING:" + address);
        gatt = device.connectGatt(activity, false, gattCallback);
    }

    public void writeCommand(final String command) {
        if (gatt == null || writeCharacteristic == null) {
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "WRITE_FAILED:NO_CHARACTERISTIC");
            return;
        }

        byte[] payload = (command + "\n").getBytes(StandardCharsets.UTF_8);
        writeCharacteristic.setValue(payload);
        boolean ok = gatt.writeCharacteristic(writeCharacteristic);

        UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus",
                ok ? "WRITE_OK:" + command : "WRITE_FAILED:" + command);
    }

    private final ScanCallback scanCallback = new ScanCallback() {
        @Override
        public void onScanResult(int callbackType, ScanResult result) {
            if (result == null || result.getDevice() == null)
                return;

            BluetoothDevice device = result.getDevice();
            String address = device.getAddress();

            String name = null;
            if (result.getScanRecord() != null) {
                name = result.getScanRecord().getDeviceName();
            }

            if (name == null || name.isEmpty()) {
                try {
                    name = device.getName();
                } catch (SecurityException ignored) {
                }
            }

            if (name == null || name.isEmpty()) {
                name = "N/A";
            }

            boolean matchesName = TARGET_NAME.equalsIgnoreCase(name.trim());
            boolean advertisesService = false;

            if (result.getScanRecord() != null) {
                List<ParcelUuid> uuids = result.getScanRecord().getServiceUuids();
                if (uuids != null) {
                    for (ParcelUuid parcelUuid : uuids) {
                        if (parcelUuid != null && SERVICE_UUID.equals(parcelUuid.getUuid())) {
                            advertisesService = true;
                            break;
                        }
                    }
                }
            }

            UnityPlayer.UnitySendMessage(
                UNITY_RECEIVER,
                "OnBleStatus",
                "SCAN_RESULT:" + name + "|" + address + "|svc=" + (advertisesService ? "1" : "0")
            );

            if (matchesName || advertisesService) {
                UnityPlayer.UnitySendMessage(
                    UNITY_RECEIVER,
                    "OnBleDeviceFound",
                    name + "|" + address
                );
            }
        }

        @Override
        public void onScanFailed(int errorCode) {
            UnityPlayer.UnitySendMessage(
                UNITY_RECEIVER,
                "OnBleStatus",
                "SCAN_FAILED:" + errorCode
            );
        }
    };

    private final BluetoothGattCallback gattCallback = new BluetoothGattCallback() {
        @Override
        public void onConnectionStateChange(BluetoothGatt g, int status, int newState) {
            if (status != BluetoothGatt.GATT_SUCCESS) {
                writeCharacteristic = null;

                UnityPlayer.UnitySendMessage(
                    UNITY_RECEIVER,
                    "OnBleStatus",
                    "GATT_ERROR:" + status
                );

                try {
                    g.close();
                } catch (Exception ignored) {
                }

                if (gatt == g) {
                    gatt = null;
                }

                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleConnectionChanged", "DISCONNECTED");
                return;
            }

            if (newState == BluetoothProfile.STATE_CONNECTED) {
                gatt = g;
                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "GATT_CONNECTED");
                g.discoverServices();
            } else if (newState == BluetoothProfile.STATE_DISCONNECTED) {
                writeCharacteristic = null;

                try {
                    g.close();
                } catch (Exception ignored) {
                }

                if (gatt == g) {
                    gatt = null;
                }

                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleConnectionChanged", "DISCONNECTED");
            }
        }

        @Override
        public void onServicesDiscovered(BluetoothGatt g, int status) {
            if (status != BluetoothGatt.GATT_SUCCESS) {
                UnityPlayer.UnitySendMessage(
                    UNITY_RECEIVER,
                    "OnBleStatus",
                    "SERVICE_DISCOVERY_FAILED:" + status
                );
                return;
            }

            BluetoothGattService service = g.getService(SERVICE_UUID);
            if (service == null) {
                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "SERVICE_NOT_FOUND");
                return;
            }

            writeCharacteristic = service.getCharacteristic(WRITE_UUID);
            if (writeCharacteristic == null) {
                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "WRITE_CHAR_NOT_FOUND");
                return;
            }

            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleStatus", "READY");
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBleConnectionChanged", "CONNECTED");
        }
    };
}