# CalamityPlugin
This plugin allows for sending a GET to http://127.0.0.1/unload_dalamud to unload Dalamud. Useful for incorporating into your Dalamud DEBUG post-build events. Thanks to Caraxi for the idea. Drop the DLL in devPlugins or use it in conjuction with [LivePluginLoad](https://github.com/Caraxi/LivePluginLoad/tree/master/LivePluginLoad).

## Usage
```
if $(ConfigurationName) == Debug (
    echo Running Debug Post Build

    echo Unloading Dalamud
    curl -m 2 "http://localhost:37435/unload_dalamud" 2>NUL
    waitfor NothingAtAll /t 2 2>NUL

    echo Copying to XIVLauncher
    xcopy /F /Y "$(TargetPath)" "%AppData%\XIVLauncher\addon\Hooks\"

    echo Injecting Dalamud
    cd "%AppData%\XIVLauncher\addon\Hooks"
    "%AppData%\XIVLauncher\addon\Hooks\Dalamud.Injector.exe"

    exit /b 0
)
```
