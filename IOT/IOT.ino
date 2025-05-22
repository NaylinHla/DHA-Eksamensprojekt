#include <WiFi.h>
#include <Wire.h>
#include <Adafruit_BME280.h>
#include <Adafruit_Sensor.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>
#include "time.h"
#include <OneWire.h>
#include <DallasTemperature.h>

// ----------------- CONFIG -----------------
#define SEALEVELPRESSURE_HPA (1013.25)

// Pins
#define MQ135_DIGITAL_PIN 4
#define MQ135_ANALOG_PIN  36
#define DS18B20_PIN       17
#define LED_PIN           16

// WiFi
const char* ssid     = "Your-Wifi-Name";
const char* password = "Your-Wifi-Password";

// MQTT
const char* mqttBroker = "Your-MGTT-Broker";
const int mqttPort = "Your-MQTT-Port";
const char* mqttToken = "Your-MQTT-Token"; 
const char* mqttTopicPublishBase = "Device/";
const char* deviceID = "Your-Device-Id";
String mqttTopicSubscribe = String("Device/") + deviceID + "/ChangePreferences";


// Time
const char* ntpServer = "pool.ntp.org";
const long gmtOffset_sec = 0;
const int daylightOffset_sec = 0;

// Wait time between data uploads
int waitTime = 10000;

// ----------------- OBJECTS -----------------
WiFiClient espClient;
PubSubClient mqttClient(espClient);
Adafruit_BME280 bme;
OneWire oneWire(DS18B20_PIN);
DallasTemperature ds18b20(&oneWire);

// ----------------- FUNCTIONS -----------------

String getUTCTime() {
  struct tm timeinfo;
  if (!getLocalTime(&timeinfo)) return "1970-01-01T00:00:00Z";
  char buffer[25];
  strftime(buffer, sizeof(buffer), "%Y-%m-%dT%H:%M:%SZ", &timeinfo);
  return String(buffer);
}

void mqttCallback(char* topic, byte* payload, unsigned int length) {
  String message;
  for (unsigned int i = 0; i < length; i++) message += (char)payload[i];

  DynamicJsonDocument doc(512);
  if (deserializeJson(doc, message)) return;

  String deviceId = doc["deviceId"];
  String intervalStr = doc["interval"];  // <- match field name
  int newWaitTime = intervalStr.toInt() * 1000;

  if (deviceId == String(deviceID) || deviceId == "+") {
    if (newWaitTime > 0) {
      waitTime = newWaitTime;
      Serial.printf("New wait time: %d ms\n", waitTime);
    }
  }
}


void connectToMQTT() {
  mqttClient.setServer(mqttBroker, mqttPort);
  mqttClient.setCallback(mqttCallback);

  while (!mqttClient.connected()) {
    Serial.print("Connecting to MQTT...");
    if (mqttClient.connect(deviceID, mqttToken, mqttToken)) {
      Serial.println("Connected!");
      mqttClient.subscribe(mqttTopicSubscribe.c_str());
      Serial.printf("Subscribed to topic: %s\n", mqttTopicSubscribe);
    } else {
      Serial.print("Failed, rc=");
      Serial.print(mqttClient.state());
      Serial.println(" Retrying in 5 seconds...");
      delay(5000);
    }
  }
}


void publishSensorData(float press, float hum, float tempDS, int airA) {
  StaticJsonDocument<512> doc;
  doc["deviceId"] = deviceID;
  doc["pressure"] = press;
  doc["humidity"] = hum;
  doc["temperature"] = static_cast<float>(roundf(tempDS * 100) / 100.0);
  doc["air_quality_analog"] = airA;
  doc["timestamp"] = getUTCTime();

  char buffer[512];
  serializeJson(doc, buffer);
  String topic = String(mqttTopicPublishBase) + deviceID + "/SensorData";
  mqttClient.publish(topic.c_str(), buffer);
}

// ----------------- SETUP -----------------

void setup() {
  Serial.begin(115200);
  pinMode(LED_PIN, OUTPUT);
  pinMode(MQ135_DIGITAL_PIN, INPUT);
  pinMode(MQ135_ANALOG_PIN, INPUT);
  Wire.begin();  

  ds18b20.begin();
  if (!bme.begin(0x76)) {
    Serial.println("BME280 not found!");
    while (1);
  }

  Serial.print("Connecting to WiFi");
  WiFi.begin(ssid, password);
  unsigned long wifiTimeout = millis();
  while (WiFi.status() != WL_CONNECTED && millis() - wifiTimeout < 20000) {
    delay(500);
    Serial.print(".");
  }
  Serial.println(WiFi.status() == WL_CONNECTED ? "\nWiFi connected." : "\nWiFi failed.");

  configTime(gmtOffset_sec, daylightOffset_sec, ntpServer);
  connectToMQTT();
}

// ----------------- LOOP -----------------

void loop() {
  static unsigned long lastSent = 0;
  mqttClient.loop();

  if (millis() - lastSent >= waitTime) {
    lastSent = millis();

    float tempBME = bme.readTemperature();
    float pressure = bme.readPressure() / 100.0F;
    float altitude = bme.readAltitude(SEALEVELPRESSURE_HPA);
    float humidity = bme.readHumidity();

    ds18b20.requestTemperatures();
    float tempDS = ds18b20.getTempCByIndex(0);

    int airQualityD = digitalRead(MQ135_DIGITAL_PIN);
    int airQualityA = analogRead(MQ135_ANALOG_PIN);

    digitalWrite(LED_PIN, tempBME > 24.0 ? HIGH : LOW);

    Serial.printf("BME Temp: %.2f°C | DS18B20 Temp: %.2f°C | Pressure: %.2f hPa | Altitude: %.2f m | Humidity: %.2f%% | Air A: %d | Air D: %d\n",
      tempBME, tempDS, pressure, altitude, humidity, airQualityA, airQualityD);

    publishSensorData(pressure, humidity, tempDS, airQualityA);
  }
}
