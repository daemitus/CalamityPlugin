# CalamityPlugin
This plugin allows for sending a GET to http://127.0.0.1/unload_dalamud to unload Dalamud. Useful for incorporating into your Dalamud DEBUG post-build events. Thanks to Caraxi for the idea, drop your DLL in devPlugins or use it in conjuction with [LivePluginLoad](https://github.com/Caraxi/LivePluginLoad/tree/master/LivePluginLoad).

## Usage
```
echo Unloading Dalamud
curl -m 2 "http://localhost:37435/unload_dalamud" 2>NUL
```
