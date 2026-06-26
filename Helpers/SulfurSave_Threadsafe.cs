using System;
using System.Collections;
using System.Threading;
using PerfectRandom.Sulfur.Core;

namespace Archipelago.Helpers;

public class SulfurSave_Threadsafe(bool newFileRequired) : SulfurSave_PC
{
	private bool _newFileRequired = newFileRequired;
    private Semaphore _semaphore = new Semaphore(1, 1);
    
    public override void SaveToDisk<T>(string key, T value)
    {
        _semaphore.WaitOne();
        base.SaveToDisk(key, value);
        _semaphore.Release();
    }
    
    public override IEnumerator Setup()
    {
        if (base.initialized)
        {
            yield break;
        }
        if (!SaveSlotManager.HasActiveSlot)
        {
            Plugin.Logger.LogDebug("SulfurSave_Threadsafe.Setup: no active save slot, skipping file init.");
            base.startupState = SulfurSave.StartupStates.Ok;
            base.initialized = true;
        }
        else
        {
	        while (!SetupImp()) ;
        }
    }
    
    private bool SetupImp()
	{
		if (initialized)
		{
			return false;
		}
		Plugin.Logger.LogDebug($"Setting up SulfurSave for slot {SaveSlotManager.ActiveSlotIndex}");
		loadResult = SulfurSave.LoadResult.Ok;
		saveSettings = ConstructES3Settings(SaveSlotManager.GetActiveSaveFile() + ".ap");
		if (_newFileRequired && ES3.FileExists(saveSettings))
		{
			ES3.DeleteFile(saveSettings);
			_newFileRequired = false;
		}
		if (!ES3.FileExists(saveSettings))
		{
			loadResult = SulfurSave.LoadResult.NewFile;
			Plugin.Logger.LogDebug("No SaveData file found, creating new one.");
		}
		if (loadResult == SulfurSave.LoadResult.Ok)
			loadResult = VerifyFile(suppressException: false, saveSettings);
		var purgeExistingFile = false;
		var shouldUpdateVersionInFile = false;
		startupState = SulfurSave.StartupStates.Ok;
		switch (loadResult)
		{
			case SulfurSave.LoadResult.NewFile:
				shouldUpdateVersionInFile = true;
				startupState = SulfurSave.StartupStates.Ok;
				break;
			case SulfurSave.LoadResult.Unsuccessful:
				purgeExistingFile = true;
				startupState = SulfurSave.StartupStates.PurgedCorrupted;
				break;
			case SulfurSave.LoadResult.WrongVersion:
				purgeExistingFile = true;
				startupState = SulfurSave.StartupStates.PurgedVersion;
				break;
		}
		if (purgeExistingFile)
		{
			ClearSaveData();
			shouldUpdateVersionInFile = true;
		}
		try
		{
			if (shouldUpdateVersionInFile) SaveVersion();
			initialized = true;
		}
		catch (Exception exception)
		{
			Plugin.Logger.LogError(exception);
		}
		return initialized;
	}
}