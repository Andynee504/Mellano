#include <Wire.h>
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <BleGamepad.h>

Adafruit_MPU6050 mpu;

// ===== PINOS =====
const int SDA_PIN = 22;
const int SCL_PIN = 21;

const int PEDAL_1_PIN = 18;  // A?
const int PEDAL_2_PIN = 19;  // B?
const int PEDAL_3_PIN = 23;  // reset/start

// ===== CONFIGURACAO =====
float centerX = 0.0f;
float centerY = 0.0f;

float deadzone = 0.80f;  // multiplicador** // zona morta em m/s2
float sensX = 18.0f;     // ganho do eixo X
float sensY = 18.0f;     // ganho do eixo Y

bool invertX = false;
bool invertY = false;

// ===== GAMEPAD =====
BleGamepad bleGamepad("Mellano Proto", "PUCSP", 100);

// ===== FAIXA HID =====
const int HID_MIN = 0;
const int HID_MAX = 32767;
const int HID_CENTER = 16384;

// ===== SAIDA LOGICA FINAL =====
int axisX = 0;  // -127 a 127
int axisY = 0;  // -127 a 127

bool btnA = false;
bool btnB = false;
bool btnX = false;

// ===== CONTROLE =====
unsigned long lastPrint = 0;
const unsigned long printInterval = 100;  // ms

// ----------------------------
// Helpers/utils
// ----------------------------
int clampAxis(int value) {
  if (value > 127) return 127;
  if (value < -127) return -127;
  return value;
}

float applyDeadzone(float value, float dz) {
  if (value > -dz && value < dz) return 0.0f;
  return value;
}
/*
int convertToAxis(float value, float sensitivity, bool invertAxis) {
  float adjusted = applyDeadzone(value, deadzone);

  if (invertAxis) {
    adjusted *= -1.0f;
  }

  int mapped = (int)(adjusted * sensitivity);
  return clampAxis(mapped);
}
*/

int mapAxisToHID(float value, float sensitivity, bool invertAxis) {
  float adjusted = applyDeadzone(value, deadzone);

  if (invertAxis) {
    adjusted *= -1.0f;
  }

  int signedValue = (int)(adjusted * sensitivity);  // calcular para perto** (-127 - 127) depende da sensibilidade
  signedValue = clampInt(signedValue, -127, 127);

  // De (-127 - 127) para (0 - 32767)
  long hidValue = map(signedValue, -127, 127, HID_MIN, HID_MAX);
  return (int)clampInt((int)hidValue, HID_MIN, HID_MAX);
}

int clampInt(int value, int minValue, int maxValue) {
  if (value < minValue) return minValue;
  if (value > maxValue) return maxValue;
  return value;
}

void calibrateCenter() {
  sensors_event_t a, g, temp;

  float sumX = 0.0f;
  float sumY = 0.0f;
  const int samples = 30;

  Serial.println("=== CALIBRANDO ===");
  for (int i = 0; i < samples; i++) {
    mpu.getEvent(&a, &g, &temp);
    sumX += a.acceleration.x;
    sumY += a.acceleration.y;
    delay(20);
  }

  centerX = sumX / samples;
  centerY = sumY / samples;

  Serial.println("=== RESULTADO ===");
  Serial.print("centerX: ");
  Serial.println(centerX, 3);
  Serial.print("centerY: ");
  Serial.println(centerY, 3);
  Serial.println("=================");
}

void setupGamepad() {
  BleGamepadConfiguration config; // ?????

  config.setAutoReport(false);
  config.setControllerType(CONTROLLER_TYPE_GAMEPAD);
  config.setButtonCount(3);
  config.setHatSwitchCount(0);

  // Botoes A e B
  config.setWhichAxes(true, true, false, false, false, false, false, false);

  config.setAxesMin(HID_MIN);
  config.setAxesMax(HID_MAX);

  // Botoes especiais extras
  config.setIncludeStart(false);
  config.setIncludeSelect(false);
  config.setIncludeMenu(false);
  config.setIncludeHome(false);
  config.setIncludeBack(false);
  config.setIncludeVolumeInc(false);
  config.setIncludeVolumeDec(false);
  config.setIncludeVolumeMute(false);

  bleGamepad.begin(&config);
}

void readMPUAndMapAxesAndButtons() {
  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  float relativeX = a.acceleration.x - centerX;
  float relativeY = a.acceleration.y - centerY;

  int hidX = mapAxisToHID(relativeX, sensX, invertX);
  int hidY = mapAxisToHID(relativeY, sensY, invertY);

  btnA = (digitalRead(PEDAL_1_PIN) == LOW);
  btnB = (digitalRead(PEDAL_2_PIN) == LOW);
  btnX = (digitalRead(PEDAL_3_PIN) == LOW);

  axisX = hidX;
  axisY = hidY;

  if (bleGamepad.isConnected()) {
    bleGamepad.setX(axisX);
    bleGamepad.setY(axisY);

    if (btnA) bleGamepad.press(BUTTON_1);
    else bleGamepad.release(BUTTON_1);

    if (btnB) bleGamepad.press(BUTTON_2);
    else bleGamepad.release(BUTTON_2);

    if (btnX) bleGamepad.press(BUTTON_3);
    else bleGamepad.release(BUTTON_3);

    bleGamepad.sendReport();
  }
}

void printState() {
  Serial.println("--- INPUT STATE ---");

  Serial.print("BLE: ");
  Serial.print(bleGamepad.isConnected() ? "ON" : "OFF");

  Serial.print("axisX: ");
  Serial.println(axisX);

  Serial.print("axisY: ");
  Serial.println(axisY);

  Serial.print("btnA: ");
  Serial.println(btnA ? 1 : 0);

  Serial.print("btnB: ");
  Serial.println(btnB ? 1 : 0);

  Serial.print("btnX: ");
  Serial.println(btnX ? 1 : 0);

  Serial.println("-------------------");
}

void handleSerialCommands() {
  if (!Serial.available()) return;

  char cmd = Serial.read();

  if (cmd == 'c' || cmd == 'C') {
    calibrateCenter();
  } else if (cmd == 'p' || cmd == 'P') {
    Serial.println("=== CONFIG ===");
    Serial.print("deadzone: ");
    Serial.println(deadzone, 3);
    Serial.print("sensX: ");
    Serial.println(sensX, 3);
    Serial.print("sensY: ");
    Serial.println(sensY, 3);
    Serial.print("invertX: ");
    Serial.println(invertX ? 1 : 0);
    Serial.print("invertY: ");
    Serial.println(invertY ? 1 : 0);
    Serial.println("==============");
  }
}

void setup() {
  Serial.begin(115200);
  delay(300);

  pinMode(PEDAL_1_PIN, INPUT_PULLUP);
  pinMode(PEDAL_2_PIN, INPUT_PULLUP);
  pinMode(PEDAL_3_PIN, INPUT_PULLUP);

  Wire.begin(SDA_PIN, SCL_PIN);
  delay(300);

  if (!mpu.begin()) {
    Serial.println("ERRO: MPU6050 nao encontrado.");
    while (true) {
      delay(50);
    }
  }

  mpu.setAccelerometerRange(MPU6050_RANGE_8_G);
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);
  mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);

  Serial.println("MPU6050 iniciado.");
  Serial.println("Comandos:");
  Serial.println("  c = recalibrar centro");
  Serial.println("  p = imprimir configuracao");

  calibrateCenter();
  setupGamepad();

  Serial.println("BLE gamepad pronto.");
  Serial.println("Pareie o dispositivo 'Mellano Proto' no celular.");
}

void loop() {
  handleSerialCommands();

  readMPUAndMapAxesAndButtons();
  /*
  if (millis() - lastPrint >= printInterval) {
    lastPrint = millis();
    printState();
  }
  */
}