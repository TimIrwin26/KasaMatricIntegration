# KasaMatricIntegration
Console application to act as a plugin for the [Matric](https://matricapp.com/) Android macro key application.

It queries TP-Link Kasa devices configured in appsettings.json and updates the state of either buttons or server variables (Matric 2.8+) in Matric.

# Requirements
- [Python](https://www.python.org/downloads/release/) (tested with Python 3.12)<br/>
- [PyKasa] module (https://github.com/python-kasa/python-kasa)<br/>
- Asyncio module

Currently experimenting with setting up a virtual or pseudo-virtual environment for application to run as a service. Running as an application should work fine with globally available Python modules. With a \Lib\site-packages directory in the same directory as KasaMatricIntegration.exe, you may be able to run as a service under your normal login user.

# Appsettings
```json
{
  "Logging": {
    "PathFormat": "Logs/KasaMatricIntegration-{Date}.txt",
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Information"
    },
    "Debug": { "LogLevel": { "Level": "Trace" } },
    "EventLog": {
      "SourceName": "Kasa Matric Integration",
      "LogName": "Application",
      "LogLevel": {
        "KasaMatricIntegration.MatricIntegration.MatricService": "Warning",
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Warning"
      }
    }
  },
  "PythonDll": "python312.dll",
  "Matric": {
    "Pin": "6140",
    "ApiPort": 50300,
    "KasaPollingIntervalSeconds": 1,
    "MatricPollingIntervalSeconds": 10,
    //"KasaVariables": [
    //  {
    //    "Name": "kasa_166",
    //    "DeviceIp": "192.168.10.166",
    //    "Outlet": ""
    //  }
    //]
    //,
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
```
