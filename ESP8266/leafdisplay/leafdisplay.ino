#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>
#include <Ticker.h>
#include <ESP8266HTTPClient.h>
#include <ArduinoJson.h>

Ticker ticker;
ESP8266WiFiMulti WiFiMulti;

// NEXTION
const uint8_t nextionCommandEnd[3] = {0xFF, 0xFF, 0xFF};

String txtSoCId = "t0";
String pbSoCId = "j0";

String socPercent = "0";
int socValue = 0;



// SETTINGS SETUP AND LOADING

#include <EEPROM.h>
struct Settings {
  char ssid[32];
  char password[32];
  char leafDataUrl[128];
};

Settings s;

Settings SettingsSetup() {
  Settings s;
  
  EEPROM.begin(512);
  EEPROM.get(0, s);
  
  Serial.println("\n\nType space to enter setup...");
  Serial.setTimeout(3000);
  
  String nl = Serial.readStringUntil('\n');

  if (nl != " ") {
    Serial.println("Skipping setup!");
    Serial.setTimeout(1000);
    return s;
  }

  Serial.setTimeout(60000);
  Serial.println("Setup");
  Serial.println("--------------");
  
  Serial.print("Enter new SSID [" + String(s.ssid) + "]: ");
  String ssid = Serial.readStringUntil('\n');
  if (ssid.length() > 0) ssid.toCharArray(s.ssid, sizeof(s.ssid));
  Serial.println(String(s.ssid));

  Serial.print("Enter new password [" + String(s.password) + "]: ");
  String password = Serial.readStringUntil('\n');
  if (password.length() > 0) password.toCharArray(s.password, sizeof(s.password));
  Serial.println(s.password);

  Serial.print("Enter new Leaf Data URL [" + String(s.leafDataUrl) + "]: ");
  String leafDataUrl = Serial.readStringUntil('\n');
  if (leafDataUrl.length() > 0) leafDataUrl.toCharArray(s.leafDataUrl, sizeof(s.leafDataUrl));
  Serial.println(s.leafDataUrl);

  EEPROM.put(0, s);
  EEPROM.commit();
  EEPROM.end();

  Serial.println("\n\nSettings saved!\n");

  Serial.setTimeout(1000);
  return s;
}


// Called from Ticker
void ticker_updateDataFromApi() {
    if((WiFiMulti.run() != WL_CONNECTED)) {
        return;
    }

    updateDataFromApi();
    updateScreen();
}

void updateDataFromApi() {
    HTTPClient http;

    http.setTimeout(10000);
  
    http.begin(s.leafDataUrl);
    int httpCode = http.GET();

    Serial.println("HTTP Code: " + String(httpCode));

    if (httpCode < 0) {
        Serial.println("Feil: " + String(http.errorToString(httpCode).c_str()));
        http.end();
        return;
    }

    if(httpCode == HTTP_CODE_OK) {
        StaticJsonBuffer<1024> jsonBuffer;
        JsonObject& root = jsonBuffer.parseObject(http.getString());

        Serial.println(http.getString());

        socPercent = root["SoC"].asString();
        socValue = root["SoC"];
    }

    http.end();
}




void setup() {
  Serial.begin(9600);
  s = SettingsSetup();

  WiFi.disconnect(); // Clears last connected AP from flash
  //WiFi.begin((String(s.ssid)).c_str(), (String(s.password).c_str()));

  WiFi.mode(WIFI_STA);
  WiFiMulti.addAP((String(s.ssid)).c_str(), (String(s.password).c_str()));

  Serial.print("Connecting to: '");
  Serial.print(s.ssid);
  Serial.print("', with password: '");
  Serial.print(s.password);
  Serial.print("'");
  
  //while (WiFi.status() != WL_CONNECTED)
  while (WiFiMulti.run() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }
  Serial.println();

  Serial.print("Connected, IP address: ");
  Serial.println(WiFi.localIP());



  //ticker.attach(5, ticker_updateDataFromApi);
}

void loop() {
      if((WiFiMulti.run() != WL_CONNECTED)) {
        return;
    }

    updateDataFromApi();
    updateScreen();

    delay(5000);
}








void sendToNextion(String command) {
    Serial.print(command);
    Serial.write(nextionCommandEnd, sizeof(nextionCommandEnd));
}


void setText(String textBox, String text) {
    sendToNextion(textBox + ".txt=\"" + text + "\"");
}

void setValue(String control, int val) {
    sendToNextion(control + ".val=" + String(val));
}

void setBackColor(String control, String color) {
    sendToNextion(control + ".bco=" + color);
}


void updateScreen() {
    setText(txtSoCId, socPercent);
    setValue(pbSoCId, socValue);
}
