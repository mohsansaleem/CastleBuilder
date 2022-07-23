﻿using PG.Core.Installer;
using RSG;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace PG.Core.Command
{
    public class UnloadAllScenesExceptCommand : BaseCommand
    {
        [Inject] private readonly ISceneLoader _sceneLoader;

        protected override void ExecuteInternal(Signal signal)
        {
            UnloadAllScenesExceptSignal loadParams = signal as UnloadAllScenesExceptSignal;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(loadParams.Scene));

            IPromise lastPromise = null;

            int count = SceneManager.sceneCount;

            for (int i = 0; i < count; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.isLoaded && !scene.name.Equals(loadParams.Scene))
                {
                    lastPromise = lastPromise != null ? lastPromise.Then(() => _sceneLoader.UnloadScene(scene.name)) : _sceneLoader.UnloadScene(scene.name);
                }
            }

            //Add promise to resolve OnComplete
            if (lastPromise != null)
            {
                lastPromise.Done(
                    () =>
                    {
                        Debug.Log($"{this} , scene loading/unloading completed!");

                        loadParams.OnComplete?.Resolve();
                    },
                    exception =>
                    {
                        Debug.LogError("UnloadAllScenesExceptCommand.Execute: " + exception);

                        loadParams.OnComplete?.Reject(exception);
                    }
                );
            }
            else
            {
                Debug.Log($"{this} , no scenes loaded/unloaded!");

                loadParams.OnComplete?.Resolve();
            }
        }
    }
}
