# Win10VirtualDesktopDeskBand
Provides a Windows Taskbar widget for creating & selecting <u>NAMED</u> Virtual Desktops.<br/>
Notably, it displays the currently selected desktop name at a glance, even when you change via Windows' native UX.

pretty quick and dirty combination of:
- ~~[SharpShell](https://github.com/dwmkerr/sharpshell) for the DeskBand (aka Toolbar)~~
- [CSDeskband](https://github.com/dsafa/CSDeskBand) for it's WPF Support, which provides background transparency, SharpShell seemed to be WinForms only 
- [VirtualDesktop](https://github.com/Grabacr07/VirtualDesktop) helper library... boy is this some complex code! no way i would've got that working
  - i've embedded the source vs referencing nuget because had to do a few small patches on top of v4.0.1 (search for "fix:") to get to work under .Net Framework (4.7.2), necessary because i don't think COM regasm is supported by .net core quite yet... sounds like they're on it

![](https://user-images.githubusercontent.com/6301228/82292998-aa391800-9960-11ea-9b6c-39ee87ff8677.png)
![](https://user-images.githubusercontent.com/6301228/82292734-4282cd00-9960-11ea-9c2d-072737dbc82f.gif)

## Notes
- had to register the main assembly (and some dependent assemblies) in gac for it to find them when starting up in the windows taskbar, [docs](https://github.com/dsafa/CSDeskBand/wiki#deskband-installation)
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
- ~~couldn't resist adding CalcBinding dependency for code laziness... it's gotta go in the gac as well, but waa, it's not strongnamed?! [this tool](https://brutaldev.com/post/NET-Assembly-Strong-Name-Signer) for the win!~~ nope, couldn't get past the lack of a publickeytoken on that assembly even with the strong-name
- had to do a bindingRedirect in **!machine.config!** to get it to load the v4.0.5.0 of System.Runtime.CompilerServices.Unsafe.dll that was present on my system versus wanting to load 4.0.3.0 that was linked in one of the dependent assemblies.
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
- there's a wpf TestApp in this solution that embeds the DeskBandUserControl so you can develop changes conveniently, without worrying about blowups being hidden by Windows' runtime context <span style="background-color: yellow; color: black">BUT you have to unregister from the gac for the TestApp to pick up on the new local build of the UserControl assembly vs the gac</span>

- in true quick n' dirty form, all the code is piled up in DeskBandUserControl.cs for this first shot at it... it's all boilerplate overhead for a little UI work
- briefly explored doing this as a .Net core based project but [Visual Studio 2019 16.6 Preview 1 Windows Forms Designer is not quite ready for User Controls yet](https://devblogs.microsoft.com/dotnet/updates-on-net-core-windows-forms-designer/)
- **started** using  [Rick Strahl's  app.config management lib](https://github.com/RickStrahl/Westwind.ApplicationConfiguration) to persist the desktop names... needed to be signed/strong-named to compile into this strong-named assembly... signing  makes installing this extension a little cleaner... used [this handy tool](https://brutaldev.com/post/NET-Assembly-Strong-Name-Signer) to sign the 3rd party lib that didn't come that way
  - but then dissapointingly discovered after all the fancy stuff his lib did well, it didn't handle simple deleting of a list item... it just saves the current list items and leaves the old ones?!
  - [switched to a simple xml file save](https://github.com/Beej126/Win10VirtualDesktopDeskBand/blob/aef58f938eca450dab1bca6a2dcfaf2eb9bc9e73/Win10VirtualDesktopDeskBand/DeskBandUserControl.xaml.cs#L151) since not much is needed here, works great!

## Reference Links
### DeskBand
- https://github.com/dsafa/CSDeskBand
- https://github.com/navhaxs/media-control-deskband
- https://github.com/Tom60chat/DeskBand-Media-Controls - better installer to model than the above... UX was flakey for me... that seems to be really common... the COM level where these extensions get registered into Windows Explorer is pretty hard to get right, maybe that's some of why Msft chucked them from Win11
- https://github.com/patbec/TaskbarSampleExtension
### Virtual Desktop
- https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ivirtualdesktopmanager
- https://github.com/Grabacr07/VirtualDesktop
- https://github.com/m0ngr31/VirtualDesktopManager/issues/13
- https://stackoverflow.com/questions/32659505/windows-10-ivirtualdesktopmanagermovewindowtodesktop
- IActionView CLSID updates: https://github.com/mzomparelli/zVirtualDesktop/wiki
