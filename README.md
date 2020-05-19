# Win10VirtualDesktopDeskBand
Provides a Windows Taskbar widget for selecting <u>NAMED</u> Virtual Desktops.

pretty quick and dirty combination of:
- ~~[SharpShell](https://github.com/dwmkerr/sharpshell) for the DeskBand (aka Toolbar)~~
- [CSDeskband](https://github.com/dsafa/CSDeskBand) for it's WPF Support, which provides background transparency, SharpShell seemed to be WinForms only 
- [VirtualDesktop](https://github.com/Grabacr07/VirtualDesktop) helper library... boy is this some complex code! no way i would've got that working
  - i've embedded the source vs referencing nuget because had to do a few small patches on top of v4.0.1 (search for "fix:") to get to work under .Net Framework (4.7.2), necessary because i don't think COM regasm is supported by .net core quite yet... sounds like they're on it


## Notes
- had to register the main assembly and all dependent assemblies in gac for it to find them all, [docs](https://github.com/dsafa/CSDeskBand/wiki#deskband-installation)
  ```bat
  cd ~/repos/Win10VirtualDesktopDeskBand/Win10VirtualDesktopDeskBand/bin/debug
  dir *.dll | % { gacutil -i $_.Name }
  ```
  - for gac, it has to be signed/strong-named
  - this key.snk came with sharpshell sample proj and doesn't require password<br/></br>
  * then just regasm the main assembly to make it COM visible
    ```bat
    regasm Win10VirtualDesktopDeskBand.dll
    ```
- ~~couldn't resist adding CalcBinding dependency for code laziness... it's gotta go in the gac as well, but waa, it's not strongnamed?! [this tool](https://brutaldev.com/post/NET-Assembly-Strong-Name-Signer) for the win!~~ nope, couldn't get passed the lack of a publickeytoken on that assembly even with the strong-name
- had to do a bindingRedirect in !machine.config! to get it to load the v4.0.5.0 of System.Runtime.CompilerServices.Unsafe.dll that was present on my system versus wanting to load 4.0.3.0 that was linked in one of the dependent assemblies.
  - C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Config\machine.config
  ```
    <runtime>
        <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
        <dependentAssembly>
            <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
            <bindingRedirect oldVersion="0.0.0.0-4.0.5.0" newVersion="4.0.5.0" />
        </dependentAssembly>
        </assemblyBinding>
    </runtime>
  ```
- there's a wpf testapp along with this solution that embeds the usercontrol so you can develop changes conveniently without worrying about blowups being hidden by the windows runtime context <span style="background-color: yellow; color: black">BUT you have to unregister from the gac for the app to pick up on the local copy vs the gac</span>

- in true hack form, all the code is in DeskBandUserControl.cs
- briefly explored .Net core based project but [Visual Studio 2019 16.6 Preview 1 Windows Forms Designer is not quite ready for User Controls yet](https://devblogs.microsoft.com/dotnet/updates-on-net-core-windows-forms-designer/)
- **started** using  [Rick Strahl's  app.config management lib](https://github.com/RickStrahl/Westwind.ApplicationConfiguration) to persist the desktop names... needed to be signed/strong-named to compile into this strong-named assembly... signing  makes installing this extension a little cleaner... used [this handy tool](https://brutaldev.com/post/NET-Assembly-Strong-Name-Signer) to sign the 3rd party lib that didn't come that way
  - but then dissapointingly discovered after all the fancy stuff his lib did well, it didn't handle simple deleting of a list item... it just saves the current list items and leaves the old ones?!
  - switched to a simple xml file save since not much is needed here
