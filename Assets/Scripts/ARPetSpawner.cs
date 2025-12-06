// ============================================================================
// File: ARPetSpawner.cs  
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPetSpawner : MonoBehaviour
{
    [Header("AR References")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject landscapePrefab;
    [SerializeField] private GameObject petPrefab;

    private readonly Dictionary<string, SpawnedSet> spawnedByImageName = new();

    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        }
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var trackedImage in args.added)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (var trackedImage in args.updated)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (var removedPair in args.removed)
        {
            DisableForImage(removedPair.Value);
        }
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.Tracking)
            return;

        string key = trackedImage.referenceImage.name;

        if (!spawnedByImageName.TryGetValue(key, out var spawned))
        {
            spawned = SpawnForImage(trackedImage);
            spawnedByImageName[key] = spawned;
        }

        if (spawned.landscapeInstance != null)
        {
            spawned.landscapeInstance.SetActive(true);
            spawned.landscapeInstance.transform.SetPositionAndRotation(
                trackedImage.transform.position,
                trackedImage.transform.rotation
            );
        }

        if (spawned.petInstance != null)
        {
            spawned.petInstance.gameObject.SetActive(true);
        }
    }

    private void DisableForImage(ARTrackedImage trackedImage)
    {
        string key = trackedImage.referenceImage.name;

        if (spawnedByImageName.TryGetValue(key, out var spawned))
        {
            if (spawned.landscapeInstance != null)
                spawned.landscapeInstance.SetActive(false);

            if (spawned.petInstance != null)
                spawned.petInstance.gameObject.SetActive(false);
        }
    }

    private SpawnedSet SpawnForImage(ARTrackedImage trackedImage)
    {
        var result = new SpawnedSet();

        if (landscapePrefab != null)
        {
            GameObject landscape = Instantiate(
                landscapePrefab,
                trackedImage.transform.position,
                trackedImage.transform.rotation,
                trackedImage.transform
            );

            result.landscapeInstance = landscape;

            Transform spawnPoint = landscape.transform.Find("PetSpawnPoint");
            Vector3 petPos = spawnPoint != null ? spawnPoint.position : landscape.transform.position;

            if (petPrefab != null)
            {
                GameObject petObj = Instantiate(
                    petPrefab,
                    petPos,
                    Quaternion.identity,
                    landscape.transform
                );

                result.petInstance = petObj.GetComponent<PetController>() ??
                                     petObj.AddComponent<PetController>();
            }
        }

        return result;
    }

    private struct SpawnedSet
    {
        public GameObject landscapeInstance;
        public PetController petInstance;
    }
}
