//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;

public class ControllerSound : MonoBehaviour
{
    public AudioSource audioSource;
    public MFlight.Demo.Plane thrustController;
    public float minPitch = 0.5f; // Minimum pitch when thrust is 0
    public float maxPitch = 2.0f; // Maximum pitch when thrust is 1
    public float minVolume = 0.1f; // Minimum volume when thrust is 0
    public float maxVolume = 1.0f; // Maximum volume when thrust is 1

    private float minThrust;
    private float maxThrust;

    void Start()
    {
        if (thrustController != null)
        {
            // Initialize minThrust and maxThrust from the thrustController
            minThrust = 0;
            maxThrust = 100;
        }
        else
        {
            Debug.LogError("ThrustController is not assigned!");
        }

        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned!");
        }
    }

    void Update()
    {
        if (thrustController != null && audioSource != null)
        {
            // Get the current thrust value
            float thrust = thrustController.throttle;

            // Normalize thrust to be between 0 and 1
            float normalizedThrust = Mathf.InverseLerp(minThrust, maxThrust, thrust);

            // Interpolate pitch and volume based on normalized thrust
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, normalizedThrust);
            audioSource.volume = Mathf.Lerp(minVolume, maxVolume, normalizedThrust);
        }
    }
}
