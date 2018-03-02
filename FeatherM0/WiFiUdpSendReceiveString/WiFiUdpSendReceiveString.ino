/*
  WiFi UDP Send and Receive String

  This sketch wait an UDP packet on localPort using a WiFi shield.
  When a packet is received an Acknowledge packet is sent to the client on port remotePort

  Circuit:
   WiFi shield attached

  created 30 December 2012
  by dlf (Metodo2 srl)

*/


#include <SPI.h>
#include <WiFi101.h>
#include <WiFiUdp.h>
#include <IRLibAll.h>
#include "arduino_secrets.h"

int status = WL_IDLE_STATUS;

///////please enter your sensitive data in the Secret tab/arduino_secrets.h
char ssid[] = SECRET_SSID;        // your network SSID (name)
char pass[] = SECRET_PASS;    // your network password (use for WPA, or use as key for WEP)


WiFiUDP udp;
IPAddress server(10, 0, 0, 74);

char connect_server[34]; // the connect message for the server
char server_response_data[255]; //buffer to hold incoming packet from the server

char serial_number[33];
char username[255];

IRrecvPCI myReceiver(15);
IRdecode myDecoder;

void setup()
{
  //Configure pins for Adafruit ATWINC1500 Feather
  WiFi.setPins(8, 7, 4, 2);
  connectWifi();

  Serial.println("\nStarting connection to server...");
  // if you get a connection, report back via serial:
  udp.begin(PORT);

  // get our unique serial number, this unique serial number is tied to your username
  getSerialNumber(serial_number);

  // connect to the server and obtain our username
  strcpy (connect_server, CONNECT_SERVER_OPERATION);
  strcat (connect_server, serial_number);
  sendMessage(connect_server); // send the connect message to the server
  receiveMessage(server_response_data); // receive the connect response from the server
  handleResponse(server_response_data); // handle the response from the server

  myReceiver.enableIRIn(); // Start the receiver
  Serial.println(F("Ready to receive IR signals"));
}

void loop()
{
  //Continue looping until you get a complete signal received
  if (myReceiver.getResults()) {
    myDecoder.decode();           //Decode it
    myDecoder.dumpResults(true);  //Now print results. Use false for less detail

    if (myDecoder.value == 0x10EFD827)
    {
      Serial.println("IR Remote POWER BUTTON");
    }
    
    myReceiver.enableIRIn();      //Restart receiver
  }
}

void getSerialNumber(char * serial_number) {
  volatile uint32_t val1, val2, val3, val4;
  volatile uint32_t *ptr1 = (volatile uint32_t *)0x0080A00C;
  val1 = *ptr1;
  volatile uint32_t *ptr = (volatile uint32_t *)0x0080A040;
  val2 = *ptr;
  ptr++;
  val3 = *ptr;
  ptr++;
  val4 = *ptr;

  Serial.print("Serial Number: 0x");
  char buf[33];
  sprintf(buf, "%8x%8x%8x%8x", val1, val2, val3, val4);
  Serial.println(buf);

  strcat (serial_number, buf);
}

void connectWifi()
{
  // check for the presence of the shield:
  if (WiFi.status() == WL_NO_SHIELD) {
    Serial.println("WiFi shield not present");
    // don't continue:
    while (true);
  }

  // attempt to connect to WiFi network:
  while ( status != WL_CONNECTED) {
    Serial.print("Attempting to connect to SSID: ");
    Serial.println(ssid);
    // Connect to WPA/WPA2 network. Change this line if using open or WEP network:
    status = WiFi.begin(ssid, pass);

    // wait 10 seconds for connection:
    delay(WIFI_CONNECTION_DELAY);
  }

  Serial.println("Connected to wifi");
  printWiFiStatus();
}

void printWiFiStatus() {
  // print the SSID of the network you're attached to:
  Serial.print("SSID: ");
  Serial.println(WiFi.SSID());

  // print your WiFi shield's IP address:
  IPAddress ip = WiFi.localIP();
  Serial.print("IP Address: ");
  Serial.println(ip);

  // print the received signal strength:
  long rssi = WiFi.RSSI();
  Serial.print("signal strength (RSSI):");
  Serial.print(rssi);
  Serial.println(" dBm");
}

void sendMessage(char message[])
{
  Serial.print("Sending Message: ");
  Serial.println(message);

  // Ask the connect to the server and obtain our username
  udp.beginPacket(server, 8000);
  udp.write(message);
  udp.endPacket();
}

void receiveMessage(char * response)
{
  // if there's data available, read a packet
  int packetSize = udp.parsePacket();
  char packetBuffer[255];

  while (!packetSize) packetSize = udp.parsePacket();

  Serial.print("Received packet of size ");
  Serial.println(packetSize);
  Serial.print("From ");
  IPAddress remoteIp = udp.remoteIP();
  Serial.print(remoteIp);
  Serial.print(", port ");
  Serial.println(udp.remotePort());

  // read the packet into packetBufffer
  int len = udp.read(response, 255);
  if (len > 0) response[len] = 0;
  Serial.println("Contents:");
  Serial.println(response);
}

void handleResponse(char response[])
{
  if (strlen(response) < 1)
    return;

  switch (response[0])
  {
    case CONNECT_SERVER_OPERATION[0]:
      {
        memmove(username, response + 1, strlen(response));
        Serial.print("Username: ");
        Serial.println(username);
        break;
      }

    default:
      {
        break;
      }
  }
}

