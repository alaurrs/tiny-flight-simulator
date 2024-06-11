//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MFlight.Demo
{
    public class Hud : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private MouseFlightController mouseFlight = null;
        [SerializeField] private Plane plane = null;

        [Header("HUD Elements")]
        [SerializeField] private RectTransform boresight = null;
        [SerializeField] private RectTransform mousePos = null;
        [SerializeField] private Text altitudeText = null;
        [SerializeField] private Text speedText = null;
        [SerializeField] private Text headingText = null;
        [SerializeField] private Text fuelText = null;
        [SerializeField] private Text verticalSpeedText = null;
        [SerializeField] private Text outOfFuelText = null;
        [SerializeField] private Button mainMenuButton = null;

        private Camera playerCam = null;

        private void Awake()
        {
            if (mouseFlight == null)
                Debug.LogError(name + ": Hud - Mouse Flight Controller not assigned!");

            if (plane == null)
                Debug.LogError(name + ": Hud - Plane not assigned!");

            playerCam = Camera.main;

            if (outOfFuelText != null)
            {
                outOfFuelText.gameObject.SetActive(false); // Masquer le texte par défaut
                mainMenuButton.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (mouseFlight == null || playerCam == null || plane == null)
                return;

            UpdateGraphics(mouseFlight);
            UpdateFlightData(plane);
        }

        private void UpdateGraphics(MouseFlightController controller)
        {
            if (boresight != null)
            {
                boresight.position = playerCam.WorldToScreenPoint(controller.BoresightPos);
                boresight.gameObject.SetActive(boresight.position.z > 1f);
            }

            if (mousePos != null)
            {
                mousePos.position = playerCam.WorldToScreenPoint(controller.MouseAimPos);
                mousePos.gameObject.SetActive(mousePos.position.z > 1f);
            }
        }

        private void UpdateFlightData(Plane plane)
        {
            if (plane.GetFuel() == 0f)
            {
                ShowOutOfFuelMessage();
                return;
            }

            if (altitudeText != null)
            {
                altitudeText.text = "Alt.   " + plane.GetAltitude().ToString("F0") + " m";
            }

            if (speedText != null)
            {
                speedText.text = "Speed   " + plane.GetSpeed().ToString("F0") + " m/s";
            }

            if (headingText != null)
            {
                headingText.text = "Heading   " + plane.GetHeading().ToString("F0") + "°";
            }

            if (fuelText != null)
            {
                fuelText.text = "Fuel  " + plane.GetFuel().ToString("F0");
            }

            if (verticalSpeedText != null)
            {
                verticalSpeedText.text = "V. Speed   " + plane.GetVerticalSpeed().ToString("F0") + " m/s";
            }
        }

        private void ShowOutOfFuelMessage()
        {
            if (outOfFuelText != null)
            {
                outOfFuelText.gameObject.SetActive(true);
                mainMenuButton.gameObject.SetActive(true);
            }

            if (altitudeText != null) altitudeText.gameObject.SetActive(false);
            if (speedText != null) speedText.gameObject.SetActive(false);
            if (headingText != null) headingText.gameObject.SetActive(false);
            if (fuelText != null) fuelText.gameObject.SetActive(false);
            if (verticalSpeedText != null) verticalSpeedText.gameObject.SetActive(false);
            if (boresight != null) boresight.gameObject.SetActive(false);
            if (mousePos != null) mousePos.gameObject.SetActive(false);
        }

        public void SetReferenceMouseFlight(MouseFlightController controller)
        {
            mouseFlight = controller;
        }

        public void SetReferencePlane(Plane planeReference)
        {
            plane = planeReference;
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene("Main Menu");
        }
    }
}
