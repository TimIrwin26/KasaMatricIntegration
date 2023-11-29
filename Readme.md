# KasaMatricIntegration
Console application to act as a plugin for the [Matric](https://matricapp.com/) Android macro key application.

It queries TP-Link Kasa devices configured in appsettings.json and updates either buttons or server variables (Matric 2.8+).

# Requirements
[Python](https://www.python.org/downloads/release/) (tested with Python 3.12)
[PyKasa](https://github.com/python-kasa/python-kasa)

# Appsettings
    {
      "Logging": { "Console": { "LogLevel": { "Level": "Debug" } } },
      "PythonDll": "python312.dll",
      "Matric": {
        "Pin": "6140",
        "ApiPort": 50300,
        "PollingIntervalSeconds": 2,
        //"KasaVariables": [
        //  {
        //    "Name": "pi_kasa_166_on",
        //    "DeviceIp": "192.168.10.166",
        //    "Outlet": ""
        //  }
        //],
        "KasaButtons": [
          {
            "Id": "287f2b2b-c3c6-4e5d-a93c-7f63e9ef668b",
            "Name": "kasa_166",
            "DeviceIp": "192.168.10.166",
            "Outlet": ""
          }
        ]
      }
    }