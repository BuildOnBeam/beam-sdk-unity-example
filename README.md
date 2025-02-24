# Beam SDK Unity Example

This demo integrates [Beam SDK Unity](https://github.com/BuildOnBeam/beam-sdk-unity) to showcase how you can initialize Operation Signing and Sessions.

![img.png](img.png)

## Docs
To read up on Sessions and Operations, please visit or [Player API docs](https://docs.onbeam.com/service/introduction-player-api).

## Setting up
Load the project via Unity Hub, add `scenes/SampleScene` to your Hierarchy view.  

In order to use this example with your game, you need to set your Publishable Beam API Key on UIDocument:
![api-key-setup.png](api-key-setup.png)

Entire Beam SDK usage can be seen in [Assets/BeamUI.cs](Assets/BeamUI.cs) which sets up listeners and basic UI logic.

## Beam Unity SDK
You can find more info on Beam Unity SDK here:
[Beam SDK Unity](https://github.com/BuildOnBeam/beam-sdk-unity)

### Using WebView
By overriding the `SetUrlOpener` method you can open the URL in whatever way you prefer: 
```csharp
beamClient.SetUrlOpener(url => { InitWebview(url); });
```
In our example, when running in iOS we will use Safari View Controller, in case of Android - Custom Chrome Tab(using [onedevapp/Unity_ChromeCustomTabs](https://github.com/onedevapp/Unity_ChromeCustomTabs)).
For all other cases we will be calling default Url Opening method which is Unity's `Application.OpenURL()` that uses any browser app installed.