# KasaMatricIntegration
Console application to act as a plugin for the [Matric](https://matricapp.com/) Android macro key application.

It queries TP-Link Kasa devices configured in appsettings.json and updates the state of either buttons or server variables (Matric 2.8+) in Matric.

# Requirements
- [Python](https://www.python.org/downloads/release/) (tested with Python 3.12)<br/>
- [PyKasa] module (https://github.com/python-kasa/python-kasa)<br/>
- Asyncio module

Currently experimenting with setting up a virtual or pseudo-virtual environment for application to run as a service. Running as an application should work fine with globally available Python modules. With a \Lib\site-packages directory in the same directory as KasaMatricIntegration.exe, you may be able to run as a service under your normal login user.

# Appsettings
See [Appsettings.json](KasaIntegration/Appsettings.json) for latest settings
