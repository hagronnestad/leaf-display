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

String txtSoC = "txtSoC";
String txtTimeStamp = "txtTimeStamp";
String txtChargeText = "txtChargeText";
String txtChargeIcon = "txtChargeIcon";
String txtRangeNoAc = "txtRangeNoAc";
String txtRangeAc = "txtRangeAc";
String pbSoC = "pbSoC";

int socValue = 0;
String chargingStatus = "";
String chargingStatusFormatted = "";
String pluginState = "";
String pluginStateFormatted = "";
int rangeNoAc = 0;
int rangeAc = 0;
String chargeTime = "";
String timeStamp = "";

int redColor = 63488;
int greenColor = 2016;
int blueColor = 823;
int orangeColor = 64512;

// SETTINGS SETUP AND LOADING

#include <EEPROM.h>
struct Settings
{
    char ssid[32];
    char password[32];
    char leafDataUrl[128];
};

Settings s;

Settings SettingsSetup()
{
    Settings s;

    EEPROM.begin(512);
    EEPROM.get(0, s);

    Serial.println("\n\nType space to enter setup...");
    Serial.setTimeout(5000);

    String sm = Serial.readStringUntil('\n');

    if (sm == " ")
    {
        Serial.setTimeout(60000);
        Serial.println("Setup");
        Serial.println("--------------");

        Serial.print("Enter new SSID [" + String(s.ssid) + "]: ");
        String ssid = Serial.readStringUntil('\n');
        if (ssid.length() > 0)
            ssid.toCharArray(s.ssid, sizeof(s.ssid));
        Serial.println(String(s.ssid));

        Serial.print("Enter new password [" + String(s.password) + "]: ");
        String password = Serial.readStringUntil('\n');
        if (password.length() > 0)
            password.toCharArray(s.password, sizeof(s.password));
        Serial.println(s.password);

        Serial.print("Enter new Leaf Data URL [" + String(s.leafDataUrl) + "]: ");
        String leafDataUrl = Serial.readStringUntil('\n');
        if (leafDataUrl.length() > 0)
            leafDataUrl.toCharArray(s.leafDataUrl, sizeof(s.leafDataUrl));
        Serial.println(s.leafDataUrl);

        EEPROM.put(0, s);
        EEPROM.commit();
        EEPROM.end();

        Serial.println("\n\nSettings saved!\n");
    }
    else
    {
        Serial.println("Skipping setup!");
    }

    Serial.setTimeout(1000);
    return s;
}

// Called from Ticker
void ticker_updateDataFromApi()
{
    if ((WiFiMulti.run() != WL_CONNECTED))
    {
        return;
    }

    updateDataFromApi();
    updateScreen();
}

void updateDataFromApi()
{
    HTTPClient http;

    http.setTimeout(10000);

    http.begin(s.leafDataUrl);
    int httpCode = http.GET();

    //Serial.println("HTTP Code: " + String(httpCode));

    if (httpCode < 0)
    {
        //Serial.println("Feil: " + String(http.errorToString(httpCode).c_str()));
        http.end();
        return;
    }

    if (httpCode == HTTP_CODE_OK)
    {
        StaticJsonBuffer<2048> jsonBuffer;
        JsonObject &root = jsonBuffer.parseObject(http.getString());

        socValue = root["stateOfCharge"];
        chargingStatus = root["chargingStatus"].asString();
        chargingStatusFormatted = root["chargingStatusFormatted"].asString();
        pluginState = root["plugInState"].asString();
        pluginStateFormatted = root["plugInStateFormatted"].asString();
        rangeNoAc = root["cruisingRangeAcOff"];
        rangeAc = root["cruisingRangeAcOn"];
        chargeTime = root["minutesToFull200Formatted"].asString();
        timeStamp = root["timestampFormatted"].asString();
    }
    else
    {
        //Serial.println("Feil: " + String(http.errorToString(httpCode).c_str()));
    }

    http.end();
}

void setup()
{
    Serial.begin(115200);

    s = SettingsSetup();

    WiFi.disconnect(); // Clears last connected AP from flash

    WiFi.mode(WIFI_STA);
    WiFiMulti.addAP((String(s.ssid)).c_str(), (String(s.password).c_str()));

    setText(txtChargeText, "WiFi connecting...");
    Serial.print("Connecting to: '");
    Serial.print(s.ssid);
    Serial.print("', with password: '");
    Serial.print(s.password);
    Serial.print("'");

    while (WiFiMulti.run() != WL_CONNECTED)
    {
        delay(500);
        Serial.print(".");
    }
    Serial.println();

    Serial.print("Connected, IP address: ");
    Serial.println(WiFi.localIP());

    setText(txtChargeText, "WiFi connected!");
}

void loop()
{
    if ((WiFiMulti.run() != WL_CONNECTED))
    {
        delay(1000);
        return;
    }

    updateDataFromApi();
    updateScreen();

    delay(60000);
}

void sendToNextion(String command)
{
    Serial.print(command);
    Serial.write(nextionCommandEnd, sizeof(nextionCommandEnd));
}

void setText(String textBox, String text)
{
    sendToNextion(textBox + ".txt=\"" + text + "\"");
}

void setValue(String control, int val)
{
    sendToNextion(control + ".val=" + String(val));
}

void setPicture(String control, int val)
{
    sendToNextion(control + ".pic=" + String(val));
}

void setBackColor(String control, int color)
{
    sendToNextion(control + ".bco=" + color);
}

void setForeColor(String control, int color)
{
    sendToNextion(control + ".pco=" + color);
}

void updateScreen()
{
    // Send the Nextion end of command packet to "clear" the Nextion serial buffer
    sendToNextion("");

    // Update values
    setText(txtSoC, String(socValue) + "%");
    setValue(pbSoC, socValue);
    setText(txtTimeStamp, timeStamp);
    setText(txtRangeAc, String(rangeAc / 1000));
    setText(txtRangeNoAc, String(rangeNoAc / 1000));

    // Car dashboard
    float socDiv = 12.0f / 100.0f;
    setPicture("page0", round(socValue * socDiv));

    // Battery progress bar colors
    if (pluginState == "CONNECTED")
    {
        setForeColor(pbSoC, socValue >= 80 ? greenColor : orangeColor);
    }
    else
    {
        setForeColor(pbSoC, socValue <= 30 ? redColor : blueColor);
    }

    // Charging status text
    if (chargingStatus != "NOT_CHARGING")
    {
        setText(txtChargeText, "Charging...");
    }
    else if (pluginState == "CONNECTED" && chargingStatus == "NOT_CHARGING")
    {
        setText(txtChargeText, "Plugged in...");
    }
    else
    {
        setText(txtChargeText, "Nissan Leaf");
    }


    // Charging status icon
    setText(txtChargeIcon, chargingStatus == "NOT_CHARGING" ? "" : "^"); // '^' is the charge icon character in the font
}
