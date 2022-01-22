# Rhino.Toolkit

A set of tools to help people to develop plugins or use Rhino more efficiently.

- Rhino.Toolkit.PlugInLoader: it is inspired by plugin "AddinManager" of Revit. Developer can debug without restart Rhino.

  Usage:

  - Input PluginManager in command to load Rhino.Toolkit.PlugInLoader
  - Input PlugInLoader to start main window
  - In the main window, click button "Load" to load plugin dll, and then all command in this plugin will be add to list
  - Select a command from list, and click button "Run" to execute it.
  - If debug, just attach to Rhino and add a breakpoint; After change some code, just build and reattach.

- Rhino.Toolkit.PlugInUninstaller: Uninstall a plugin via registry where Rhino store information of plugin

- Rhino.Toolkit.ModelImporter: Import model from .3dm file, and set insert point with mouse while default import just insert model to origin