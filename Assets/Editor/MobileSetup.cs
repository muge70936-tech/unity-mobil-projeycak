using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// Sets the Android build settings automatically (mobile port by Prime MT).
[InitializeOnLoad]
public static class MobileSetup
{
	static MobileSetup()
	{
		PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.primemt.kolacikolata");
		PlayerSettings.productName = "Kola Cikolata";
		PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
		PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
		PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
		PlayerSettings.allowedAutorotateToPortrait = false;
		PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
		PlayerSettings.allowedAutorotateToLandscapeLeft = true;
		PlayerSettings.allowedAutorotateToLandscapeRight = true;
		PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Disabled);
	}
}
