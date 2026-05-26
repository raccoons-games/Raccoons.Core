using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Raccoons.Editor
{
    [InitializeOnLoad]
    public static class RaccoonsDependencyInstaller
    {
        private static readonly (string Id, string Url)[] GitDependencies =
        {
            ("com.cysharp.unitask",
                "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11"),
            ("com.mathijsbakker.extenject",
                "https://github.com/Mathijs-Bakker/Extenject.git?path=UnityProject/Assets/Plugins/Zenject/Source")
        };

        public static bool InstallationComplete { get; private set; }
        public static event Action OnInstallationComplete;

        static RaccoonsDependencyInstaller()
        {
            var listRequest = Client.List();
            EditorApplication.update += WaitForList;

            void WaitForList()
            {
                if (!listRequest.IsCompleted) return;
                EditorApplication.update -= WaitForList;

                if (listRequest.Status != StatusCode.Success)
                {
                    MarkComplete();
                    return;
                }

                var installed = listRequest.Result.Select(p => p.name).ToHashSet();
                var missing = new Queue<(string Id, string Url)>(
                    GitDependencies.Where(d => !installed.Contains(d.Id)));

                if (missing.Count == 0)
                {
                    MarkComplete();
                    return;
                }

                InstallNext(missing);
            }
        }

        private static void InstallNext(Queue<(string Id, string Url)> queue)
        {
            if (queue.Count == 0)
            {
                MarkComplete();
                return;
            }

            var (id, url) = queue.Dequeue();
            Debug.Log($"[Raccoons Core] Installing dependency: {id}");
            var addRequest = Client.Add(url);

            EditorApplication.update += WaitForAdd;

            void WaitForAdd()
            {
                if (!addRequest.IsCompleted) return;
                EditorApplication.update -= WaitForAdd;

                if (addRequest.Status == StatusCode.Success)
                    Debug.Log($"[Raccoons Core] Installed: {id}");
                else
                    Debug.LogError($"[Raccoons Core] Failed to install {id}: {addRequest.Error?.message}");

                InstallNext(queue);
            }
        }

        private static void MarkComplete()
        {
            InstallationComplete = true;
            OnInstallationComplete?.Invoke();
        }
    }
}
